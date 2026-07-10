using Hangfire.Dashboard;

namespace AINews.API.Middleware;

/// <summary>Only lets authenticated users in the "Admin" role open the Hangfire dashboard.</summary>
public class AdminsOnlyDashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true && httpContext.User.IsInRole("Admin");
    }
}
