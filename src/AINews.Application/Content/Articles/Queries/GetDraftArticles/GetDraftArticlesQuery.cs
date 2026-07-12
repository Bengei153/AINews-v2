using AINews.Application.Common.Interfaces;
using AINews.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINews.Application.Content.Articles.Queries.GetDraftArticles;

public record GetDraftArticlesQuery : IRequest<List<DraftArticleDto>>;

public record DraftArticleDto(
    Guid Id,
    string Title,
    string Summary,
    string CategoryName,
    ArticleStatus Status,
    string SourceType,
    string? SourceName,
    string? SourceUrl,
    DateTimeOffset Created);

public class GetDraftArticlesQueryHandler : IRequestHandler<GetDraftArticlesQuery, List<DraftArticleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDraftArticlesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<DraftArticleDto>> Handle(GetDraftArticlesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Articles
            .Where(a => a.Status == ArticleStatus.Draft || a.Status == ArticleStatus.InReview)
            .OrderByDescending(a => a.Created)
            .Select(a => new DraftArticleDto(
                a.Id, a.Title, a.Summary, a.Category.Name, a.Status,
                a.SourceType.ToString(), a.SourceName, a.SourceUrl, a.Created))
            .ToListAsync(cancellationToken);
    }
}