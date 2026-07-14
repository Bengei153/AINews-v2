using System.Text;
using AINews.Application.Common.Interfaces;
using AINews.Domain.Entities;
using AINews.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AINews.Application.Content.Newsletter.Commands.SendNewsletterIssue;

public record SendNewsletterIssueCommand(string? Subject = null, int ArticleCount = 5) : IRequest<NewsletterSendResult>;

public record NewsletterSendResult(int ArticleCount, int RecipientCount, int FailedSends, List<string> Errors);

public class SendNewsletterIssueCommandHandler : IRequestHandler<SendNewsletterIssueCommand, NewsletterSendResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IFrontendLinkBuilder _linkBuilder;
    private readonly ILogger<SendNewsletterIssueCommandHandler> _logger;

    public SendNewsletterIssueCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        IFrontendLinkBuilder linkBuilder,
        ILogger<SendNewsletterIssueCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _linkBuilder = linkBuilder;
        _logger = logger;
    }

    public async Task<NewsletterSendResult> Handle(SendNewsletterIssueCommand request, CancellationToken cancellationToken)
    {
        var articles = await _context.Articles
            .Where(a => a.Status == ArticleStatus.Published)
            .OrderByDescending(a => a.PublishedOn)
            .Take(request.ArticleCount)
            .ToListAsync(cancellationToken);

        if (articles.Count == 0)
        {
            return new NewsletterSendResult(0, 0, 0, new List<string> { "No published articles to send — nothing was sent." });
        }

        var subscribers = await _context.NewsletterSubscribers
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);

        if (subscribers.Count == 0)
        {
            return new NewsletterSendResult(articles.Count, 0, 0, new List<string> { "No active subscribers — nothing was sent." });
        }

        var subject = request.Subject ?? $"AI Brief — {DateTimeOffset.UtcNow:MMMM d}: {articles.First().Title}";
        var errors = new List<string>();
        var failed = 0;

        foreach (var subscriber in subscribers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var html = BuildEmailHtml(articles, subscriber.UnsubscribeToken);
                await _emailService.SendAsync(subscriber.Email, subject, html, cancellationToken);
            }
            catch (Exception ex)
            {
                failed++;
                _logger.LogWarning(ex, "Failed to send newsletter to {Email}", subscriber.Email);
                errors.Add($"{subscriber.Email}: {ex.Message}");
            }
        }

        var issue = new NewsletterIssue(subject, DateTimeOffset.UtcNow);
        foreach (var article in articles)
        {
            issue.AddArticle(article.Id);
        }
        issue.MarkSent(subscribers.Count - failed);
        _context.NewsletterIssues.Add(issue);
        await _context.SaveChangesAsync(cancellationToken);

        return new NewsletterSendResult(articles.Count, subscribers.Count - failed, failed, errors);
    }

    private string BuildEmailHtml(List<Article> articles, Guid unsubscribeToken)
    {
        var sb = new StringBuilder();
        sb.Append("""
            <div style="font-family: 'Inter', Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #1c1917;">
              <div style="padding: 24px 0; border-bottom: 2px solid #1c1917;">
                <h1 style="font-family: Georgia, serif; font-size: 22px; margin: 0;">AI Brief</h1>
                <p style="font-size: 12px; color: #78716c; margin: 4px 0 0;">Learn AI. Use AI. Stay Ahead.</p>
              </div>
            """);

        foreach (var article in articles)
        {
            var articleUrl = _linkBuilder.ArticleUrl(article.Slug);
            sb.Append($"""
                <div style="padding: 20px 0; border-bottom: 1px solid #e7e5e4;">
                  <p style="font-size: 11px; text-transform: uppercase; letter-spacing: 0.05em; color: #059669; font-weight: 600; margin: 0 0 6px;">{System.Net.WebUtility.HtmlEncode(article.Pillar.ToString())}</p>
                  <h2 style="font-size: 18px; margin: 0 0 8px;"><a href="{articleUrl}" style="color: #1c1917; text-decoration: none;">{System.Net.WebUtility.HtmlEncode(article.Title)}</a></h2>
                  <p style="font-size: 14px; color: #44403c; line-height: 1.5; margin: 0 0 8px;">{System.Net.WebUtility.HtmlEncode(article.Summary)}</p>
                  <a href="{articleUrl}" style="font-size: 13px; color: #059669; font-weight: 600; text-decoration: none;">Read more &rarr;</a>
                </div>
                """);
        }

        var unsubscribeUrl = _linkBuilder.UnsubscribeUrl(unsubscribeToken);
        sb.Append($"""
              <div style="padding: 20px 0; text-align: center;">
                <p style="font-size: 11px; color: #a8a29e;">
                  You're receiving this because you subscribed to AI Brief.
                  <a href="{unsubscribeUrl}" style="color: #a8a29e;">Unsubscribe</a>
                </p>
              </div>
            </div>
            """);

        return sb.ToString();
    }
}