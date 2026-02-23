# Backend Code Review

## Critical

- **Password hashing uses SHA256** — Not a password hashing algorithm. Replace with `BCrypt.Net-Next` or `PasswordHasher<T>`. (`UsersService.cs:131`)
- **JWT secret in appsettings.json** — Move to user secrets (dev) and environment variables (prod).
- **DB credentials in appsettings.json** — Same as above. Never commit secrets to source control.

## High

- **No login endpoint** — `JwtService` exists but is never called. `[Authorize]` attributes are unusable without an auth endpoint.
- **Missing `AsNoTracking()`** — Read queries in `UsersRepository` track entities unnecessarily. Add `.AsNoTracking()` for reads.
- **No ordering on paginated queries** — `Skip/Take` without `OrderBy` gives non-deterministic results. Add `.OrderBy(u => u.Id)`.
- **No tests for Users feature** — Only the `_FeatureTemplate` has tests. The actual Users feature has zero coverage.

## Medium

- **No ownership check on Update/GetById** — Any authenticated user can read or modify any other user's data.
- **PUT replaces all fields** — No partial update support. Consider `PATCH` for updating individual fields.
- **`JsonSerializerOptions` allocated per error** — In `ErrorHandlingMiddleware`, make it `static readonly`.
- **`ApiResponse<T>` has mutable properties** — Use `init` setters or convert to a record.
- **Redundant OpenAPI registration** — Both `AddSwaggerGen()` and `AddOpenApi()` are registered. Pick one.

## What's Good

- Feature-sliced folder structure
- Interface abstractions for services and repositories
- CancellationToken passed through all layers
- Consistent `ApiResponse<T>` wrapper
- Centralized error handling middleware
- FluentValidation with auto-registration
- Well-designed `PagedResult<T>` with computed properties
- Structured logging with Serilog
- DTOs as immutable records
- Reusable feature template for scaffolding
