using AINews.Application.Common.Interfaces;
using AINews.Domain.Entities;
using FluentValidation;
using MediatR;

namespace AINews.Application.Content.Tags.Commands.CreateTag;

public record CreateTagCommand(string Name, string Slug) : IRequest<Guid>;

public class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(80);
    }
}

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateTagCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = new Tag(request.Name, request.Slug);
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync(cancellationToken);
        return tag.Id;
    }
}
