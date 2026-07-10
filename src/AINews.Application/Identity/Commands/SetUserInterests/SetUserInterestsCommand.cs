using AINews.Application.Common.Exceptions;
using AINews.Application.Common.Interfaces;
using AINews.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINews.Application.Identity.Commands.SetUserInterests;

public record SetUserInterestsCommand(List<Guid> InterestIds) : IRequest;

public class SetUserInterestsCommandValidator : AbstractValidator<SetUserInterestsCommand>
{
    public SetUserInterestsCommandValidator()
    {
        RuleFor(x => x.InterestIds).NotNull();
    }
}

public class SetUserInterestsCommandHandler : IRequestHandler<SetUserInterestsCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SetUserInterestsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(SetUserInterestsCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            throw new ForbiddenAccessException();
        }

        var userId = _currentUser.UserId.Value;

        var validInterestIds = await _context.Interests
            .Where(i => request.InterestIds.Contains(i.Id) && i.IsActive)
            .Select(i => i.Id)
            .ToListAsync(cancellationToken);

        if (validInterestIds.Count != request.InterestIds.Distinct().Count())
        {
            throw new NotFoundException(nameof(Interest), string.Join(",", request.InterestIds));
        }

        var existing = await _context.UserInterests
            .Where(ui => ui.UserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var toRemove in existing.Where(e => !validInterestIds.Contains(e.InterestId)))
        {
            _context.UserInterests.Remove(toRemove);
        }

        var existingInterestIds = existing.Select(e => e.InterestId).ToHashSet();
        foreach (var interestId in validInterestIds.Where(id => !existingInterestIds.Contains(id)))
        {
            _context.UserInterests.Add(new UserInterest(userId, interestId));
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
