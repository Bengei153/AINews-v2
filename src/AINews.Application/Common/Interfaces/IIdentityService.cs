namespace AINews.Application.Common.Interfaces;

/// <summary>
/// Abstraction over ASP.NET Core Identity so the Application layer's Identity
/// commands/queries don't reference Microsoft.AspNetCore.Identity directly.
/// Implemented in AINews.Infrastructure.Identity.IdentityService.
/// </summary>
public interface IIdentityService
{
    Task<(bool Succeeded, string[] Errors, Guid UserId)> CreateUserAsync(string email, string fullName, string password);

    Task<(bool Succeeded, Guid UserId, string FullName, string Role)> ValidateCredentialsAsync(string email, string password);

    Task<string?> GetUserFullNameAsync(Guid userId);

    Task<string?> GetUserEmailAsync(Guid userId);

    Task<string> GetUserRoleAsync(Guid userId);

    Task<string> GenerateEmailConfirmationTokenAsync(Guid userId);

    Task<bool> ConfirmEmailAsync(Guid userId, string token);

    /// <summary>Persists a newly issued refresh token so it can be validated/revoked later.</summary>
    Task StoreRefreshTokenAsync(Guid userId, string refreshToken, DateTimeOffset expiresOn);

    /// <summary>Returns the owning user id if the refresh token is valid, unexpired and unrevoked; otherwise null.</summary>
    Task<Guid?> ValidateRefreshTokenAsync(string refreshToken);

    /// <summary>Revokes a refresh token (used on refresh-rotation and logout).</summary>
    Task RevokeRefreshTokenAsync(string refreshToken);
}

/// <summary>Abstraction for issuing/validating JWT access + refresh tokens.</summary>
public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, string email, string fullName, string role);

    (string Token, DateTimeOffset ExpiresOn) GenerateRefreshToken();
}

/// <summary>Provides the identity of the currently authenticated caller.</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}

/// <summary>Testable wrapper around DateTimeOffset.UtcNow.</summary>
public interface IDateTime
{
    DateTimeOffset Now { get; }
}

/// <summary>Abstraction for sending transactional/newsletter emails (implemented with MimeKit).</summary>
public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}

/// <summary>
/// Builds links that point at the frontend app (not the API) for use inside
/// emails — e.g. an article URL the reader clicks, or the one-click
/// unsubscribe link. Backed by a single configured frontend base URL.
/// </summary>
public interface IFrontendLinkBuilder
{
    string ArticleUrl(string slug);
    string UnsubscribeUrl(Guid unsubscribeToken);
    string HomeUrl();

    /// <summary>
    /// The link to actually hand out for sharing (newsletter, copy-link
    /// button). Points at the backend's own /share/articles/{slug} route,
    /// which serves real og:* meta tags for crawlers before redirecting
    /// humans to ArticleUrl — see NewsletterController-adjacent
    /// ShareController for why this indirection exists.
    /// </summary>
    string ShareUrl(string slug);
}
