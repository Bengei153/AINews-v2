using AINews.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINews.Application.Content.AITools.Queries.GetAITools;

public record GetAIToolsQuery(bool FeaturedOnly = false) : IRequest<List<AIToolDto>>;

public record AIToolDto(Guid Id, string Name, string Slug, string Description, string WebsiteUrl, string Pricing, double Rating, string Tags, bool IsFeaturedToday);

public class GetAIToolsQueryHandler : IRequestHandler<GetAIToolsQuery, List<AIToolDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAIToolsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<AIToolDto>> Handle(GetAIToolsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AITools.AsQueryable();

        if (request.FeaturedOnly)
            query = query.Where(t => t.IsFeaturedToday);

        return await query
            .OrderByDescending(t => t.Rating)
            .Select(t => new AIToolDto(t.Id, t.Name, t.Slug, t.Description, t.WebsiteUrl, t.Pricing, t.Rating, t.Tags, t.IsFeaturedToday))
            .ToListAsync(cancellationToken);
    }
}
