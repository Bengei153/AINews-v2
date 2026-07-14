using AINews.Application.Content.Newsletter.Commands.SendNewsletterIssue;
using AINews.Application.Content.Newsletter.Commands.Subscribe;
using AINews.Application.Content.Newsletter.Commands.Unsubscribe;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AINews.API.Controllers;

[ApiController]
[Route("api/newsletter")]
public class NewsletterController : ControllerBase
{
    private readonly ISender _mediator;

    public NewsletterController(ISender mediator) => _mediator = mediator;

    /// <summary>Subscribe to the newsletter by email — no account required (public).</summary>
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe(SubscribeCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>One-click unsubscribe via the token embedded in every newsletter footer (public).</summary>
    [HttpGet("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromQuery] Guid token, CancellationToken cancellationToken)
    {
        var found = await _mediator.Send(new UnsubscribeCommand(token), cancellationToken);
        return found ? Ok(new { message = "You've been unsubscribed." }) : NotFound();
    }

    /// <summary>Manually send a newsletter issue right now, using the most recently published articles (admin).</summary>
    [HttpPost("send")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<NewsletterSendResult>> Send([FromQuery] int articleCount = 5, CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(new SendNewsletterIssueCommand(ArticleCount: articleCount), cancellationToken));
}