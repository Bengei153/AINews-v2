using AINews.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using ValidationException = AINews.Application.Common.Exceptions.ValidationException;

namespace AINews.Application.Identity.Commands.Register;

public record RegisterCommand(string FullName, string Email, string Password) : IRequest<AuthResult>;

public record AuthResult(Guid UserId, string FullName, string Email, string Role, string AccessToken, string RefreshToken, DateTimeOffset RefreshTokenExpiresOn);

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.");
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCommandHandler(IIdentityService identityService, IJwtTokenService jwtTokenService)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, errors, userId) = await _identityService.CreateUserAsync(request.Email, request.FullName, request.Password);

        if (!succeeded)
        {
            throw new ValidationException(
                errors.Select(e => new FluentValidation.Results.ValidationFailure(nameof(request.Email), e)));
        }

        const string role = "User";
        var accessToken = _jwtTokenService.GenerateAccessToken(userId, request.Email, request.FullName, role);
        var (refreshToken, expiresOn) = _jwtTokenService.GenerateRefreshToken();
        await _identityService.StoreRefreshTokenAsync(userId, refreshToken, expiresOn);

        return new AuthResult(userId, request.FullName, request.Email, role, accessToken, refreshToken, expiresOn);
    }
}
