# Project Conventions

## Critical Conventions (DO NOT DEVIATE)

### Backend

- Never hash passwords with SHA256 — use `BCrypt.Net-Next` (`BCrypt.HashPassword` / `BCrypt.Verify`)
  > Why: SHA256 is a fast hash — attackers can try billions of guesses per second offline. BCrypt is designed to be slow (tunable cost factor), making brute-force infeasible. This applies to passwords only; fast hashes are fine for non-secret digests (e.g., ETags, cache keys).
- All read-only EF queries MUST use `.AsNoTracking()` (see `UsersRepository.GetAllAsync`)
  > Why: EF Core tracks every entity it loads in memory, even for queries that only read data. This wastes memory and risks accidental ghost writes if `SaveChanges` is called later in the same scope. `.AsNoTracking()` skips change tracking entirely for read-only paths.
- All paginated queries MUST have `.OrderBy()` before `.Skip()/.Take()` — omitting it gives non-deterministic results
  > Why: SQL has no guaranteed row order unless ORDER BY is specified. Without it, different pages can return the same rows or skip rows entirely — users see duplicates or missing items depending on the query plan the database chooses.
- Never put secrets in `appsettings.json` — use environment variables or `dotnet user-secrets`
  > Why: `appsettings.json` is committed to source control. Once a secret is in git history it is permanently exposed — even if you delete the file in a later commit, the secret remains accessible in the full commit log.
- API docs use `AddSwaggerGen()` only — do NOT add `AddOpenApi()` alongside it
  > Why: `AddSwaggerGen()` (Swashbuckle) and `AddOpenApi()` (.NET 9 minimal API docs) both register overlapping middleware and routes. Running both produces duplicate or broken API doc endpoints that cannot be resolved without removing one.
- Always return `ApiResponse<T>` from controllers — never return a raw DTO or primitive
- Always pass `CancellationToken` through every async method (controller → service → repository)
- Register every new feature in `Program.cs`: `AddScoped<IProductsRepository, ProductsRepository>()` and `AddScoped<IProductsService, ProductsService>()`

### Frontend

- Never install new npm packages without checking if an existing library already covers it
- Never edit files under `api/generated/` — they are overwritten by Orval on every `api:sync`
  > Why: Orval regenerates the entire `api/generated/` folder from the backend OpenAPI spec on every `npm run api:sync`. Any manual edits are silently deleted. Customizations belong in `api/mutator/apiFetch.ts` or wrapper hooks, not in generated files.
- After any backend DTO or endpoint change, run `npm run api:sync` from the repo root before touching the frontend
- Server state (API data) lives in React Query — never duplicate it into Redux
  > Why: Duplicating server state into Redux creates two sources of truth that drift apart. Stale Redux state silently overrides fresh API data — components read cached Redux values instead of the latest server response, causing bugs that are hard to reproduce.
- Redux slices hold UI-only state only: search query, selected IDs, active tab, open panels
- Never hardcode visible UI text — all strings must go through `t()` from `useTranslation()` (react-i18next)
- When adding translation keys, always update BOTH `src/locales/en.json` AND `src/locales/fi.json`

---

## Key Infrastructure

### Backend — use these, do not reinvent

| Type                  | Location                                   | Purpose                                                                                               |
| --------------------- | ------------------------------------------ | ----------------------------------------------------------------------------------------------------- |
| `BaseEntity`          | `Common/Models/BaseEntity.cs`              | Base for all entities — provides `Id` (int), `CreatedAt`, `UpdatedAt`                                 |
| `ApiResponse<T>`      | `Common/Models/ApiResponse.cs`             | Standard response wrapper — use `ApiResponse<T>.Ok(data)` and `ApiResponse<T>.Fail(message)`          |
| `PagedResult<T>`      | `Common/Models/PagedResult.cs`             | Paginated list wrapper — record with `Items`, `TotalCount`, `Page`, `PageSize`, computed `TotalPages` |
| `NotFoundException`   | `Common/Exceptions/NotFoundException.cs`   | `throw new NotFoundException("Product", id)` → auto-mapped to 404                                     |
| `ValidationException` | `Common/Exceptions/ValidationException.cs` | `throw new ValidationException(errors)` → auto-mapped to 400 with error list                          |

### Frontend — use these, do not reinvent

| Type / Hook                         | Location                  | Purpose                                                                                |
| ----------------------------------- | ------------------------- | -------------------------------------------------------------------------------------- |
| `useAppDispatch` / `useAppSelector` | `store/hooks.ts`          | Typed Redux hooks — always use these, never raw `useDispatch`/`useSelector`            |
| `apiFetch`                          | `api/mutator/apiFetch.ts` | All HTTP calls go through this — Orval uses it automatically                           |
| `cn()`                              | `lib/cn.ts`               | Tailwind class merging utility                                                         |
| `useDebounce`                       | `hooks/useDebounce.ts`    | Debounce input values before triggering queries                                        |
| `useTranslation`                    | `react-i18next`           | All UI strings must use `const { t } = useTranslation()` — never hardcode visible text |

---

## Clarify Before You Code (ALWAYS DO THIS)

**For new features, always ask:**

1. **Entity fields** — What fields does the entity need? What are their types, constraints, and whether they're required or optional?
2. **Relationships** — Does this entity relate to any other entity (e.g., belongs to a User)?
3. **Business rules** — Any special logic? (e.g., status transitions, computed fields, restrictions)
4. **Endpoints** — Standard CRUD only, or are there custom actions (e.g., toggle, bulk delete)?
5. **Frontend UI** — Table + form dialog (standard)? Or something different?
6. **Authorization** — Public, authenticated only, or role-restricted?

**For changes to existing features, ask if any of these are unclear:**

- What is the exact expected behavior after the change?
- Does this affect any other feature or shared component?
- Should old data be migrated or is this forward-only?

Ask all questions in a single message. Do not start coding until requirements are clear.

---

## Feature Generation

- **Backend steps + exception handling + layer pattern** → `docs/feature-generation/backend.md`
- **Frontend steps + file tree + file roles** → `docs/feature-generation/frontend.md`
- **Implementation checklist** → `docs/feature-generation/implementation-checklist.md`

---

## Feature Spec Sync (ALWAYS DO THIS)

Specs live in `feature_docs/` — one file per feature, no separate `architectures/` folder. Each spec is the single source of truth combining behavior and architecture.

**Spec section order:**

1. Entity · 2. Core Values & Principles · 3. Architecture Decisions · 4. Data Flow · 5. API Endpoints · 6. Validation Rules · 7. Business Rules · 8. Authorization · 9. Frontend UI Description · 10. Redux UI State · 11. File Locations · 12. Tests

**Workflow for every request (DO NOT SKIP):**

1. Read `feature_docs/FEATURES.md` — identify the feature
2. **NEVER make any code change before reading that feature's spec file.** Read it in full.
3. Make changes
4. **ALWAYS update the spec before responding to the user** if the task changed any of: entity fields, validation or business rules, endpoints or auth, UI behavior or Redux state, architecture. Use the `spec-sync` agent to do this. **Do NOT consider the task complete until the spec is up to date.**

**No update needed for:** code quality fixes with no behavior impact.

---

## Coding Style

→ See `docs/coding-style.md` for all style rules (naming, formatting, Tailwind, testing).

**To change a style rule:** edit `docs/coding-style.md`, commit it, and the AI will follow the updated rules from the next session onward.

---

## Tooling

- Always use Context7 MCP when library/API documentation, code generation, or configuration steps are needed — do not ask, just use it if available