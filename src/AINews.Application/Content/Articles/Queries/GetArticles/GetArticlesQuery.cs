using AINews.Application.Common.Interfaces;
using AINews.Application.Common.Models;
using AINews.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINews.Application.Content.Articles.Queries.GetArticles;

public record GetArticlesQuery(
    ContentPillar? Pillar = null,
    Guid? CategoryId = null,
    Guid? TagId = null,
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PaginatedList<ArticleSummaryDto>>;

public record ArticleSummaryDto(
    Guid Id,
    string Title,
    string Slug,
    string Summary,
    ContentPillar Pillar,
    string CategoryName,
    int ReadTimeMinutes,
    DateTimeOffset? PublishedOn,
    string? CoverImageUrl);

public class GetArticlesQueryHandler : IRequestHandler<GetArticlesQuery, PaginatedList<ArticleSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetArticlesQueryHandler(IApplicationDbContext context) => _context = context;

    public Task<PaginatedList<ArticleSummaryDto>> Handle(GetArticlesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Articles
            .Where(a => a.Status == ArticleStatus.Published)
            .AsQueryable();

        if (request.Pillar is not null)
            query = query.Where(a => a.Pillar == request.Pillar);

        if (request.CategoryId is not null)
            query = query.Where(a => a.CategoryId == request.CategoryId);

        if (request.TagId is not null)
            query = query.Where(a => a.ArticleTags.Any(at => at.TagId == request.TagId));

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(a => a.Title.Contains(request.Search) || a.Summary.Contains(request.Search));

        var projected = query
            .OrderByDescending(a => a.PublishedOn)
            .Select(a => new ArticleSummaryDto(
                a.Id, a.Title, a.Slug, a.Summary, a.Pillar, a.Category.Name, a.ReadTimeMinutes, a.PublishedOn, a.CoverImageUrl));

        return PaginatedList<ArticleSummaryDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
