using AINews.Domain.Common;

namespace AINews.Domain.Entities;

public class Tag : BaseAuditableEntity
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;

    private readonly List<ArticleTag> _articleTags = new();
    public IReadOnlyCollection<ArticleTag> ArticleTags => _articleTags.AsReadOnly();

    private Tag() { }

    public Tag(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }

    public void Rename(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }
}

/// <summary>Many-to-many join between Article and Tag.</summary>
public class ArticleTag : BaseEntity
{
    public Guid ArticleId { get; private set; }
    public Article Article { get; private set; } = default!;

    public Guid TagId { get; private set; }
    public Tag Tag { get; private set; } = default!;

    private ArticleTag() { }

    public ArticleTag(Guid articleId, Guid tagId)
    {
        ArticleId = articleId;
        TagId = tagId;
    }
}
