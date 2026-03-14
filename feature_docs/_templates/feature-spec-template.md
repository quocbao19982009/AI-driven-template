# Feature Specification: [FeatureName]

**Last Updated:** `[YYYY-MM-DD]`
**Tests written:** no

<!--
HOW TO USE THIS TEMPLATE
1. Copy this file and rename it: feature-spec-products.md
2. Fill in every section for your new feature
3. Delete sections that don't apply (e.g. no relationships, no auth rules)
4. Hand this file to AI with the prompt:
   "Scaffold this feature end-to-end following the project patterns in CLAUDE.md.
    Use feature-spec-products.md as the source of truth."
5. If AI annotated any section with `<!-- TODO: ... -->` or `[NEEDS CLARIFICATION: ...]`,
   resolve those before running /scaffold-feature — the skill will refuse to proceed while
   any such markers exist.
6. After scaffolding, run: npm run api:sync (from repo root)
-->

---

## 1. Entity

**Name:** `[FeatureName]` *(e.g. Product)*
**Table name (plural):** `[FeatureNames]` *(e.g. Products)*

### Fields

| Property | C# Type | Required | Constraints | Notes |
|----------|---------|----------|-------------|-------|
| `Name`   | `string` | yes | max 200 chars | |

> `Id` (int), `CreatedAt`, `UpdatedAt` are inherited from `BaseEntity` — do not add them.

---

## Relationships

<!--
Examples:
- belongs to Category (CategoryId FK, required)
- has many OrderItems
- none
-->

- none

---

## 2. Core Values & Principles

<!--
What is this feature's core purpose? What invariants must hold?
Examples:
- "Factory names must be globally unique to prevent ambiguity"
- "Deletion is soft — historical data must never be lost"
-->

- [Describe the feature's purpose and key invariants]

---

## 3. Architecture Decisions

<!--
Note any deviations from the standard pattern or important design choices.
If standard (Controller → Service → Repository → EF Core), just say "Standard layered pattern."
-->

- Standard layered pattern: Controller → Service → Repository → EF Core

---

## 4. Data Flow

<!--
Describe how data moves through the layers for each operation.
Delete this section for simple CRUD features where the flow is obvious.
-->

### Create
1. Client POST `/api/[features]` with request body
2. Controller calls Service
3. Service validates, maps to entity, calls Repository
4. Repository saves, returns entity
5. Service maps to DTO, returns ApiResponse

### Read (list)
1. Client GET `/api/[features]?page=1&pageSize=10`
2. Repository applies filters, OrderBy, Skip/Take, returns PagedResult
3. Service maps to DTOs

---

## 5. API Endpoints

| Method | Route | Description | Auth required |
|--------|-------|-------------|---------------|
| `GET`    | `/api/[features]`       | Paginated list | no  |
| `GET`    | `/api/[features]/{id}`  | Get single     | no  |
| `POST`   | `/api/[features]`       | Create         | yes |
| `PUT`    | `/api/[features]/{id}`  | Full update    | yes |
| `DELETE` | `/api/[features]/{id}`  | Delete         | yes |

---

## 6. Validation Rules

<!--
These map directly to FluentValidation rules in [Feature]sValidator.cs
-->

- `Name`: required, not empty, max 200 characters

---

## 7. Business Rules

<!--
Logic that goes in the Service layer, not just validation.
Examples: ownership checks, state machine transitions, cascading updates.
Delete this section if there are none.
-->

- none

### Acceptance Scenarios

<!--
One happy-path + one failure scenario per endpoint or business rule.
Fill these in before running /scaffold-feature — they become the test targets.
-->

**Scenario: Create with valid data**
- Given: a POST /api/[features] with all required fields valid
- When: the request is processed
- Then: returns 201 with the created entity wrapped in ApiResponse<T>

**Scenario: Create with empty name**
- Given: a POST /api/[features] with Name = "" or whitespace
- When: the request is processed
- Then: returns 400 with a validation error listing the Name field

**Scenario: Get by non-existent ID**
- Given: a GET /api/[features]/{id} where the ID does not exist
- When: the request is processed
- Then: returns 404 with NotFoundException message

---

## 8. Authorization

<!--
Leave blank or write "none" if no role-based rules apply.
-->

- none

---

## 9. Frontend UI

### Design reference

<!--
Attach a Figma screenshot, export, or any image that shows what this feature should look like.
AI will use it to determine which components, layout, and interactions to build.

Options:
- Paste a Figma link:     https://figma.com/...
- Attach an image file:   ![UI Design](../designs/products-ui.png)
- Sketch in ASCII if no design exists yet (see below)
-->

[Paste Figma link or attach image here]

[NEEDS CLARIFICATION: design reference not provided — attach a Figma link, image path, or write "none"]

### Description

<!--
Describe the UI in plain language. Mention key interactions, states, and behavior.
The more specific you are, the less AI has to guess.

Examples:
- "Table of products with search bar at the top. Each row has an action dropdown
   with Edit and Delete. Clicking Edit opens a slide-over panel with the full form.
   Empty state shows an illustration with a 'Create your first product' button."
- "Dashboard showing 4 KPI cards at the top, a line chart of sales over time,
   and a recent orders table below. Data refreshes every 30 seconds."
- "Two-column settings page. Left side is a nav list of setting categories.
   Right side renders the active category form. Save button is sticky at the bottom."
-->

[NEEDS CLARIFICATION: UI description is missing — describe what the page looks like, key interactions, and empty state behavior]

---

## 10. Redux UI State

<!--
Only UI-only state belongs here (not server data — that's React Query).
AI will infer most state from your description and design — only add state
that isn't obvious from the UI (e.g. a multi-step wizard's current step).
Examples: searchQuery, selectedIds, activeTab, isFilterOpen, sortDirection
-->

- `searchQuery: string`
- `selectedIds: string[]`

---

## 11. File Locations

### Backend

| File | Path |
|------|------|
| Entity | `backend/src/Backend.Api/Features/[FeatureName]s/[FeatureName].cs` |
| DTOs | `backend/src/Backend.Api/Features/[FeatureName]s/[FeatureName]Dtos.cs` |
| Validator | `backend/src/Backend.Api/Features/[FeatureName]s/[FeatureName]sValidator.cs` |
| Repository interface | `backend/src/Backend.Api/Features/[FeatureName]s/I[FeatureName]sRepository.cs` |
| Repository | `backend/src/Backend.Api/Features/[FeatureName]s/[FeatureName]sRepository.cs` |
| Service interface | `backend/src/Backend.Api/Features/[FeatureName]s/I[FeatureName]sService.cs` |
| Service | `backend/src/Backend.Api/Features/[FeatureName]s/[FeatureName]sService.cs` |
| Controller | `backend/src/Backend.Api/Features/[FeatureName]s/[FeatureName]sController.cs` |

### Frontend

| File | Path |
|------|------|
| Page component | `frontend/src/features/[feature-name]/components/[feature-name]-page.tsx` |
| Table component | `frontend/src/features/[feature-name]/components/[feature-name]-table.tsx` |
| Form dialog | `frontend/src/features/[feature-name]/components/[singular]-form-dialog.tsx` |
| Delete dialog | `frontend/src/features/[feature-name]/components/[singular]-delete-dialog.tsx` |
| Pagination hook | `frontend/src/features/[feature-name]/hooks/use-[feature-name]-pagination.ts` |
| Redux slice | `frontend/src/features/[feature-name]/store/[feature-name]-slice.ts` |
| Route | `frontend/src/routes/[feature-name]/index.tsx` |
| Generated API | `frontend/src/api/generated/[feature-name]/` |

---

## 12. Tests

<!--
List the test scenarios for this feature. Update this section as tests are written.
-->

**Tests written:** no

### Backend Unit Tests

| Test | Description |
| ---- | ----------- |
| `CreateAsync_WithValidData_Returns[FeatureName]Dto` | Happy path |
| `GetAllAsync_ReturnsPaginatedResult` | Correct page and total |
| `GetByIdAsync_WithInvalidId_ThrowsNotFoundException` | 404 for missing entity |

### Frontend Tests

| Test | Description |
| ---- | ----------- |
| `[FeatureName]sPage renders table` | Integration: table populated from mocked query |
| `Form dialog submits create request` | Correct payload sent |

---

## Migration Name

<!--
Convention: Add[EntityName]Entity
Used in: dotnet ef migrations add [MigrationName]
-->

`Add[FeatureName]Entity`

---

> **Marker convention:** Both marker types must be resolved before running `/scaffold-feature`:
> - `<!-- TODO: [what is missing] -->` — AI flags missing info that may have a reasonable default
> - `[NEEDS CLARIFICATION: [question] ]` — requires a specific human answer before scaffolding

## Checklist

### Backend
- [ ] Entity created in `Features/[Feature]s/[Feature].cs`
- [ ] DTOs created in `Features/[Feature]s/[Feature]Dtos.cs`
- [ ] Validator created in `Features/[Feature]s/[Feature]sValidator.cs`
- [ ] Repository interface + implementation created
- [ ] Service interface + implementation created
- [ ] Controller created with correct routes
- [ ] Registered in `Program.cs`
- [ ] Migration created and applied

### API Sync
- [ ] `npm run api:sync` run from repo root
- [ ] `api/generated/[feature]s/` folder generated by Orval

### Frontend
- [ ] `features/[feature]s/` folder created with all layers
- [ ] Table columns match spec above
- [ ] Form fields match spec above
- [ ] Redux slice registered in `store/store.ts`
- [ ] Route added in `routes/[feature]s/index.tsx`
- [ ] Link added to navigation in `components/layout/app-layout.tsx`
