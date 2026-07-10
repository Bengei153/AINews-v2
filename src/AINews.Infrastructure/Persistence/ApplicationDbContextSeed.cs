using AINews.Domain.Entities;
using AINews.Domain.Enums;
using AINews.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AINews.Infrastructure.Persistence;

/// <summary>
/// Seeds the roles the app depends on (so RegisterCommandHandler's
/// AddToRoleAsync("User") never fails on a fresh database) plus a starter set
/// of Interests/Categories matching the product plan's content pillars.
/// Safe to run repeatedly — every step checks for existing data first.
/// </summary>
public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
            }
        }

        if (!await context.Interests.AnyAsync())
        {
            context.Interests.AddRange(
                new Interest("Programming", "programming", "Coding, software engineering, and dev tools."),
                new Interest("Research", "research", "Papers, breakthroughs, and academic AI news."),
                new Interest("Writing", "writing", "AI-assisted writing, editing, and content creation."),
                new Interest("Design", "design", "AI for design, creativity, and visual work."),
                new Interest("Business", "business", "AI for entrepreneurship, productivity, and work."),
                new Interest("Education", "education", "AI for studying, teaching, and learning."),
                new Interest("Productivity", "productivity", "Tools and workflows that save time."));
        }

        if (!await context.Categories.AnyAsync())
        {
            context.Categories.AddRange(
                new Category("AI for Students", "ai-for-students", ContentPillar.AIForStudents, "Study smarter with AI: notes, exam prep, research tools."),
                new Category("AI for Work", "ai-for-work", ContentPillar.AIForWork, "AI for Excel, email, coding, freelancing and more."),
                new Category("AI News", "ai-news", ContentPillar.AINews, "Only the AI stories that actually matter, explained."),
                new Category("AI Tool Spotlight", "ai-tool-spotlight", ContentPillar.AIToolSpotlight, "One useful AI tool, explained every day."),
                new Category("Future of AI", "future-of-ai", ContentPillar.FutureOfAI, "Jobs, skills, and where AI is heading next."));
        }

        await context.SaveChangesAsync();
    }
}
