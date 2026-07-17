using AINews.Application.Common.Exceptions;
using AINews.Application.Common.Interfaces;
using AINews.Domain.Entities;
using AINews.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINews.Application.Content.Articles.Queries.GetArticleShareInfo;

public record GetArticleShareInfoQuery(string Slug) : IRequest<ArticleShareInfoDto>;

public record ArticleShareInfoDto(string Title, string Summary, string? CoverImageUrl);

public class GetArticleShareInfoQueryHandler : IRequestHandler<GetArticleShareInfoQuery, ArticleShareInfoDto>
{
    private readonly IApplicationDbContext _context;

    public GetArticleShareInfoQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ArticleShareInfoDto> Handle(GetArticleShareInfoQuery request, CancellationToken cancellationToken)
    {
        var article = await _context.Articles
            .Where(a => a.Slug == request.Slug && a.Status == ArticleStatus.Published)
            .Select(a => new ArticleShareInfoDto(a.Title, a.Summary, a.CoverImageUrl))
            .FirstOrDefaultAsync(cancellationToken);

        if (article is null)
        {
            throw new NotFoundException(nameof(Article), request.Slug);
        }

        return article;
    }
}