using AINews.Domain.Common;

namespace AINews.Domain.Entities;

/// <summary>
/// Someone who wants the newsletter by email, without creating a full
/// account. Deliberately separate from ApplicationUser: registration asks
/// for a password and gets you bookmarks/interests; subscribing just asks
/// for an email. Lower friction, matches how most people actually want to
/// join a newsletter.
/// </summary>
public class NewsletterSubscriber : BaseEntity
{
    public string Email { get; private set; } = default!;
    public DateTimeOffset SubscribedOn { get; private set; } = DateTimeOffset.UtcNow;
    public Guid UnsubscribeToken { get; private set; } = Guid.NewGuid();
    public bool IsActive { get; private set; } = true;

    private NewsletterSubscriber() { }

    public NewsletterSubscriber(string email)
    {
        Email = email.Trim().ToLowerInvariant();
    }

    public void Unsubscribe() => IsActive = false;

    public void Resubscribe() => IsActive = true;
}