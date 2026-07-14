using AINews.Application.Common.Interfaces;
using AINews.Domain.Common;
using AINews.Domain.Entities;
using AINews.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AINews.Infrastructure.Persistence;

/// <summary>
/// The single EF Core DbContext for the whole solution. Combines ASP.NET
/// Identity's tables with the domain's own tables. Implements
/// IApplicationDbContext so the Application layer only ever sees the
/// abstraction, never EF Core types directly.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUser;
    private readonly IDateTime? _dateTime;
    private readonly IPublisher? _publisher;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUser = null,
        IDateTime? dateTime = null,
        IPublisher? publisher = null) : base(options)
    {
        _currentUser = currentUser;
        _dateTime = dateTime;
        _publisher = publisher;
    }

    public DbSet<Interest> Interests => Set<Interest>();
    public DbSet<UserInterest> UserInterests => Set<UserInterest>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleTag> ArticleTags => Set<ArticleTag>();
    public DbSet<AITool> AITools => Set<AITool>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<NewsletterIssue> NewsletterIssues => Set<NewsletterIssue>();
    public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();

        // Collect domain events before saving, dispatch after a successful save
        // so handlers only ever react to state that is actually persisted.
        var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count != 0)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_publisher is not null)
        {
            foreach (var entity in entitiesWithEvents)
            {
                var events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();
                foreach (var domainEvent in events)
                {
                    await _publisher.Publish(domainEvent, cancellationToken);
                }
            }
        }

        return result;
    }

    private void ApplyAuditInfo()
    {
        var now = _dateTime?.Now ?? DateTimeOffset.UtcNow;
        var userId = _currentUser?.UserId?.ToString();

        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Created = now;
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.LastModified = now;
                    entry.Entity.LastModifiedBy = userId;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModified = now;
                    entry.Entity.LastModifiedBy = userId;
                    break;
            }
        }
    }
}
