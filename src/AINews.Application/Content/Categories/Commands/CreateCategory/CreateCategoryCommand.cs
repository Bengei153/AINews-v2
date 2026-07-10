using AINews.Application.Common.Interfaces;
using AINews.Domain.Entities;
using AINews.Domain.Enums;
using FluentValidation;
using MediatR;

namespace AINews.Application.Content.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(string Name, string Slug, ContentPillar Pillar, string? Description) : IRequest<Guid>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(120);
    }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category(request.Name, request.Slug, request.Pillar, request.Description);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}
