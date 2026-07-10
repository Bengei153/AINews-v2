using AINews.Domain.Common;

namespace AINews.Domain.Entities;

public class AITool : BaseAuditableEntity
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string WebsiteUrl { get; private set; } = default!;
    public string Pricing { get; private set; } = default!;
    public double Rating { get; private set; }
    public string? LogoUrl { get; private set; }

    /// <summary>Comma-separated or normalized tag list for quick filtering (e.g. "writing,students").</summary>
    public string Tags { get; private set; } = string.Empty;

    public bool IsFeaturedToday { get; private set; }

    private AITool() { }

    public AITool(string name, string slug, string description, string websiteUrl, string pricing, string tags = "", string? logoUrl = null)
    {
        Name = name;
        Slug = slug;
        Description = description;
        WebsiteUrl = websiteUrl;
        Pricing = pricing;
        Tags = tags;
        LogoUrl = logoUrl;
    }

    public void Update(string name, string slug, string description, string websiteUrl, string pricing, string tags, string? logoUrl)
    {
        Name = name;
        Slug = slug;
        Description = description;
        WebsiteUrl = websiteUrl;
        Pricing = pricing;
        Tags = tags;
        LogoUrl = logoUrl;
    }

    public void Rate(double rating) => Rating = Math.Clamp(rating, 0, 5);

    public void MarkAsFeaturedToday() => IsFeaturedToday = true;
    public void UnmarkFeatured() => IsFeaturedToday = false;
}
