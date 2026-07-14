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

        // ArticleIds is exposed as a get-only IReadOnlyCollection backed by a
        // private List<Guid> field. Map that field directly to a Postgres
        // uuid[] array column (Npgsql supports primitive collections
        // natively) — EF binds "_articleIds" to the real CLR field of that
        // name rather than creating a true shadow property, since one exists.
        // The public read-only property itself is excluded from mapping to
        // avoid EF trying to map both the field and the property.
        builder.Ignore(n => n.ArticleIds);
        builder.Property<List<Guid>>("_articleIds").HasColumnName("ArticleIds");
    }
}

public class NewsletterSubscriberConfiguration : IEntityTypeConfiguration<NewsletterSubscriber>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscriber> builder)
    {
        builder.ToTable("NewsletterSubscribers");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(s => s.Email).IsUnique();
        builder.HasIndex(s => s.UnsubscribeToken).IsUnique();
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
