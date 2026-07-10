using AINews.Domain.Common;
using AINews.Domain.Enums;

namespace AINews.Domain.Entities;

public class Category : BaseAuditableEntity
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? Description { get; private set; }
    public ContentPillar Pillar { get; private set; }

    private readonly List<Article> _articles = new();
    public IReadOnlyCollection<Article> Articles => _articles.AsReadOnly();

    private Category() { }

    public Category(string name, string slug, ContentPillar pillar, string? description = null)
    {
        Name = name;
        Slug = slug;
        Pillar = pillar;
        Description = description;
    }

    public void Update(string name, string slug, ContentPillar pillar, string? description)
    {
        Name = name;
        Slug = slug;
        Pillar = pillar;
        Description = description;
    }
}
