# Project Memory — AI-driven-template

## Project Overview

Clean fullstack template (domain PoC features stripped 2026-03-06). .NET 10 backend + React/TypeScript frontend. Ready for new feature development.

## Key Paths

- Backend: `backend/src/Backend.Api/`
- Frontend: `frontend/src/`
- Feature docs: `feature_docs/` — one subfolder per feature; templates in `feature_docs/_templates/`
- Locales: `frontend/src/locales/en.json` and `fi.json`

## Active Features (as of 2026-03-06)

Only infrastructure + scaffolding remain:

- `Features/_FeatureTemplate/` — backend scaffold example
- `Features/Users/` — user infrastructure
- `features/_template-feature/` — frontend scaffold example
- `routes/features/index.tsx` — template demo route

## Database

- Single migration `InitialCreate` — covers only `Users` and `Features` tables (clean slate)

## Architecture Patterns

- Controller → Service → Repository → DbContext
- `ApiResponse<T>` wrapper on all endpoints
- `PagedResult<T>` for paginated responses
- `internal static MapToDto()` in Service classes

## Frontend Patterns

- Orval-generated hooks from `npm run api:sync` → `api/generated/`
- Redux slices: UI-only state (search, filters, selectedIds)
- Each feature: `store/`, `hooks/`, `components/`, `index.ts`
- Routes: `routes/{feature}/index.tsx` using TanStack Router file-based routing
- Translations: Always update BOTH en.json AND fi.json
- `cn()` utility: `lib/utils.ts` (NOT `lib/cn.ts`)
- `useDebounce` hook: `hooks/use-debounce.ts` (kebab-case)
- Component files: kebab-case (`factories-page.tsx`, NOT `FactoriesPage.tsx`)
- Backend feature files: flat structure (no `Dtos/` or `Validators/` subfolders)

## Tooling Notes

- `npm run api:sync` uses `dotnet swagger tofile` (no DB required) — no need to start `dotnet run` first.

## Workflow Rules

- **Always `TaskStop` background servers after use.** When starting `dotnet run` in background for `api:sync`, stop it immediately after `api:sync` completes. Forgetting leaves the port occupied and blocks future `dotnet run`.

## Tooling

- `npm run api:sync` from repo root regenerates frontend API hooks
- `dotnet ef migrations add <Name>` then `dotnet ef database update` for schema changes
- Lint: `cd frontend && npm run lint`
- Type check: `cd frontend && npx tsc --noEmit`
