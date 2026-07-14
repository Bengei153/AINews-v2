using AINews.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINews.Application.Content.Newsletter.Commands.Unsubscribe;

public record UnsubscribeCommand(Guid Token) : IRequest<bool>;

public class UnsubscribeCommandHandler : IRequestHandler<UnsubscribeCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UnsubscribeCommandHandler(IApplicationDbContext context) => _context = context;

    /// <returns>true if a matching subscriber was found and unsubscribed, false otherwise.</returns>
    public async Task<bool> Handle(UnsubscribeCommand request, CancellationToken cancellationToken)
    {
        var subscriber = await _context.NewsletterSubscribers
            .FirstOrDefaultAsync(s => s.UnsubscribeToken == request.Token, cancellationToken);

        if (subscriber is null) return false;

        subscriber.Unsubscribe();
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}