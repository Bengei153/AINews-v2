using AINews.Domain.Entities;
using AINews.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AINews.Infrastructure.Persistence.Configurations;

public class AIToolConfiguration : IEntityTypeConfiguration<AITool>
{
    public void Configure(EntityTypeBuilder<AITool> builder)
    {
        builder.ToTable("AITools");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(150);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(180);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(2000);
        builder.Property(t => t.WebsiteUrl).IsRequired().HasMaxLength(500);
        builder.Property(t => t.Pricing).HasMaxLength(200);
        builder.Property(t => t.Tags).HasMaxLength(500);
        builder.HasIndex(t => t.Slug).IsUnique();
    }
}

public class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
{
    public void Configure(EntityTypeBuilder<Bookmark> builder)
    {
        builder.ToTable("Bookmarks");
        builder.HasKey(b => b.Id);

        builder.HasOne(b => b.Article)
            .WithMany()
            .HasForeignKey(b => b.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => new { b.UserId, b.ArticleId }).IsUnique();
    }
}

public class NewsletterIssueConfiguration : IEntityTypeConfiguration<NewsletterIssue>
{
    public void Configure(EntityTypeBuilder<NewsletterIssue> builder)
    {
        builder.ToTable("NewsletterIssues");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Subject).IsRequired().HasMaxLength(300);

        // ArticleIds is a Phase 4 stub (Newsletter service not built out yet);
        // not persisted until that feature gets a proper join table.
        builder.Ignore(n => n.ArticleIds);
    }
}

public class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Token).IsRequired().HasMaxLength(500);
        builder.HasIndex(r => r.Token).IsUnique();
        builder.HasIndex(r => r.UserId);
    }
}
