using AINews.Domain.Common;

namespace AINews.Domain.Entities;

/// <summary>
/// Scaffolding for Phase 4 (Newsletter). Kept minimal for now: a newsletter
/// issue bundles a set of published articles and tracks send/engagement
/// stats once the Newsletter service is built out.
/// </summary>
public class NewsletterIssue : BaseAuditableEntity
{
    public string Subject { get; private set; } = default!;
    public DateTimeOffset ScheduledFor { get; private set; }
    public DateTimeOffset? SentOn { get; private set; }
    public int RecipientCount { get; private set; }
    public int OpenCount { get; private set; }
    public int ClickCount { get; private set; }

    private readonly List<Guid> _articleIds = new();
    public IReadOnlyCollection<Guid> ArticleIds => _articleIds.AsReadOnly();

    private NewsletterIssue() { }

    public NewsletterIssue(string subject, DateTimeOffset scheduledFor)
    {
        Subject = subject;
        ScheduledFor = scheduledFor;
    }

    public void AddArticle(Guid articleId)
    {
        if (!_articleIds.Contains(articleId)) _articleIds.Add(articleId);
    }

    public void MarkSent(int recipientCount)
    {
        SentOn = DateTimeOffset.UtcNow;
        RecipientCount = recipientCount;
    }

    public void RecordOpen() => OpenCount++;
    public void RecordClick() => ClickCount++;
}
