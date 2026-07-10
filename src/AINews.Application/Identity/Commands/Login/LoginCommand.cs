using AINews.Application.Common.Exceptions;
using AINews.Application.Common.Interfaces;
using AINews.Application.Identity.Commands.Register;
using FluentValidation;
using MediatR;

namespace AINews.Application.Identity.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResult>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(IIdentityService identityService, IJwtTokenService jwtTokenService)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, userId, fullName, role) = await _identityService.ValidateCredentialsAsync(request.Email, request.Password);

        if (!succeeded)
        {
            throw new AuthenticationFailedException();
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(userId, request.Email, fullName, role);
        var (refreshToken, expiresOn) = _jwtTokenService.GenerateRefreshToken();
        await _identityService.StoreRefreshTokenAsync(userId, refreshToken, expiresOn);

        return new AuthResult(userId, fullName, request.Email, role, accessToken, refreshToken, expiresOn);
    }
}
