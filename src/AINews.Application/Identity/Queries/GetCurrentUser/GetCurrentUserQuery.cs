using AINews.Application.Common.Exceptions;
using AINews.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINews.Application.Identity.Queries.GetCurrentUser;

public record GetCurrentUserQuery : IRequest<CurrentUserDto>;

public record CurrentUserDto(Guid Id, string Email, string FullName, string Role, List<string> Interests);

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public GetCurrentUserQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser, IIdentityService identityService)
    {
        _context = context;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            throw new ForbiddenAccessException();
        }

        var userId = _currentUser.UserId.Value;

        var fullName = await _identityService.GetUserFullNameAsync(userId) ?? string.Empty;
        var role = await _identityService.GetUserRoleAsync(userId);

        var interests = await _context.UserInterests
            .Where(ui => ui.UserId == userId)
            .Select(ui => ui.Interest.Name)
            .ToListAsync(cancellationToken);

        return new CurrentUserDto(userId, _currentUser.Email ?? string.Empty, fullName, role, interests);
    }
}
