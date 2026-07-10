# AI Brief — Backend

> **Learn AI. Use AI. Stay Ahead.**
> The go-to AI resource for students and young professionals — news that
> matters, tools worth using, and a daily nudge on how AI makes today easier.

This is the **backend only** (ASP.NET Core 8). The React frontend described
in the product plan will live in a separate project and talk to this API.

---

## Architecture

Clean Architecture + DDD, four projects, dependencies only point inward:

```
AINews.API            → controllers, Program.cs, Swagger, auth wiring, Docker
   ↓ depends on
AINews.Infrastructure  → EF Core, PostgreSQL, ASP.NET Identity, JWT, Redis,
                          Hangfire, SMTP (MailKit/MimeKit)
   ↓ depends on
AINews.Application     → CQRS via MediatR, FluentValidation, DTOs, the
                          interfaces Infrastructure implements
   ↓ depends on
AINews.Domain          → entities, value objects, domain events. Zero
                          external dependencies — this is the core.
```

Key patterns used, matching the roadmap's "DDD + Clean Architecture" goal:

- **Domain layer has no framework references.** `Article`, `Category`,
  `Interest`, etc. are plain C# with private setters and factory methods
  (`Article.CreateDraft(...)`) that enforce invariants — the anemic-model
  trap is avoided on purpose.
- **Users are referenced by `Guid` only** from Domain entities
  (`Bookmark.UserId`, `UserInterest.UserId`). ASP.NET Identity's
  `ApplicationUser` lives in `Infrastructure.Identity`, so Domain never
  depends on the auth framework.
- **CQRS with MediatR.** Every use case is a `Command` or `Query` +
  `Handler` under `Application/<Feature>/Commands|Queries/<UseCase>/`. This
  is what "Content Service", "Identity Service" etc. from the plan compile
  down to inside a single deployable for now — see *Splitting into
  microservices later* below.
- **Domain events.** `Article.Publish()` raises `ArticlePublishedEvent`,
  dispatched by `ApplicationDbContext.SaveChangesAsync` after a successful
  commit. This is the hook point for "queue this into the next newsletter
  issue" once the Newsletter service is built out.
- **A single `ApplicationDbContext`** implements the Application layer's
  `IApplicationDbContext` interface, so handlers depend on an abstraction,
  not EF Core directly (easy to unit test with an in-memory/fake context).

---

## What's implemented (Phase 1 + slice of Phase 2)

Matches the roadmap's Phase 1 deliverable — *"Users can register, log in,
and access protected resources"* — plus enough of Phase 2 (Content) to prove
the pattern end-to-end:

- **Auth**: register, login, refresh-token rotation (persisted + revocable),
  role-based JWTs (`Admin` / `User`).
- **Users**: profile (`GET /api/me`), personalization interests
  (`GET /api/interests`, `PUT /api/me/interests`).
- **Content**: Categories, Tags, Articles (draft → review → publish
  workflow, matches the "AI Queue" concept), Bookmarks, AI Tools directory
  (with a "featured today" spotlight flag for the homepage).
- **Cross-cutting**: global exception → HTTP status mapping, FluentValidation
  pipeline behaviour, Serilog request logging, health check endpoint,
  Swagger with a JWT auth button, CORS for the future frontend, Hangfire
  wired up and dashboarded at `/jobs` (ready for the News Collector / AI
  Summarizer / Email Scheduler workers from Phase 5 — no jobs registered
  yet, that's the next milestone).

### Not yet built (by design — next phases per the roadmap)

- AI Service (summarization, categorization, headline generation) — Phase 3.
- Newsletter Service (`NewsletterIssue` entity exists as a stub only;
  no send pipeline yet) — Phase 4.
- News Aggregator background job (dedupe → summarize → draft → review) —
  Phase 5.
- Admin dashboard analytics endpoints, payments/subscriptions — Phase 6+.

---

## Running it locally

**Prerequisites:** .NET 8 SDK, Docker (for Postgres/Redis), or a local
Postgres instance.

### Option A — everything in Docker

```bash
docker compose up --build
```

API comes up on `http://localhost:8080/swagger`. Postgres and Redis run in
their own containers. On first boot the API auto-applies EF Core migrations
and seeds roles + starter Interests/Categories (Development environment
only — see `Program.cs`).

### Option B — API on your machine, infra in Docker

```bash
docker compose up postgres redis -d
cd src/AINews.API
dotnet run
```

### First migration

No migrations are checked in yet (this sandbox has no .NET SDK to generate
them). Create the initial one before first run:

```bash
dotnet tool install --global dotnet-ef   # if you don't have it
cd src/AINews.API
dotnet ef migrations add InitialCreate -p ../AINews.Infrastructure -s .
dotnet ef database update -p ../AINews.Infrastructure -s .
```

(`ApplicationDbContextFactory` lets these commands run without a live
connection string — it falls back to the Development one if
`appsettings.json` isn't found.)

### Creating your first admin user

Registration always assigns the `User` role. Promote yourself to `Admin`
directly in the database after registering, e.g.:

```sql
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT u."Id", r."Id"
FROM "AspNetUsers" u, "AspNetRoles" r
WHERE u."Email" = 'you@example.com' AND r."Name" = 'Admin';
```

(A proper admin-invite flow belongs in Phase 6 — flagged, not forgotten.)

---

## Project layout

```
AINews.sln
docker-compose.yml
src/
  AINews.Domain/
    Common/BaseEntity.cs           — BaseEntity, BaseAuditableEntity, domain events
    Entities/                      — Article, Category, Tag, Interest, AITool, Bookmark, ...
    Enums/                         — ArticleStatus, ContentPillar, SubscriptionType
    Events/ArticlePublishedEvent.cs
    Exceptions/DomainException.cs
  AINews.Application/
    Common/Interfaces/             — IApplicationDbContext, IIdentityService, IJwtTokenService...
    Common/Behaviours/             — MediatR validation + logging pipeline
    Common/Models/PaginatedList.cs
    Identity/Commands|Queries/     — Register, Login, RefreshToken, SetUserInterests, ...
    Content/Articles|Categories|Tags|Bookmarks|AITools/Commands|Queries/
  AINews.Infrastructure/
    Identity/                      — ApplicationUser, JwtTokenService, IdentityService, ...
    Persistence/                   — ApplicationDbContext, EF Core Configurations, Seed
    Services/                      — EmailService (MailKit/MimeKit), DateTimeService
  AINews.API/
    Controllers/                   — Auth, Me, Interests, Articles, Categories, Tags, AITools
    Middleware/                    — global exception handler, Hangfire dashboard auth
    Program.cs, appsettings*.json, Dockerfile
```

---

## Splitting into microservices later

The plan's original diagram shows separate Identity/Content/Newsletter APIs
behind a gateway. This build keeps them as **Application-layer feature
folders inside one deployable** on purpose — it's the same CQRS boundaries
you'd need anyway, but without the operational cost of running four
services on day one. When traffic/team size justifies it, each
`Application/<Feature>` folder plus its slice of `Infrastructure` can be
lifted into its own project with minimal churn, because handlers already
only depend on interfaces.

---

## A note on this sandbox

This project was generated in an environment without the .NET SDK or
network access, so nothing here has been `dotnet build`-ed or
`dotnet ef`-verified. The code follows the standard Jason-Taylor-style
Clean Architecture template closely and package versions were kept aligned
with what was already pinned in the original `AINews.csproj`, but please run
`dotnet restore && dotnet build` first thing and expect to fix the odd
package-version or API-surface mismatch (Hangfire.PostgreSql's fluent
`UsePostgreSqlStorage` options API in particular is worth double-checking
against the installed version's docs).
