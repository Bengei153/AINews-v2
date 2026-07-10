using AINews.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AINews.Infrastructure.Persistence.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("Articles");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title).IsRequired().HasMaxLength(300);
        builder.Property(a => a.Slug).IsRequired().HasMaxLength(350);
        builder.Property(a => a.Summary).IsRequired().HasMaxLength(1000);
        builder.Property(a => a.Body).IsRequired();
        builder.Property(a => a.SourceUrl).HasMaxLength(1000);
        builder.Property(a => a.SourceName).HasMaxLength(200);

        builder.Property(a => a.Pillar).HasConversion<string>().HasMaxLength(40);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.SourceType).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(a => a.Slug).IsUnique();
        builder.HasIndex(a => new { a.Status, a.PublishedOn });
        builder.HasIndex(a => a.Pillar);

        // Domain events are runtime-only, never persisted.
        builder.Ignore(a => a.DomainEvents);

        // ArticleTags is exposed as a get-only IReadOnlyCollection backed by a
        // private List<ArticleTag> field (DDD encapsulation) — tell EF Core to
        // read/write through the field directly rather than the property.
        builder.Navigation(a => a.ArticleTags).HasField("_articleTags").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
