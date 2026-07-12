using AINews.API.Middleware;
using AINews.Application;
using AINews.Infrastructure;
using AINews.Infrastructure.Persistence;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using AINews.Application.Content.NewsIngestion.Commands.RunNewsIngestion;
using MediatR;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---- Logging -----------------------------------------------------------
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// ---- Layers --------------------------------------------------------------
// Each layer owns its own DI registration (see DependencyInjection.cs in
// AINews.Application and AINews.Infrastructure). Program.cs just wires the
// three rings of Clean Architecture together plus the API-only concerns
// below (controllers, Swagger, CORS).
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Brief API",
        Version = "v1",
        Description = "Learn AI. Use AI. Stay Ahead. — backend API for the AI Brief platform."
    });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste a JWT access token: Bearer {token}",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    options.AddSecurityDefinition("Bearer", jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000", "http://localhost:5173" };

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

// ---- Middleware pipeline --------------------------------------------------
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Brief API v1"));
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Hangfire dashboard for monitoring the News Collector / AI Summarizer /
// Email Scheduler background jobs. Locked down beyond Development — replace
// AllowAllDashboardAuthFilter with a real auth filter before shipping.
app.MapHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization = app.Environment.IsDevelopment()
        ? Array.Empty<IDashboardAuthorizationFilter>()
        : new IDashboardAuthorizationFilter[] { new AdminsOnlyDashboardAuthFilter() }
});

// Apply pending EF Core migrations automatically on startup in Development
// so `docker compose up` gives you a ready-to-use database. In production,
// prefer running migrations as an explicit deploy step.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AINews.Infrastructure.Identity.ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AINews.Infrastructure.Identity.ApplicationUser>>();
    await ApplicationDbContextSeed.SeedAsync(db, roleManager, userManager, builder.Configuration);
}

// Recurring job: News Collector + AI Summarizer pipeline. Schedule comes
// from NewsIngestion:CronSchedule in appsettings (default: 7am daily).
// You can also trigger it on demand via POST /api/articles/ingest-news, or
// click "Trigger now" for this job on the /jobs dashboard.
var newsIngestionSettings = app.Services.GetRequiredService<IOptions<AINews.Infrastructure.Services.NewsIngestionSettings>>().Value;
RecurringJob.AddOrUpdate<ISender>(
    "news-ingestion",
    sender => sender.Send(new RunNewsIngestionCommand(), CancellationToken.None),
    newsIngestionSettings.CronSchedule);

app.Run();

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program { }
