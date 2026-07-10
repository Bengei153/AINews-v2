using Microsoft.AspNetCore.Identity;

namespace AINews.Infrastructure.Identity;

/// <summary>
/// The ASP.NET Core Identity user. Deliberately kept in the Infrastructure
/// layer (not Domain) per Clean Architecture: Domain entities reference users
/// only by Guid (see Domain.Entities.Bookmark/UserInterest), so the framework
/// dependency on Identity never leaks inward.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = default!;
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
    public string SubscriptionType { get; set; } = "Free";
}

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}

/// <summary>
/// Persisted refresh token, one row per issued token, enabling rotation and
/// revocation (logout / reuse detection). Owned by Infrastructure since it's
/// purely an authentication concern, not a Domain concept.
/// </summary>
public class RefreshTokenEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Token { get; set; } = default!;
    public DateTimeOffset ExpiresOn { get; set; }
    public DateTimeOffset? RevokedOn { get; set; }
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;

    public bool IsActive => RevokedOn is null && ExpiresOn > DateTimeOffset.UtcNow;
}
