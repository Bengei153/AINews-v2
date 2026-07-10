using AINews.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AINews.Infrastructure.Persistence.Configurations;

public class InterestConfiguration : IEntityTypeConfiguration<Interest>
{
    public void Configure(EntityTypeBuilder<Interest> builder)
    {
        builder.ToTable("Interests");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Name).IsRequired().HasMaxLength(100);
        builder.Property(i => i.Slug).IsRequired().HasMaxLength(120);
        builder.Property(i => i.Description).HasMaxLength(500);
        builder.HasIndex(i => i.Slug).IsUnique();
    }
}

public class UserInterestConfiguration : IEntityTypeConfiguration<UserInterest>
{
    public void Configure(EntityTypeBuilder<UserInterest> builder)
    {
        builder.ToTable("UserInterests");
        builder.HasKey(ui => ui.Id);
        builder.HasOne(ui => ui.Interest)
            .WithMany()
            .HasForeignKey(ui => ui.InterestId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(ui => new { ui.UserId, ui.InterestId }).IsUnique();
    }
}
