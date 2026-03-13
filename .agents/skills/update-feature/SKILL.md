---
name: update-feature
description: Modify an existing feature — add fields, change behavior, update endpoints, or adjust UI. Reads the current spec, plans changes, implements across all layers, and keeps the spec in sync.
argument-hint: "[feature-name]: [what to change]"
allowed-tools: "Read, Write, Edit, Bash, Glob, Grep, mcp__context7__resolve-library-id, mcp__context7__query-docs"
context: fork
---

# Update Feature

Update the `$ARGUMENTS` feature.

## Argument Parsing

Parse `$ARGUMENTS` into:

- **feature-name** — the text before the first `:` (normalize to lowercase-kebab-case)
- **change description** — everything after the first `:`

If no `:` is found, treat the entire argument as the feature name and ask the user what they want to change before proceeding.

## Step 1: Read Current Spec

Read the feature spec:

```
feature_docs/{feature-name}/feature-spec-{feature-name}.md
```

If no spec exists, **stop** and tell the user:

> "No spec found at `feature_docs/{feature-name}/feature-spec-{feature-name}.md`. Create one first with `/create-spec {feature-name}`."

Also read `feature_docs/FEATURES.md` for the current feature status.

## Step 2: Plan the Changes

Based on the change description, determine which layers are affected:

| Change Type                     | Layers to Update                                                                                |
| ------------------------------- | ----------------------------------------------------------------------------------------------- |
| Add/remove/rename entity fields | Entity → DTOs → Validator → Repository → Service → Migration → API sync → Frontend form + table |
| Change validation rules         | Validator → Frontend form schema                                                                |
| Change business logic           | Service → Service tests                                                                         |
| Add/change endpoints            | Controller → Service → Repository → API sync → Frontend hooks                                   |
| Change UI behavior              | Frontend components → Redux slice (if UI state changed)                                         |
| Change authorization            | Controller attributes → Frontend route guards                                                   |

List the planned changes before implementing. If the scope is ambiguous, ask the user to confirm before proceeding.

## Step 3: Implement Backend Changes

Follow all AGENTS.md conventions:

- Entity inherits `BaseEntity` — do NOT re-add Id, CreatedAt, UpdatedAt
- All read-only queries use `.AsNoTracking()`
- All paginated queries have `.OrderBy()` before `.Skip()/.Take()`
- All controller actions return `ApiResponse<T>`
- All async methods pass `CancellationToken` through the full chain
- `NotFoundException` for missing entities, `ValidationException` for rule violations

**If entity fields changed:**

1. Update `Entity.cs` with new/modified properties
2. Update `DTOs` (Create, Update, Response records)
3. Update `Validator` rules
4. Update `Repository` if query logic changed
5. Update `Service` mapping logic
6. Create a migration: `dotnet ef migrations add <DescriptiveName> --project backend/src/Backend.Api`
7. Tell the user to run `dotnet ef database update` themselves — **NEVER run it**

## Step 4: Run API Sync (if endpoints or DTOs changed)

If any controller endpoints or DTO shapes changed:

```bash
cd frontend && npm run api:sync
```

This regenerates the Orval types and hooks. Do NOT proceed to frontend changes until this succeeds.

## Step 5: Implement Frontend Changes

Follow all frontend conventions:

- Never edit files under `api/generated/`
- Import types from `@/api/generated/models`
- All UI strings through `t()` from `useTranslation()`
- Server state in React Query, UI-only state in Redux
- Update both `en.json` and `fi.json` if adding translation keys

**Common frontend updates:**

- New/renamed fields → update form dialog, table columns, validation schema
- New endpoints → Orval generates hooks automatically after api:sync
- Changed UI behavior → update page/table/dialog components
- New UI state → update Redux slice

## Step 6: Run Quality Checks

- If backend changed: `dotnet test backend/Backend.sln`
- If frontend changed: `cd frontend && npm run lint && npm run typecheck && npm run test:run`
- Fix any failures before proceeding

## Step 7: Update Feature Spec

Update the spec file to reflect ALL changes made:

- Entity fields changed → update Entity Fields table and Relationships
- Validation changed → update Validation Rules section
- Business logic changed → update Business Rules and Acceptance Scenarios
- Endpoints changed → update API Endpoints table
- UI changed → update Frontend UI Description
- Files added/moved → update File Locations
- Tests added → update Tests section
- Update `Last Updated` date

Also update `feature_docs/FEATURES.md` — keep the row's Data Model, Summary, and Status columns in sync.

## Step 8: Summary

Provide a summary of:

- What was changed (grouped by layer)
- Files modified/created
- Migration created (if any) — remind user to run `dotnet ef database update`
- Test results
- Spec sections updated
