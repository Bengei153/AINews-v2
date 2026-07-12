using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AINews.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AINews.Infrastructure.Services;

public class AnthropicSettings
{
    public const string SectionName = "Anthropic";

    /// <summary>Paste your Anthropic API key here (or better, set it via user-secrets/env var — see README).</summary>
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "claude-sonnet-4-5";
    public int MaxTokens { get; set; } = 1024;
}

/// <summary>
/// The "AI Service" from the product plan: takes a raw news item and asks
/// Claude to turn it into a title, a short summary, a rewritten body, a
/// suggested content pillar/category, and tags — all as strict JSON so it
/// can be parsed straight into an Article draft.
/// </summary>
public class AnthropicContentService : IAiContentService
{
    private readonly HttpClient _httpClient;
    private readonly AnthropicSettings _settings;
    private readonly ILogger<AnthropicContentService> _logger;

    public AnthropicContentService(HttpClient httpClient, IOptions<AnthropicSettings> settings, ILogger<AnthropicContentService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<AiDraftResult> SummarizeAsync(RawNewsItem item, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException(
                "Anthropic:ApiKey is not configured. Set it in appsettings.Development.json, " +
                "user-secrets, or the ANTHROPIC__APIKEY environment variable before running news ingestion.");
        }

        var prompt = BuildPrompt(item);

        var requestBody = new
        {
            model = _settings.Model,
            max_tokens = _settings.MaxTokens,
            messages = new[] { new { role = "user", content = prompt } }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", _settings.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Anthropic API call failed ({Status}): {Body}", response.StatusCode, responseBody);
            throw new InvalidOperationException($"Anthropic API returned {response.StatusCode}.");
        }

        return ParseResponse(responseBody, item);
    }

    private static string BuildPrompt(RawNewsItem item)
    {
        return $$"""
            You are the editorial AI for "AI Brief", a newsletter for students and
            young professionals who want to learn AI, use AI, and stay ahead.

            Turn the raw news item below into a short, clear article draft.
            Explain what happened, why it matters, and who should care —
            don't just restate the source.

            Raw item:
            Title: {{item.Title}}
            Source: {{item.SourceName}}
            Content: {{item.Content}}

            Respond with ONLY a single JSON object (no markdown fences, no
            commentary before or after) with exactly these fields:
            {
              "title": "a clear, specific headline, under 100 characters",
              "summary": "a 1-3 sentence summary, under 300 characters",
              "body": "a 150-300 word article body in markdown, written for the audience above",
              "pillar": "one of: AIForStudents, AIForWork, AINews, AIToolSpotlight, FutureOfAI",
              "categorySlug": "one of: ai-for-students, ai-for-work, ai-news, ai-tool-spotlight, future-of-ai",
              "tags": ["2-4 short lowercase tags, e.g. gpt-6, study-tools"]
            }
            """;
    }

    private static AiDraftResult ParseResponse(string responseBody, RawNewsItem item)
    {
        using var doc = JsonDocument.Parse(responseBody);
        var text = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? throw new InvalidOperationException("Anthropic response had no text content.");

        // Claude was asked for raw JSON, but strip code fences defensively in
        // case the model wraps it anyway.
        var jsonText = text.Trim().Trim('`').Trim();
        if (jsonText.StartsWith("json", StringComparison.OrdinalIgnoreCase))
        {
            jsonText = jsonText[4..].Trim();
        }

        using var parsed = JsonDocument.Parse(jsonText);
        var root = parsed.RootElement;

        var tags = root.TryGetProperty("tags", out var tagsEl)
            ? tagsEl.EnumerateArray().Select(t => t.GetString() ?? string.Empty).Where(t => t.Length > 0).ToList()
            : new List<string>();

        return new AiDraftResult(
            Title: root.GetProperty("title").GetString() ?? item.Title,
            Summary: root.GetProperty("summary").GetString() ?? item.Summary ?? string.Empty,
            Body: root.GetProperty("body").GetString() ?? item.Content,
            SuggestedPillar: root.TryGetProperty("pillar", out var p) ? p.GetString() ?? "AINews" : "AINews",
            SuggestedCategorySlug: root.TryGetProperty("categorySlug", out var c) ? c.GetString() ?? "ai-news" : "ai-news",
            SuggestedTags: tags);
    }
}