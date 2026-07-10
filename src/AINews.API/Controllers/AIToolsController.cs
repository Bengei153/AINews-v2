using AINews.Application.Content.AITools.Commands.CreateAITool;
using AINews.Application.Content.AITools.Queries.GetAITools;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AINews.API.Controllers;

[ApiController]
[Route("api/ai-tools")]
public class AIToolsController : ControllerBase
{
    private readonly ISender _mediator;

    public AIToolsController(ISender mediator) => _mediator = mediator;

    /// <summary>Browse the AI tool directory; pass featuredOnly=true for just "today's tool spotlight".</summary>
    [HttpGet]
    public async Task<ActionResult<List<AIToolDto>>> GetTools([FromQuery] bool featuredOnly, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetAIToolsQuery(featuredOnly), cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Guid>> Create(CreateAIToolCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));
}
