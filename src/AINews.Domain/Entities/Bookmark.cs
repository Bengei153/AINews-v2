using AINews.Domain.Common;

namespace AINews.Domain.Entities;

/// <summary>A user saving an article to read later.</summary>
public class Bookmark : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid ArticleId { get; private set; }
    public Article Article { get; private set; } = default!;
    public DateTimeOffset SavedOn { get; private set; } = DateTimeOffset.UtcNow;

    private Bookmark() { }

    public Bookmark(Guid userId, Guid articleId)
    {
        UserId = userId;
        ArticleId = articleId;
    }
}
