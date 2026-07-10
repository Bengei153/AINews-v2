using AINews.Application.Common.Exceptions;
using AINews.Application.Common.Interfaces;
using AINews.Domain.Entities;
using AINews.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINews.Application.Content.Articles.Queries.GetArticleBySlug;

public record GetArticleBySlugQuery(string Slug) : IRequest<ArticleDetailDto>;

public record ArticleDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string Summary,
    string Body,
    ContentPillar Pillar,
    string CategoryName,
    int ReadTimeMinutes,
    DateTimeOffset? PublishedOn,
    string? SourceName,
    string? SourceUrl,
    List<string> Tags);

public class GetArticleBySlugQueryHandler : IRequestHandler<GetArticleBySlugQuery, ArticleDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetArticleBySlugQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ArticleDetailDto> Handle(GetArticleBySlugQuery request, CancellationToken cancellationToken)
    {
        var article = await _context.Articles
            .Include(a => a.Category)
            .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
            .FirstOrDefaultAsync(a => a.Slug == request.Slug && a.Status == ArticleStatus.Published, cancellationToken);

        if (article is null)
        {
            throw new NotFoundException(nameof(Article), request.Slug);
        }

        return new ArticleDetailDto(
            article.Id,
            article.Title,
            article.Slug,
            article.Summary,
            article.Body,
            article.Pillar,
            article.Category.Name,
            article.ReadTimeMinutes,
            article.PublishedOn,
            article.SourceName,
            article.SourceUrl,
            article.ArticleTags.Select(at => at.Tag.Name).ToList());
    }
}
