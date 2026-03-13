# Feature Implementation Checklist

> Referenced by CLAUDE.md. Use this to verify a feature is fully implemented before marking it done.

---

## Backend

- [ ] Entity inherits `BaseEntity` — no manual `Id`/`CreatedAt`/`UpdatedAt`
- [ ] `DbSet<T>` added to `ApplicationDbContext`
- [ ] DTOs defined: `Create<Feature>Request`, `Update<Feature>Request`, `<Feature>Dto`
- [ ] Validator defined for both create and update requests
- [ ] `IFeatureRepository` interface + `FeatureRepository` implementation
  - [ ] All reads use `.AsNoTracking()`
  - [ ] All paginated reads use `.OrderBy()` before `.Skip()/.Take()`
- [ ] `IFeatureService` interface + `FeatureService` implementation
  - [ ] Internal static `MapToDto()` method
  - [ ] `NotFoundException` thrown for missing entities
  - [ ] `CancellationToken` threaded through all async calls
- [ ] `FeatureController` returns `ApiResponse<T>` on every action
- [ ] Services registered in `Program.cs` (`AddScoped`)
- [ ] Migration created: `dotnet ef migrations add Add<Feature>Entity`
- [ ] User told to run: `dotnet ef database update`
- [ ] `dotnet build` passes — 0 errors

## API Sync

- [ ] `npm run api:sync` run from repo root after all backend changes
- [ ] Orval-generated hooks visible in `frontend/src/api/generated/<feature>/`

## Frontend

- [ ] Redux slice created with UI-only state; registered in `store/store.ts`
- [ ] Pagination hook wraps Orval list hook
- [ ] Form hook uses Orval-generated Zod schema with `.extend()` for translated errors
- [ ] Page, table, form dialog, delete dialog components created (kebab-case filenames)
- [ ] Route file created in `routes/<feature>/index.tsx`
- [ ] Nav link added in `components/layout/app-layout.tsx`
- [ ] Translation keys added to **both** `en.json` and `fi.json`
- [ ] No hardcoded visible strings (all use `t()`)
- [ ] No server state duplicated into Redux
- [ ] `npm run lint` — 0 errors
- [ ] `npx tsc --noEmit` — 0 errors

## Spec

- [ ] Feature spec file exists in `feature_docs/<feature>/feature-spec-<feature>.md`
- [ ] Feature registered in `feature_docs/FEATURES.md`
- [ ] Spec reflects final implemented behavior (no stale TODOs or NEEDS CLARIFICATION markers)

## Tests (optional but recommended)

- [ ] Backend unit tests cover: happy path, not-found, validation failure
- [ ] Frontend tests cover: render, form submit, error state
