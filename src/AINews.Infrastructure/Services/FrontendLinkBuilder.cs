using AINews.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace AINews.Infrastructure.Services;

public class FrontendSettings
{
    public const string SectionName = "Frontend";

    /// <summary>Your deployed frontend's base URL, e.g. https://aibrief.example.com (no trailing slash).</summary>
    public string BaseUrl { get; set; } = "http://localhost:3000";
}

public class ApiSettings
{
    public const string SectionName = "Api";

    /// <summary>
    /// This backend's own public URL (e.g. your Render URL,
    /// https://ainews-6bc7.onrender.com — no trailing slash). Needed so the
    /// backend can build links to its own /share/articles/{slug} route for
    /// use in emails and share buttons.
    /// </summary>
    public string PublicBaseUrl { get; set; } = "http://localhost:5254";

    /// <summary>
    /// Fallback image used for link previews when an article has no
    /// CoverImageUrl of its own. Point this at a real static image you've
    /// uploaded somewhere (e.g. your frontend's /public folder) —
    /// recommended size 1200x630.
    /// </summary>
    public string DefaultShareImageUrl { get; set; } = string.Empty;
}

public class FrontendLinkBuilder : IFrontendLinkBuilder
{
    private readonly string _frontendBaseUrl;
    private readonly string _apiBaseUrl;

    public FrontendLinkBuilder(IOptions<FrontendSettings> frontendSettings, IOptions<ApiSettings> apiSettings)
    {
        _frontendBaseUrl = frontendSettings.Value.BaseUrl.TrimEnd('/');
        _apiBaseUrl = apiSettings.Value.PublicBaseUrl.TrimEnd('/');
    }

    public string ArticleUrl(string slug) => $"{_frontendBaseUrl}/articles/{slug}";

    public string UnsubscribeUrl(Guid unsubscribeToken) => $"{_frontendBaseUrl}/unsubscribe?token={unsubscribeToken}";

    public string HomeUrl() => _frontendBaseUrl;

    public string ShareUrl(string slug) => $"{_apiBaseUrl}/share/articles/{slug}";
}