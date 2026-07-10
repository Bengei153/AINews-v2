using AINews.Application.Common.Exceptions;
using AINews.Application.Common.Interfaces;
using AINews.Domain.Entities;
using MediatR;

namespace AINews.Application.Content.Articles.Commands.PublishArticle;

public record PublishArticleCommand(Guid ArticleId) : IRequest;

public class PublishArticleCommandHandler : IRequestHandler<PublishArticleCommand>
{
    private readonly IApplicationDbContext _context;

    public PublishArticleCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(PublishArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await _context.Articles.FindAsync(new object[] { request.ArticleId }, cancellationToken);
        if (article is null)
        {
            throw new NotFoundException(nameof(Article), request.ArticleId);
        }

        article.Publish();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
