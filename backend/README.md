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

## Customization

### JWT Authentication

JWT settings live in `appsettings.json` under the `Jwt` key:

```json
"Jwt": {
  "Key": "your-secret-key-min-32-chars",
  "Issuer": "Backend",
  "Audience": "Backend.Client",
  "ExpiryMinutes": "60"
}
```

- **Key** — signing secret, minimum 32 characters. Use `dotnet user-secrets` or an environment variable in production — never commit a real key.
- **ExpiryMinutes** — how long a token is valid before the client must re-authenticate.
- **Issuer / Audience** — validated on every request; change both here and in any client that verifies tokens.

Token validation parameters (e.g. disabling lifetime validation for testing) are configured in `Common/Extensions/ServiceExtensions.cs` → `AddJwtAuthentication`.

---

### CORS

Allowed origins are listed in `appsettings.json`:

```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:5173",
    "https://your-production-domain.com"
  ]
}
```

Add your frontend origin here. The policy allows any header and any method from listed origins. To restrict allowed headers or methods, edit `AddCorsPolicy` in `Common/Extensions/ServiceExtensions.cs`.

---

### Database

The connection string is in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5433;Database=backend_db;Username=postgres;Password=postgres"
}
```

The project uses **PostgreSQL via Npgsql**. To switch to a different provider (e.g. SQL Server), replace `UseNpgsql` with the appropriate provider in `Common/Extensions/ServiceExtensions.cs` → `AddDatabase` and update the NuGet package.

---

### Logging (Serilog)

Logging is fully configured in `appsettings.json` under the `Serilog` key — no code changes needed for most adjustments:

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "WriteTo": [
    { "Name": "Console" },
    {
      "Name": "File",
      "Args": {
        "path": "logs/log-.txt",
        "rollingInterval": "Day",
        "retainedFileCountLimit": 7
      }
    }
  ]
}
```

Common adjustments:
- **Silence EF Core SQL queries** — `"Microsoft.EntityFrameworkCore": "Warning"` (already set)
- **Enable debug logging** — change `"Default": "Debug"`
- **Keep more log files** — increase `retainedFileCountLimit`
- **Add a sink** (e.g. Seq, Application Insights) — add a NuGet package and a new entry in `WriteTo`

---

### Swagger / OpenAPI

The API title, version, and description are set in `Program.cs`:

```csharp
options.SwaggerDoc("v1", new OpenApiInfo
{
    Title = "Backend API",
    Version = "v1",
    Description = "..."
});
```

Swagger UI is only enabled in the `Development` environment. To enable it in other environments, move the `app.UseSwagger()` block outside the `if (app.Environment.IsDevelopment())` check in `Program.cs`.

---

### Code Formatting

The project uses `.editorconfig` at `backend/.editorconfig` for all C# formatting rules — indentation, brace style, naming conventions, and more. Any editor that respects EditorConfig will pick this up automatically.

To enforce formatting in the terminal or CI:

```bash
npm run format:backend        # fix files in place
npm run format:backend:check  # check only, exits non-zero if changes needed (CI)
```

Both commands are run from the **repo root** and use `dotnet format` against `backend/Backend.sln`.

---

## Adding a New Feature

The full workflow is AI-driven. From the repo root:

1. `/create-spec <feature-name>` — creates the spec file
2. `/scaffold-feature <feature-name>` — generates all backend files
3. `/add-migration Add<FeatureName>Entity` — creates and applies the EF migration
4. Register the feature in `Program.cs` (`AddScoped` for service + repository)

See the root `README.md` and `docs/ai-workflow.md` for the complete workflow.
