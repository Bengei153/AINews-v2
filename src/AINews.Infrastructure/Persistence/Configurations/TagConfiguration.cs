using AINews.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AINews.Infrastructure.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(60);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(80);
        builder.HasIndex(t => t.Slug).IsUnique();
        builder.Navigation(t => t.ArticleTags).HasField("_articleTags").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class ArticleTagConfiguration : IEntityTypeConfiguration<ArticleTag>
{
    public void Configure(EntityTypeBuilder<ArticleTag> builder)
    {
        builder.ToTable("ArticleTags");
        builder.HasKey(at => at.Id);

        builder.HasOne(at => at.Article)
            .WithMany(a => a.ArticleTags)
            .HasForeignKey(at => at.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(at => at.Tag)
            .WithMany(t => t.ArticleTags)
            .HasForeignKey(at => at.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(at => new { at.ArticleId, at.TagId }).IsUnique();
    }
}
