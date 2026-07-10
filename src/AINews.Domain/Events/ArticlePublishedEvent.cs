using AINews.Domain.Common;
using AINews.Domain.Entities;

namespace AINews.Domain.Events;

/// <summary>
/// Raised when an Article transitions to Published. Handled in the
/// Application layer (e.g. to queue it into the next newsletter issue, or to
/// invalidate the "latest articles" cache).
/// </summary>
public class ArticlePublishedEvent : BaseDomainEvent
{
    public Article Article { get; }

    public ArticlePublishedEvent(Article article)
    {
        Article = article;
    }
}
