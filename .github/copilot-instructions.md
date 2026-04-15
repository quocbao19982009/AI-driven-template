# Project Conventions

> Backend conventions apply to `backend/**/*.cs` — see `.claude/rules/backend.md`.
> Frontend conventions apply to `frontend/src/**/*.{ts,tsx}` — see `.claude/rules/frontend.md`.

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

Specs live in `feature_docs/` — one subfolder per feature (e.g. `feature_docs/factories/feature-spec-factories.md`). Templates live in `feature_docs/_templates/`. Each spec is the single source of truth combining behavior and architecture.

**Spec section order:**

1. Entity · 2. Core Values & Principles · 3. Architecture Decisions · 4. Data Flow · 5. API Endpoints · 6. Validation Rules · 7. Business Rules · 8. Authorization · 9. Frontend UI Description · 10. Redux UI State · 11. File Locations · 12. Tests

**Workflow for every request (DO NOT SKIP):**

1. Read `feature_docs/FEATURES.md` — identify the feature and its spec path
2. **If the feature is NEW (no spec file exists yet):** create the spec file first — do NOT write any code until the spec file exists and is complete. Register the feature in `feature_docs/FEATURES.md`.
3. **NEVER make any code change before reading that feature's spec file.** Read it in full.
4. Make changes.
5. **ALWAYS update the spec** if the task changed any of: entity fields, validation or business rules, endpoints or auth, UI behavior or Redux state, architecture. Do NOT consider the task complete until the spec is up to date.

**No update needed for:** code quality fixes with no behavior impact.

---

## Coding Style

→ See `docs/coding-style.md` for all style rules (naming, formatting, Tailwind, testing).

---

## Database Safety (HARD RULE — NEVER VIOLATE)

- Only run `dotnet ef migrations add <Name>` to create migration files.
- NEVER run any of the following — these are reserved for the user only:
  - `dotnet ef database update`
  - `dotnet ef database drop`
  - `dotnet ef migrations remove`
  - Any raw SQL that modifies schema or data (`DROP`, `DELETE`, `TRUNCATE`, `ALTER`, `UPDATE` outside of normal application code)
- After creating a migration, always tell the user to run `dotnet ef database update` themselves.

---

## Backend Rules (applies to `backend/**/*.cs`)

- **NEVER run database-modifying commands** — only `dotnet ef migrations add <Name>` is allowed.
- Never hash passwords with SHA256 — use `BCrypt.Net-Next` (`BCrypt.HashPassword` / `BCrypt.Verify`).
- All read-only EF queries MUST use `.AsNoTracking()`.
- All paginated queries MUST have `.OrderBy()` before `.Skip()/.Take()`.
- Never put secrets in `appsettings.json` — use environment variables or `dotnet user-secrets`.
- Always return `ApiResponse<T>` from controllers — never return a raw DTO or primitive.
- Always pass `CancellationToken` through every async method (controller → service → repository).
- Register every new feature in `Program.cs`: `AddScoped<IXRepository, XRepository>()` and `AddScoped<IXService, XService>()`.

### Key Infrastructure

| Type                  | Location                                   | Purpose                                                                                               |
| --------------------- | ------------------------------------------ | ----------------------------------------------------------------------------------------------------- |
| `BaseEntity`          | `Common/Models/BaseEntity.cs`              | Base for all entities — provides `Id` (int), `CreatedAt`, `UpdatedAt`                                 |
| `ApiResponse<T>`      | `Common/Models/ApiResponse.cs`             | Standard response wrapper — use `ApiResponse<T>.Ok(data)` and `ApiResponse<T>.Fail(message)`          |
| `PagedResult<T>`      | `Common/Models/PagedResult.cs`             | Paginated list wrapper — record with `Items`, `TotalCount`, `Page`, `PageSize`, computed `TotalPages` |
| `NotFoundException`   | `Common/Exceptions/NotFoundException.cs`   | `throw new NotFoundException("Product", id)` → auto-mapped to 404                                     |
| `ValidationException` | `Common/Exceptions/ValidationException.cs` | `throw new ValidationException(errors)` → auto-mapped to 400 with error list                          |

---

## Frontend Rules (applies to `frontend/src/**/*.{ts,tsx}`)

- Never install new npm packages without checking if an existing library already covers it.
- Never edit files under `api/generated/` — they are overwritten by Orval on every `api:sync`.
- Always import DTO and model types from `@/api/generated/models` — never redefine them locally.
- After any backend DTO or endpoint change, run `npm run api:sync` from the repo root before touching the frontend.
- Server state (API data) lives in React Query — never duplicate it into Redux.
- Redux slices hold UI-only state only: search query, selected IDs, active tab, open panels.
- Never hardcode visible UI text — all strings must go through `t()` from `useTranslation()` (react-i18next).
- When adding translation keys, always update BOTH `src/locales/en.json` AND `src/locales/fi.json`.

### Key Infrastructure

| Type / Hook                         | Location                  | Purpose                                                                                |
| ----------------------------------- | ------------------------- | -------------------------------------------------------------------------------------- |
| `useAppDispatch` / `useAppSelector` | `store/hooks.ts`          | Typed Redux hooks — always use these, never raw `useDispatch`/`useSelector`            |
| `apiFetch`                          | `api/mutator/apiFetch.ts` | All HTTP calls go through this — Orval uses it automatically                           |
| `cn()`                              | `lib/utils.ts`            | Tailwind class merging utility                                                         |
| `useDebounce`                       | `hooks/use-debounce.ts`   | Debounce input values before triggering queries                                        |
| `useTranslation`                    | `react-i18next`           | All UI strings must use `const { t } = useTranslation()` — never hardcode visible text |

## Code Intelligence

Prefer LSP over Grep/Read for code navigation - it's faster, precise and avoids reading entire files:

- `workspaceSymbol` to find where something is defined
- `findReferences` to see all usages across the codebase
- `goToDefinition`/`goToImplementation` to jump to source
- `hover` for type info without reading the file
  Use Grep only when LSP isn't available or text/pattern searches (comments, string, config)

after writing or editing code, check LSP diagnostics and fix errors before proceeding
