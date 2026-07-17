using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AINews.Application.Common.Interfaces;

/// <summary>A single unprocessed item pulled from a news source, before AI summarization.</summary>
public record RawNewsItem(string Title, string? Summary, string Content, string SourceUrl, string SourceName, DateTimeOffset PublishedOn, string? ImageUrl = null);

/// <summary>
/// Fetches candidate news items to consider for ingestion. Implemented by
/// RssNewsSourceService (Infrastructure) reading feeds from configuration.
/// </summary>
public interface INewsSourceService
{
    Task<List<RawNewsItem>> FetchLatestAsync(CancellationToken cancellationToken = default);
}

/// <summary>The structured result of asking an LLM to turn a raw item into a publishable draft.</summary>
public record AiDraftResult(
    string Title,
    string Summary,
    string Body,
    string SuggestedPillar,
    string SuggestedCategorySlug,
    List<string> SuggestedTags);

/// <summary>
/// Wraps whichever LLM provider does the actual summarization/categorization
/// ("AI Service" from the product plan). Implemented by
/// AnthropicContentService (Infrastructure) — swap providers by writing a
/// new implementation of this one interface.
/// </summary>
public interface IAiContentService
{
    Task<AiDraftResult> SummarizeAsync(RawNewsItem item, CancellationToken cancellationToken = default);
}