using AINews.Application.Content.Tags.Commands.CreateTag;
using AINews.Application.Content.Tags.Queries.GetTags;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AINews.API.Controllers;

[ApiController]
[Route("api/tags")]
public class TagsController : ControllerBase
{
    private readonly ISender _mediator;

    public TagsController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<TagDto>>> GetTags(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetTagsQuery(), cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Guid>> Create(CreateTagCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));
}
