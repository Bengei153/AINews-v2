using AINews.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AINews.Application.Common.Interfaces;

/// <summary>
/// Abstraction over EF Core's DbContext so the Application layer (handlers)
/// never references Microsoft.EntityFrameworkCore.Sqlite/Npgsql directly.
/// Implemented by AINews.Infrastructure.Persistence.ApplicationDbContext.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Interest> Interests { get; }
    DbSet<UserInterest> UserInterests { get; }
    DbSet<Category> Categories { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Article> Articles { get; }
    DbSet<ArticleTag> ArticleTags { get; }
    DbSet<AITool> AITools { get; }
    DbSet<Bookmark> Bookmarks { get; }
    DbSet<NewsletterIssue> NewsletterIssues { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
