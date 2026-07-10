using AINews.Application.Identity.Commands.Login;
using AINews.Application.Identity.Commands.RefreshToken;
using AINews.Application.Identity.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AINews.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator) => _mediator = mediator;

    /// <summary>Create a new account and receive an access/refresh token pair.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Register(RegisterCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    /// <summary>Exchange email/password for an access/refresh token pair.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login(LoginCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    /// <summary>Exchange a valid refresh token for a new access/refresh token pair (rotation).</summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResult>> Refresh(RefreshTokenCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));
}
