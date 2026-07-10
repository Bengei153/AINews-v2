using AINews.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINews.Application.Content.Tags.Queries.GetTags;

public record GetTagsQuery : IRequest<List<TagDto>>;

public record TagDto(Guid Id, string Name, string Slug);

public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, List<TagDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTagsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Tags
            .OrderBy(t => t.Name)
            .Select(t => new TagDto(t.Id, t.Name, t.Slug))
            .ToListAsync(cancellationToken);
    }
}
