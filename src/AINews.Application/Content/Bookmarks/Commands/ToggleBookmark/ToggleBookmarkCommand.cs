using AINews.Application.Common.Exceptions;
using AINews.Application.Common.Interfaces;
using AINews.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINews.Application.Content.Bookmarks.Commands.ToggleBookmark;

public record ToggleBookmarkCommand(Guid ArticleId) : IRequest<bool>;

public class ToggleBookmarkCommandHandler : IRequestHandler<ToggleBookmarkCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ToggleBookmarkCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    /// <returns>true if the article is now bookmarked, false if it was just removed.</returns>
    public async Task<bool> Handle(ToggleBookmarkCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            throw new ForbiddenAccessException();
        }

        var userId = _currentUser.UserId.Value;

        var articleExists = await _context.Articles.AnyAsync(a => a.Id == request.ArticleId, cancellationToken);
        if (!articleExists)
        {
            throw new NotFoundException(nameof(Article), request.ArticleId);
        }

        var existing = await _context.Bookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.ArticleId == request.ArticleId, cancellationToken);

        if (existing is not null)
        {
            _context.Bookmarks.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }

        _context.Bookmarks.Add(new Bookmark(userId, request.ArticleId));
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
