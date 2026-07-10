using AINews.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINews.Application.Identity.Queries.GetInterests;

public record GetInterestsQuery : IRequest<List<InterestDto>>;

public record InterestDto(Guid Id, string Name, string Slug, string? Description);

public class GetInterestsQueryHandler : IRequestHandler<GetInterestsQuery, List<InterestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetInterestsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<InterestDto>> Handle(GetInterestsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Interests
            .Where(i => i.IsActive)
            .OrderBy(i => i.Name)
            .Select(i => new InterestDto(i.Id, i.Name, i.Slug, i.Description))
            .ToListAsync(cancellationToken);
    }
}
