using AINews.Application.Common.Exceptions;
using AINews.Application.Common.Interfaces;
using AINews.Domain.Entities;
using AINews.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINews.Application.Content.Articles.Commands.CreateArticle;

public record CreateArticleCommand(
    string Title,
    string Summary,
    string Body,
    ContentPillar Pillar,
    Guid CategoryId,
    List<Guid>? TagIds,
    ArticleSourceType SourceType,
    string? SourceUrl,
    string? SourceName) : IRequest<Guid>;

public class CreateArticleCommandValidator : AbstractValidator<CreateArticleCommand>
{
    public CreateArticleCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Body).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public class CreateArticleCommandHandler : IRequestHandler<CreateArticleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateArticleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new NotFoundException(nameof(Category), request.CategoryId);
        }

        var slug = SlugGenerator.FromTitle(request.Title);

        var article = Article.CreateDraft(
            request.Title,
            slug,
            request.Summary,
            request.Body,
            request.Pillar,
            request.CategoryId,
            _currentUser.UserId,
            request.SourceType,
            request.SourceUrl,
            request.SourceName);

        foreach (var tagId in request.TagIds ?? new List<Guid>())
        {
            article.AddTag(tagId);
        }

        _context.Articles.Add(article);
        await _context.SaveChangesAsync(cancellationToken);

        return article.Id;
    }
}

internal static class SlugGenerator
{
    public static string FromTitle(string title)
    {
        var slug = title.Trim().ToLowerInvariant();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-").Trim('-');
        return $"{slug}-{Guid.NewGuid().ToString()[..8]}";
    }
}
