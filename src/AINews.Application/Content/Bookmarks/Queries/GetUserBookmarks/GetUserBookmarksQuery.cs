using AINews.Application.Common.Exceptions;
using AINews.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINews.Application.Content.Bookmarks.Queries.GetUserBookmarks;

public record GetUserBookmarksQuery : IRequest<List<BookmarkedArticleDto>>;

public record BookmarkedArticleDto(Guid ArticleId, string Title, string Slug, string Summary, DateTimeOffset SavedOn);

public class GetUserBookmarksQueryHandler : IRequestHandler<GetUserBookmarksQuery, List<BookmarkedArticleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUserBookmarksQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<BookmarkedArticleDto>> Handle(GetUserBookmarksQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            throw new ForbiddenAccessException();
        }

        return await _context.Bookmarks
            .Where(b => b.UserId == _currentUser.UserId)
            .OrderByDescending(b => b.SavedOn)
            .Select(b => new BookmarkedArticleDto(b.ArticleId, b.Article.Title, b.Article.Slug, b.Article.Summary, b.SavedOn))
            .ToListAsync(cancellationToken);
    }
}
