using AINews.Application.Content.Bookmarks.Commands.ToggleBookmark;
using AINews.Application.Content.Bookmarks.Queries.GetUserBookmarks;
using AINews.Application.Identity.Commands.SetUserInterests;
using AINews.Application.Identity.Queries.GetCurrentUser;
using AINews.Application.Identity.Queries.GetInterests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AINews.API.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly ISender _mediator;

    public MeController(ISender mediator) => _mediator = mediator;

    /// <summary>Get the authenticated user's profile and chosen interests.</summary>
    [HttpGet]
    public async Task<ActionResult<CurrentUserDto>> GetCurrentUser(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetCurrentUserQuery(), cancellationToken));

    /// <summary>Replace the authenticated user's personalization interests.</summary>
    [HttpPut("interests")]
    public async Task<IActionResult> SetInterests(SetUserInterestsCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>List the authenticated user's saved (bookmarked) articles.</summary>
    [HttpGet("bookmarks")]
    public async Task<ActionResult<List<BookmarkedArticleDto>>> GetBookmarks(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetUserBookmarksQuery(), cancellationToken));

    /// <summary>Bookmark an article, or remove the bookmark if it already exists.</summary>
    [HttpPost("bookmarks/{articleId:guid}")]
    public async Task<ActionResult<bool>> ToggleBookmark(Guid articleId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ToggleBookmarkCommand(articleId), cancellationToken));
}

[ApiController]
[Route("api/interests")]
public class InterestsController : ControllerBase
{
    private readonly ISender _mediator;

    public InterestsController(ISender mediator) => _mediator = mediator;

    /// <summary>List all interests a user can personalize their feed with (public).</summary>
    [HttpGet]
    public async Task<ActionResult<List<InterestDto>>> GetInterests(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetInterestsQuery(), cancellationToken));
}
