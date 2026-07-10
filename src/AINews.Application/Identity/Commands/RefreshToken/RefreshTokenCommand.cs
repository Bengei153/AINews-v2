using AINews.Application.Common.Exceptions;
using AINews.Application.Common.Interfaces;
using AINews.Application.Identity.Commands.Register;
using FluentValidation;
using MediatR;

namespace AINews.Application.Identity.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResult>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenCommandHandler(IIdentityService identityService, IJwtTokenService jwtTokenService)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = await _identityService.ValidateRefreshTokenAsync(request.RefreshToken);
        if (userId is null)
        {
            throw new AuthenticationFailedException("Refresh token is invalid or has expired.");
        }

        // Rotate: revoke the old token and issue a new pair.
        await _identityService.RevokeRefreshTokenAsync(request.RefreshToken);

        var fullName = await _identityService.GetUserFullNameAsync(userId.Value) ?? string.Empty;
        var email = await _identityService.GetUserEmailAsync(userId.Value) ?? string.Empty;
        var role = await _identityService.GetUserRoleAsync(userId.Value);

        var accessToken = _jwtTokenService.GenerateAccessToken(userId.Value, email, fullName, role);
        var (newRefreshToken, expiresOn) = _jwtTokenService.GenerateRefreshToken();
        await _identityService.StoreRefreshTokenAsync(userId.Value, newRefreshToken, expiresOn);

        return new AuthResult(userId.Value, fullName, email, role, accessToken, newRefreshToken, expiresOn);
    }
}
