---
globs: backend/**/*.cs
---

# Backend Critical Conventions (DO NOT DEVIATE)

- **NEVER run database-modifying commands** — `dotnet ef database update`, `dotnet ef database drop`, `dotnet ef migrations remove`, or any destructive SQL. The AI may only run `dotnet ef migrations add <Name>`. All database changes are applied manually by the user only.
- Never hash passwords with SHA256 — use `BCrypt.Net-Next` (`BCrypt.HashPassword` / `BCrypt.Verify`)
  > Why: SHA256 is a fast hash — attackers can try billions of guesses per second offline. BCrypt is designed to be slow (tunable cost factor), making brute-force infeasible. This applies to passwords only; fast hashes are fine for non-secret digests (e.g., ETags, cache keys).
- All read-only EF queries MUST use `.AsNoTracking()` (see `UsersRepository.GetAllAsync`)
  > Why: EF Core tracks every entity it loads in memory, even for queries that only read data. This wastes memory and risks accidental ghost writes if `SaveChanges` is called later in the same scope. `.AsNoTracking()` skips change tracking entirely for read-only paths.
- All paginated queries MUST have `.OrderBy()` before `.Skip()/.Take()` — omitting it gives non-deterministic results
  > Why: SQL has no guaranteed row order unless ORDER BY is specified. Without it, different pages can return the same rows or skip rows entirely — users see duplicates or missing items depending on the query plan the database chooses.
- Never put secrets in `appsettings.json` — use environment variables or `dotnet user-secrets`
  > Why: `appsettings.json` is committed to source control. Once a secret is in git history it is permanently exposed — even if you delete the file in a later commit, the secret remains accessible in the full commit log.
- Always return `ApiResponse<T>` from controllers — never return a raw DTO or primitive
- Always pass `CancellationToken` through every async method (controller → service → repository)
- Register every new feature in `Program.cs`: `AddScoped<IProductsRepository, ProductsRepository>()` and `AddScoped<IProductsService, ProductsService>()`
- Use `UserRole` enum for roles — never use magic strings like `"Admin"` or `"User"`
  > Why: Magic strings are typo-prone and invisible to the compiler. The `UserRole` enum is stored as a string in the DB via `HasConversion<string>()` and serialized as a string in JSON via `[JsonConverter(typeof(JsonStringEnumConverter))]`, so the API and DB remain human-readable.
- Deactivated users (`IsActive = false`) must be rejected during login — check after password verification, return the same generic 401 message
  > Why: If a deactivated user can still authenticate, admin deactivation is meaningless. Checking after password verification prevents timing-based enumeration of active vs. inactive accounts.

---

## Authorization Pattern

- Always add `[Authorize]` or `[AllowAnonymous]` explicitly on every controller or endpoint — never rely on defaults
- For endpoints that access user-specific resources, validate ownership in the controller:
  ```csharp
  if (!IsOwnerOrAdmin(id)) return Forbid();
  ```
  > Why: Without IDOR protection, any authenticated user can view/edit other users' data by changing the `id` parameter. The controller is the right place because the caller's identity is available via `User.Claims` without a DB lookup.
- Only admins may change `UserRole` — pass `isAdmin` from the controller to the service. Non-admin callers silently keep the existing role.
  > Why: If the service trusts the `Role` field from the request body without checking the caller's authority, any user can self-promote to Admin. Silently ignoring (rather than rejecting) prevents information leakage about what fields are restricted.
- Never accept `Role` in public registration endpoints — default to `UserRole.User`

---

## Key Infrastructure — use these, do not reinvent

| Type                  | Location                                   | Purpose                                                                                               |
| --------------------- | ------------------------------------------ | ----------------------------------------------------------------------------------------------------- |
| `BaseEntity`          | `Common/Models/BaseEntity.cs`              | Base for all entities — provides `Id` (int), `CreatedAt`, `UpdatedAt`                                 |
| `ApiResponse<T>`      | `Common/Models/ApiResponse.cs`             | Standard response wrapper — use `ApiResponse<T>.Ok(data)` and `ApiResponse<T>.Fail(message)`          |
| `PagedResult<T>`      | `Common/Models/PagedResult.cs`             | Paginated list wrapper — record with `Items`, `TotalCount`, `Page`, `PageSize`, computed `TotalPages` |
| `UserRole`            | `Common/Models/UserRole.cs`                | Role enum (`User`, `Admin`) — stored as string in DB, serialized as string in JSON. Use instead of magic strings. |
| `NotFoundException`   | `Common/Exceptions/NotFoundException.cs`   | `throw new NotFoundException("Product", id)` → auto-mapped to 404                                     |
| `ValidationException` | `Common/Exceptions/ValidationException.cs` | `throw new ValidationException(errors)` → auto-mapped to 400 with error list                          |
