using AINews.Application.Common.Interfaces;
using AINews.Domain.Entities;
using AINews.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AINews.Application.Content.NewsIngestion.Commands.RunNewsIngestion;

/// <summary>
/// Runs one pass of the News Collector → AI Summarizer pipeline from the
/// product plan. Safe to run repeatedly/on a schedule — items are deduped
/// by SourceUrl, so re-fetching the same RSS feed won't create duplicates.
/// </summary>
public record RunNewsIngestionCommand : IRequest<NewsIngestionResult>;

public record NewsIngestionResult(int ItemsFetched, int DraftsCreated, int Skipped, List<string> Errors);

public class RunNewsIngestionCommandHandler : IRequestHandler<RunNewsIngestionCommand, NewsIngestionResult>
{
    private readonly INewsSourceService _newsSource;
    private readonly IAiContentService _aiContent;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RunNewsIngestionCommandHandler> _logger;

    public RunNewsIngestionCommandHandler(
        INewsSourceService newsSource,
        IAiContentService aiContent,
        IApplicationDbContext context,
        ILogger<RunNewsIngestionCommandHandler> logger)
    {
        _newsSource = newsSource;
        _aiContent = aiContent;
        _context = context;
        _logger = logger;
    }

    public async Task<NewsIngestionResult> Handle(RunNewsIngestionCommand request, CancellationToken cancellationToken)
    {
        var rawItems = await _newsSource.FetchLatestAsync(cancellationToken);
        var errors = new List<string>();
        var created = 0;
        var skipped = 0;

        // Load categories once — the AI suggests a category by slug, we map
        // it back to a real Category row (falling back to "AI News" if the
        // model suggests something that doesn't exist).
        var categories = await _context.Categories.ToListAsync(cancellationToken);
        var fallbackCategory = categories.FirstOrDefault(c => c.Slug == "ai-news") ?? categories.FirstOrDefault();

        foreach (var item in rawItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var alreadyExists = await _context.Articles.AnyAsync(a => a.SourceUrl == item.SourceUrl, cancellationToken);
            if (alreadyExists)
            {
                skipped++;
                continue;
            }

            try
            {
                var draft = await _aiContent.SummarizeAsync(item, cancellationToken);

                var category = categories.FirstOrDefault(c => c.Slug == draft.SuggestedCategorySlug) ?? fallbackCategory;
                if (category is null)
                {
                    errors.Add($"No categories exist yet — skipped '{item.Title}'. Seed at least one Category first.");
                    continue;
                }

                if (!Enum.TryParse<ContentPillar>(draft.SuggestedPillar, out var pillar))
                {
                    pillar = category.Pillar;
                }

                var slug = SlugFrom(draft.Title);

                var article = Article.CreateDraft(
                    draft.Title,
                    slug,
                    draft.Summary,
                    draft.Body,
                    pillar,
                    category.Id,
                    authorId: null, // AI-generated draft, no human author yet
                    sourceType: ArticleSourceType.AIGenerated,
                    sourceUrl: item.SourceUrl,
                    sourceName: item.SourceName,
                    coverImageUrl: item.ImageUrl);

                _context.Articles.Add(article);
                created++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to summarize/create draft for {SourceUrl}", item.SourceUrl);
                errors.Add($"'{item.Title}': {ex.Message}");
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new NewsIngestionResult(rawItems.Count, created, skipped, errors);
    }

    private static string SlugFrom(string title)
    {
        var slug = title.Trim().ToLowerInvariant();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-").Trim('-');
        return $"{slug}-{Guid.NewGuid().ToString()[..8]}";
    }
}