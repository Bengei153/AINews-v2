using AINews.Application.Common.Models;
using AINews.Application.Content.Articles.Commands.CreateArticle;
using AINews.Application.Content.Articles.Commands.PublishArticle;
using AINews.Application.Content.Articles.Queries.GetArticleBySlug;
using AINews.Application.Content.Articles.Queries.GetArticles;
using AINews.Application.Content.Articles.Queries.GetDraftArticles;
using AINews.Application.Content.NewsIngestion.Commands.RunNewsIngestion;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AINews.API.Controllers;

[ApiController]
[Route("api/articles")]
public class ArticlesController : ControllerBase
{
    private readonly ISender _mediator;

    public ArticlesController(ISender mediator) => _mediator = mediator;

    /// <summary>Browse published articles, optionally filtered by pillar/category/tag/search.</summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedList<ArticleSummaryDto>>> GetArticles([FromQuery] GetArticlesQuery query, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(query, cancellationToken));

    /// <summary>List draft/in-review articles awaiting approval — the "AI Queue" (admin).</summary>
    [HttpGet("drafts")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<DraftArticleDto>>> GetDrafts(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetDraftArticlesQuery(), cancellationToken));

    /// <summary>Manually trigger one pass of the news ingestion pipeline right now (admin, for testing).</summary>
    [HttpPost("ingest-news")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<NewsIngestionResult>> IngestNews(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new RunNewsIngestionCommand(), cancellationToken));

    /// <summary>Get a single published article by its slug.</summary>
    [HttpGet("{slug}")]
    public async Task<ActionResult<ArticleDetailDto>> GetBySlug(string slug, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetArticleBySlugQuery(slug), cancellationToken));

    /// <summary>Create a new draft article (curator/admin — feeds the "AI Queue" review workflow).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Guid>> Create(CreateArticleCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    /// <summary>Publish a draft/in-review article (admin).</summary>
    [HttpPost("{articleId:guid}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Publish(Guid articleId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new PublishArticleCommand(articleId), cancellationToken);
        return NoContent();
    }
}
