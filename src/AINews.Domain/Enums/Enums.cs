namespace AINews.Domain.Enums;

public enum SubscriptionType
{
    Free = 0,
    Premium = 1
}

public enum ArticleStatus
{
    Draft = 0,
    InReview = 1,
    Published = 2,
    Archived = 3
}

/// <summary>
/// The content pillars from the product vision: AI for Students, AI for Work,
/// AI News, AI Tool Spotlight, Future of AI. Stored on Article/Category so the
/// homepage and personalization engine can filter by pillar independent of
/// the free-form Category/Tag taxonomy.
/// </summary>
public enum ContentPillar
{
    AIForStudents = 0,
    AIForWork = 1,
    AINews = 2,
    AIToolSpotlight = 3,
    FutureOfAI = 4
}

public enum ArticleSourceType
{
    Original = 0,
    Aggregated = 1,
    AIGenerated = 2
}
