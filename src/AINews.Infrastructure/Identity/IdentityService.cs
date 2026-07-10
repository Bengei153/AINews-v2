using AINews.Application.Common.Interfaces;
using AINews.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AINews.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private const string DefaultRole = "User";

    public IdentityService(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<(bool Succeeded, string[] Errors, Guid UserId)> CreateUserAsync(string email, string fullName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, DefaultRole);
        }

        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray(), user.Id);
    }

    public async Task<(bool Succeeded, Guid UserId, string FullName, string Role)> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return (false, Guid.Empty, string.Empty, string.Empty);
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
        {
            return (false, Guid.Empty, string.Empty, string.Empty);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? DefaultRole;

        return (true, user.Id, user.FullName, role);
    }

    public async Task<string?> GetUserFullNameAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.FullName;
    }

    public async Task<string?> GetUserEmailAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.Email;
    }

    public async Task<string> GetUserRoleAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return DefaultRole;

        var roles = await _userManager.GetRolesAsync(user);
        return roles.FirstOrDefault() ?? DefaultRole;
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User not found.");
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<bool> ConfirmEmailAsync(Guid userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task StoreRefreshTokenAsync(Guid userId, string refreshToken, DateTimeOffset expiresOn)
    {
        _dbContext.RefreshTokens.Add(new RefreshTokenEntity
        {
            UserId = userId,
            Token = refreshToken,
            ExpiresOn = expiresOn
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Guid?> ValidateRefreshTokenAsync(string refreshToken)
    {
        var token = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        return token is { IsActive: true } ? token.UserId : null;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (token is null) return;

        token.RevokedOn = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
    }
}
