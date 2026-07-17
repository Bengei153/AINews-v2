using System.ServiceModel.Syndication;
using System.Xml;
using AINews.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AINews.Infrastructure.Services;

public class NewsIngestionSettings
{
    public const string SectionName = "NewsIngestion";

    /// <summary>RSS/Atom feed URLs to pull AI news from. Add/remove freely.</summary>
    public List<string> RssFeedUrls { get; set; } = new();

    /// <summary>Max items pulled per feed per run, to bound cost/time.</summary>
    public int MaxItemsPerFeed { get; set; } = 5;

    /// <summary>Cron expression for the recurring Hangfire job (default: 7am daily).</summary>
    public string CronSchedule { get; set; } = "0 7 * * *";
}

public class RssNewsSourceService : INewsSourceService
{
    private readonly HttpClient _httpClient;
    private readonly NewsIngestionSettings _settings;
    private readonly ILogger<RssNewsSourceService> _logger;

    public RssNewsSourceService(HttpClient httpClient, IOptions<NewsIngestionSettings> settings, ILogger<RssNewsSourceService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<List<RawNewsItem>> FetchLatestAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<RawNewsItem>();

        foreach (var feedUrl in _settings.RssFeedUrls)
        {
            try
            {
                using var stream = await _httpClient.GetStreamAsync(feedUrl, cancellationToken);
                using var xmlReader = XmlReader.Create(stream);
                var feed = SyndicationFeed.Load(xmlReader);

                if (feed is null) continue;

                var sourceName = string.IsNullOrWhiteSpace(feed.Title?.Text) ? feedUrl : feed.Title.Text;

                foreach (var feedItem in feed.Items.Take(_settings.MaxItemsPerFeed))
                {
                    var link = feedItem.Links.FirstOrDefault()?.Uri?.ToString() ?? feedItem.Id;
                    if (string.IsNullOrWhiteSpace(link)) continue;

                    var summary = feedItem.Summary?.Text ?? string.Empty;
                    var content = (feedItem.Content as TextSyndicationContent)?.Text ?? summary;

                    // Many feeds attach a cover image as an "enclosure" link
                    // with an image MIME type. Not every feed has this — it's
                    // a nice-to-have, not something to rely on.
                    var imageUrl = feedItem.Links
                        .FirstOrDefault(l => l.RelationshipType == "enclosure" && l.MediaType?.StartsWith("image/") == true)
                        ?.Uri?.ToString();

                    results.Add(new RawNewsItem(
                        Title: feedItem.Title?.Text ?? "(untitled)",
                        Summary: summary,
                        Content: string.IsNullOrWhiteSpace(content) ? summary : content,
                        SourceUrl: link,
                        SourceName: sourceName,
                        PublishedOn: feedItem.PublishDate.UtcDateTime == default
                            ? DateTimeOffset.UtcNow
                            : feedItem.PublishDate,
                        ImageUrl: imageUrl));
                }
            }
            catch (Exception ex)
            {
                // One bad/unreachable feed shouldn't stop the others from being processed.
                _logger.LogWarning(ex, "Failed to fetch or parse RSS feed {FeedUrl}", feedUrl);
            }
        }

        return results;
    }
}