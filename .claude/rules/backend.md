---
globs: backend/**/*.cs
---

# Backend Critical Conventions (DO NOT DEVIATE)

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

---

## Key Infrastructure — use these, do not reinvent

| Type                  | Location                                   | Purpose                                                                                               |
| --------------------- | ------------------------------------------ | ----------------------------------------------------------------------------------------------------- |
| `BaseEntity`          | `Common/Models/BaseEntity.cs`              | Base for all entities — provides `Id` (int), `CreatedAt`, `UpdatedAt`                                 |
| `ApiResponse<T>`      | `Common/Models/ApiResponse.cs`             | Standard response wrapper — use `ApiResponse<T>.Ok(data)` and `ApiResponse<T>.Fail(message)`          |
| `PagedResult<T>`      | `Common/Models/PagedResult.cs`             | Paginated list wrapper — record with `Items`, `TotalCount`, `Page`, `PageSize`, computed `TotalPages` |
| `NotFoundException`   | `Common/Exceptions/NotFoundException.cs`   | `throw new NotFoundException("Product", id)` → auto-mapped to 404                                     |
| `ValidationException` | `Common/Exceptions/ValidationException.cs` | `throw new ValidationException(errors)` → auto-mapped to 400 with error list                          |
