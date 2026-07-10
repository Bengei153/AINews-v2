using AINews.Domain.Common;

namespace AINews.Domain.Entities;

/// <summary>
/// A topical interest a user can subscribe to (Programming, Research, Writing,
/// Design, Business, ...). Drives personalization of the newsletter/homepage.
/// </summary>
public class Interest : BaseAuditableEntity
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Interest() { }

    public Interest(string name, string slug, string? description = null)
    {
        Name = name;
        Slug = slug;
        Description = description;
    }

    public void Update(string name, string slug, string? description)
    {
        Name = name;
        Slug = slug;
        Description = description;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

/// <summary>
/// Join entity between an identity user (owned by the Infrastructure layer)
/// and an Interest. The Domain layer stores only the user's Id (Guid) so it
/// never has to depend on ASP.NET Core Identity types.
/// </summary>
public class UserInterest : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid InterestId { get; private set; }
    public Interest Interest { get; private set; } = default!;

    private UserInterest() { }

    public UserInterest(Guid userId, Guid interestId)
    {
        UserId = userId;
        InterestId = interestId;
    }
}
