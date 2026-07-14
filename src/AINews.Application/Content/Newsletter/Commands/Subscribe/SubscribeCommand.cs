using AINews.Application.Common.Interfaces;
using AINews.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINews.Application.Content.Newsletter.Commands.Subscribe;

public record SubscribeCommand(string Email) : IRequest;

public class SubscribeCommandValidator : AbstractValidator<SubscribeCommand>
{
    public SubscribeCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}

public class SubscribeCommandHandler : IRequestHandler<SubscribeCommand>
{
    private readonly IApplicationDbContext _context;

    public SubscribeCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(SubscribeCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existing = await _context.NewsletterSubscribers
            .FirstOrDefaultAsync(s => s.Email == email, cancellationToken);

        if (existing is not null)
        {
            // Already subscribed, or re-subscribing after a previous unsubscribe.
            // Either way this is a no-op success from the caller's point of view.
            if (!existing.IsActive)
            {
                existing.Resubscribe();
                await _context.SaveChangesAsync(cancellationToken);
            }
            return;
        }

        _context.NewsletterSubscribers.Add(new NewsletterSubscriber(email));
        await _context.SaveChangesAsync(cancellationToken);
    }
}