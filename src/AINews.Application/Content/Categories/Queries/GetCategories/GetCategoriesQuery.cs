using AINews.Application.Common.Interfaces;
using AINews.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINews.Application.Content.Categories.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<List<CategoryDto>>;

public record CategoryDto(Guid Id, string Name, string Slug, ContentPillar Pillar, string? Description);

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoriesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Slug, c.Pillar, c.Description))
            .ToListAsync(cancellationToken);
    }
}
