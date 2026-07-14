using AINews.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace AINews.Infrastructure.Services;

public class FrontendSettings
{
    public const string SectionName = "Frontend";

    /// <summary>Your deployed frontend's base URL, e.g. https://aibrief.example.com (no trailing slash).</summary>
    public string BaseUrl { get; set; } = "http://localhost:3000";
}

public class FrontendLinkBuilder : IFrontendLinkBuilder
{
    private readonly string _baseUrl;

    public FrontendLinkBuilder(IOptions<FrontendSettings> settings)
    {
        _baseUrl = settings.Value.BaseUrl.TrimEnd('/');
    }

    public string ArticleUrl(string slug) => $"{_baseUrl}/articles/{slug}";

    public string UnsubscribeUrl(Guid unsubscribeToken) => $"{_baseUrl}/unsubscribe?token={unsubscribeToken}";
}