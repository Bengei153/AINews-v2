using System.Net;
using AINews.Application.Common.Exceptions;
using AINews.Application.Common.Interfaces;
using AINews.Application.Content.Articles.Queries.GetArticleShareInfo;
using AINews.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AINews.API.Controllers;

/// <summary>
/// Deliberately NOT under /api — this returns HTML, not JSON. Link-preview
/// crawlers (WhatsApp, Twitter, Slack, iMessage, LinkedIn, Facebook, ...)
/// fetch whatever URL you share and read &lt;meta&gt; tags from the raw HTML
/// response; they don't execute JavaScript. Since the frontend is a
/// client-rendered SPA, it can't serve per-article meta tags itself — this
/// endpoint exists purely to give crawlers something real to read, then
/// instantly sends actual human visitors on to the real frontend page.
/// </summary>
[ApiController]
[Route("share")]
public class ShareController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IFrontendLinkBuilder _linkBuilder;
    private readonly ApiSettings _apiSettings;

    public ShareController(ISender mediator, IFrontendLinkBuilder linkBuilder, IOptions<ApiSettings> apiSettings)
    {
        _mediator = mediator;
        _linkBuilder = linkBuilder;
        _apiSettings = apiSettings.Value;
    }

    [HttpGet("articles/{slug}")]
    public async Task<ContentResult> GetArticleShareCard(string slug, CancellationToken cancellationToken)
    {
        string title, description, imageUrl, redirectUrl;

        try
        {
            var info = await _mediator.Send(new GetArticleShareInfoQuery(slug), cancellationToken);
            title = info.Title;
            description = info.Summary;
            imageUrl = info.CoverImageUrl ?? _apiSettings.DefaultShareImageUrl;
            redirectUrl = _linkBuilder.ArticleUrl(slug);
        }
        catch (NotFoundException)
        {
            // Unknown/unpublished slug: send crawlers and humans alike to the
            // homepage rather than a broken share card.
            title = "AI Brief";
            description = "Learn AI. Use AI. Stay Ahead.";
            imageUrl = _apiSettings.DefaultShareImageUrl;
            redirectUrl = _linkBuilder.HomeUrl();
        }

        var imageTagOg = string.IsNullOrWhiteSpace(imageUrl) ? "" : $"<meta property=\"og:image\" content=\"{WebUtility.HtmlEncode(imageUrl)}\" />";
        var imageTagTwitter = string.IsNullOrWhiteSpace(imageUrl) ? "" : $"<meta name=\"twitter:image\" content=\"{WebUtility.HtmlEncode(imageUrl)}\" />";
        var twitterCardType = string.IsNullOrWhiteSpace(imageUrl) ? "summary" : "summary_large_image";

        var html = "<!doctype html>\n"
            + "<html lang=\"en\">\n"
            + "<head>\n"
            + "  <meta charset=\"utf-8\" />\n"
            + $"  <title>{WebUtility.HtmlEncode(title)}</title>\n"
            + "  <meta property=\"og:type\" content=\"article\" />\n"
            + "  <meta property=\"og:site_name\" content=\"AI Brief\" />\n"
            + $"  <meta property=\"og:title\" content=\"{WebUtility.HtmlEncode(title)}\" />\n"
            + $"  <meta property=\"og:description\" content=\"{WebUtility.HtmlEncode(description)}\" />\n"
            + $"  {imageTagOg}\n"
            + $"  <meta property=\"og:url\" content=\"{WebUtility.HtmlEncode(redirectUrl)}\" />\n"
            + $"  <meta name=\"twitter:card\" content=\"{twitterCardType}\" />\n"
            + $"  <meta name=\"twitter:title\" content=\"{WebUtility.HtmlEncode(title)}\" />\n"
            + $"  <meta name=\"twitter:description\" content=\"{WebUtility.HtmlEncode(description)}\" />\n"
            + $"  {imageTagTwitter}\n"
            + $"  <meta http-equiv=\"refresh\" content=\"0;url={WebUtility.HtmlEncode(redirectUrl)}\" />\n"
            + "</head>\n"
            + "<body>\n"
            + $"  <p>Redirecting to <a href=\"{WebUtility.HtmlEncode(redirectUrl)}\">{WebUtility.HtmlEncode(title)}</a>&hellip;</p>\n"
            + "</body>\n"
            + "</html>";

        return Content(html, "text/html; charset=utf-8");
    }
}