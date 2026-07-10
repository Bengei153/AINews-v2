using AINews.Application.Common.Interfaces;
using AINews.Domain.Entities;
using FluentValidation;
using MediatR;

namespace AINews.Application.Content.AITools.Commands.CreateAITool;

public record CreateAIToolCommand(string Name, string Slug, string Description, string WebsiteUrl, string Pricing, string Tags, string? LogoUrl) : IRequest<Guid>;

public class CreateAIToolCommandValidator : AbstractValidator<CreateAIToolCommand>
{
    public CreateAIToolCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.WebsiteUrl).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    }
}

public class CreateAIToolCommandHandler : IRequestHandler<CreateAIToolCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateAIToolCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(CreateAIToolCommand request, CancellationToken cancellationToken)
    {
        var tool = new AITool(request.Name, request.Slug, request.Description, request.WebsiteUrl, request.Pricing, request.Tags, request.LogoUrl);
        _context.AITools.Add(tool);
        await _context.SaveChangesAsync(cancellationToken);
        return tool.Id;
    }
}
