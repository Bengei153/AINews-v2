using AINews.Domain.Common;
using AINews.Domain.Enums;
using AINews.Domain.Events;
using AINews.Domain.Exceptions;

namespace AINews.Domain.Entities;

/// <summary>
/// Aggregate root for a piece of content (news item, tutorial, tool spotlight,
/// etc). Encapsulates the draft -> review -> publish workflow described in the
/// "News Aggregator" and "AI Queue" sections of the product plan.
/// </summary>
public class Article : BaseAuditableEntity
{
    public string Title { get; private set; } = default!;
    public string Slug { get; private set; } = default!;

    /// <summary>Short AI- or editor-written summary shown in lists/newsletter.</summary>
    public string Summary { get; private set; } = default!;

    /// <summary>Full article body (markdown).</summary>
    public string Body { get; private set; } = default!;

    public string? SourceUrl { get; private set; }
    public string? SourceName { get; private set; }
    public ArticleSourceType SourceType { get; private set; } = ArticleSourceType.Original;

    public ContentPillar Pillar { get; private set; }
    public ArticleStatus Status { get; private set; } = ArticleStatus.Draft;

    public int ReadTimeMinutes { get; private set; }
    public DateTimeOffset? PublishedOn { get; private set; }

    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = default!;

    /// <summary>Id of the identity user who authored/curated this (nullable for AI-generated drafts).</summary>
    public Guid? AuthorId { get; private set; }

    private readonly List<ArticleTag> _articleTags = new();
    public IReadOnlyCollection<ArticleTag> ArticleTags => _articleTags.AsReadOnly();

    private Article() { }

    public static Article CreateDraft(
        string title,
        string slug,
        string summary,
        string body,
        ContentPillar pillar,
        Guid categoryId,
        Guid? authorId,
        ArticleSourceType sourceType = ArticleSourceType.Original,
        string? sourceUrl = null,
        string? sourceName = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Article title is required.");
        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("Article body is required.");

        var article = new Article
        {
            Title = title,
            Slug = slug,
            Summary = summary,
            Body = body,
            Pillar = pillar,
            CategoryId = categoryId,
            AuthorId = authorId,
            SourceType = sourceType,
            SourceUrl = sourceUrl,
            SourceName = sourceName,
            Status = ArticleStatus.Draft,
            ReadTimeMinutes = EstimateReadTime(body)
        };

        return article;
    }

    public void UpdateContent(string title, string slug, string summary, string body, ContentPillar pillar, Guid categoryId)
    {
        EnsureEditable();
        Title = title;
        Slug = slug;
        Summary = summary;
        Body = body;
        Pillar = pillar;
        CategoryId = categoryId;
        ReadTimeMinutes = EstimateReadTime(body);
    }

    public void SubmitForReview()
    {
        if (Status != ArticleStatus.Draft)
            throw new DomainException("Only draft articles can be submitted for review.");
        Status = ArticleStatus.InReview;
    }

    public void Publish()
    {
        if (Status is ArticleStatus.Published)
            throw new DomainException("Article is already published.");
        Status = ArticleStatus.Published;
        PublishedOn = DateTimeOffset.UtcNow;
        AddDomainEvent(new ArticlePublishedEvent(this));
    }

    public void Archive()
    {
        Status = ArticleStatus.Archived;
    }

    public void AddTag(Guid tagId)
    {
        if (_articleTags.Any(at => at.TagId == tagId)) return;
        _articleTags.Add(new ArticleTag(Id, tagId));
    }

    public void RemoveTag(Guid tagId)
    {
        var existing = _articleTags.FirstOrDefault(at => at.TagId == tagId);
        if (existing is not null) _articleTags.Remove(existing);
    }

    private void EnsureEditable()
    {
        if (Status == ArticleStatus.Published)
            throw new DomainException("Published articles cannot be edited directly; archive and create a new draft instead.");
    }

    private static int EstimateReadTime(string body)
    {
        const int wordsPerMinute = 200;
        var wordCount = body.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, (int)Math.Ceiling(wordCount / (double)wordsPerMinute));
    }
}
