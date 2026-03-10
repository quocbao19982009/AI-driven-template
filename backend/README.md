# Backend

ASP.NET Core 10 REST API for the AI-Driven Full-Stack Template.

## Stack

- **ASP.NET Core 10** — REST API
- **Entity Framework Core 10** + **Npgsql** — PostgreSQL ORM
- **FluentValidation** — request validation
- **JWT Bearer** — authentication
- **Serilog** — structured logging (console + rolling file)
- **Swashbuckle** — Swagger/OpenAPI (used by `api:sync` to generate frontend hooks)

## Getting Started

1. Start the database**

From the repo root:

```bash
docker compose -f docker/docker-compose.yml up -d
```

PostgreSQL runs on port `5433`.

**2. Set the JWT secret**

Open `src/Backend.Api/appsettings.json` and replace the placeholder:

```json
"Jwt": {
  "Key": "CHANGE-THIS-TO-A-SECURE-KEY-AT-LEAST-32-CHARS!"
}
```

For production, use `dotnet user-secrets` or environment variables — never commit real secrets.

**3. Restore, migrate, and run**

```bash
cd backend/src/Backend.Api
dotnet restore
dotnet tool restore
dotnet ef database update
dotnet run
```

API runs at <http://localhost:5054>. Swagger UI at <http://localhost:5054/swagger>.

## Key Commands

| Command | What it does |
|---|---|
| `dotnet run` | Start the API |
| `dotnet build` | Build only |
| `dotnet test` | Run all tests |
| `dotnet ef migrations add <Name>` | Create a new EF Core migration |
| `dotnet ef database update` | Apply pending migrations |

## Configuration

All configuration lives in `src/Backend.Api/appsettings.json`:

| Key | Purpose |
|---|---|
| `ConnectionStrings.DefaultConnection` | PostgreSQL connection string |
| `Jwt.Key` | JWT signing key (min 32 chars) |
| `Jwt.Issuer` / `Jwt.Audience` | JWT token claims |
| `Jwt.ExpiryMinutes` | Token lifetime |
| `Cors.AllowedOrigins` | Origins allowed to call the API |

## Project Structure

```
backend/
├── src/Backend.Api/
│   ├── Features/
│   │   ├── _FeatureTemplate/   # Template — AI copies this when scaffolding
│   │   ├── Users/              # Example feature
│   │   ├── Todos/
│   │   ├── Rooms/
│   │   ├── Bookings/
│   │   └── Locations/
│   ├── Common/
│   │   ├── Models/
│   │   │   ├── BaseEntity.cs   # Base for all entities (Id, CreatedAt, UpdatedAt)
│   │   │   ├── ApiResponse.cs  # Standard response wrapper ApiResponse<T>
│   │   │   └── PagedResult.cs  # Paginated list wrapper
│   │   ├── Exceptions/
│   │   │   ├── NotFoundException.cs    # → 404
│   │   │   └── ValidationException.cs # → 400
│   │   ├── Middleware/
│   │   │   └── ErrorHandlingMiddleware.cs
│   │   └── Extensions/
│   │       └── ServiceExtensions.cs
│   ├── Data/
│   │   └── ApplicationDbContext.cs
│   └── Program.cs
└── Dockerfile                  # Optional — not used in default docker-compose setup
```

## Layer Pattern

Each feature follows the same structure:

```
Entity.cs           → EF Core model (extends BaseEntity)
EntityDtos.cs       → Request/response DTOs
EntityValidator.cs  → FluentValidation rules
IEntityRepository   → Repository interface
EntityRepository    → EF Core implementation (.AsNoTracking() on reads)
IEntityService      → Service interface
EntityService       → Business logic
EntityController    → HTTP layer (always returns ApiResponse<T>)
```

## Key Infrastructure

| Type | Usage |
|---|---|
| `BaseEntity` | Extend for all entities — provides `Id`, `CreatedAt`, `UpdatedAt` |
| `ApiResponse<T>.Ok(data)` | Standard success response |
| `ApiResponse<T>.Fail(message)` | Standard error response |
| `PagedResult<T>` | Return from paginated list endpoints |
| `throw new NotFoundException("Entity", id)` | Auto-mapped to 404 |
| `throw new ValidationException(errors)` | Auto-mapped to 400 |

## Adding a New Feature

The full workflow is AI-driven. From the repo root:

1. `/create-spec <feature-name>` — creates the spec file
2. `/scaffold-feature <feature-name>` — generates all backend files
3. `/add-migration Add<FeatureName>Entity` — creates and applies the EF migration
4. Register the feature in `Program.cs` (`AddScoped` for service + repository)

See the root `README.md` and `docs/ai-workflow.md` for the complete workflow.
