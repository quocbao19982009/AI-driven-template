# Feature Specification: Todo

**Last Updated:** `2026-03-09`
**Tests written:** yes

---

## 1. Entity

**Name:** `Todo`
**Table name (plural):** `Todos`

### Fields

| Property      | C# Type    | Required | Constraints              | Notes                          |
|---------------|------------|----------|--------------------------|--------------------------------|
| `Title`       | `string`   | yes      | max 200 chars            |                                |
| `Description` | `string?`  | no       | max 1000 chars           |                                |
| `IsCompleted` | `bool`     | yes      | default `false`          | toggled via dedicated endpoint |
| `DueDate`     | `DateTime?`| no       | must be in the future if provided | nullable |

> `Id` (int), `CreatedAt`, `UpdatedAt` are inherited from `BaseEntity` — do not add them.

---

## Relationships

- none

---

## 2. Core Values & Principles

- Todos are global (not per-user) — any visitor can view, create, edit, or delete
- `IsCompleted` is toggled via a dedicated PATCH endpoint, not editable directly through PUT (avoids accidental reset)
- Title uniqueness is NOT enforced — duplicate titles are allowed
- All endpoints are public (no authentication required)

---

## 3. Architecture Decisions

**Decision**: Dedicated toggle endpoint for `IsCompleted`
**Alternatives Considered**: Allow IsCompleted in PUT body; use a boolean query param
**Rationale**: A toggle endpoint (`PATCH /todos/{id}/toggle`) is semantically clearer and prevents callers from accidentally overwriting completion state during a general update.

**Decision**: Standard layered pattern (Controller → Service → Repository → EF Core)
**Alternatives Considered**: Minimal API, CQRS with MediatR
**Rationale**: Consistent with every other feature in this template; no complexity warrants CQRS here.

---

## 4. Data Flow

### Create
1. Client POST `/api/todos` with `{ title, description?, dueDate? }`
2. Controller calls `TodosService.CreateAsync`
3. Service validates via FluentValidation, maps request to `Todo` entity (`IsCompleted = false`)
4. Repository saves, returns entity
5. Service maps to `TodoDto`, returns `ApiResponse<TodoDto>` (201)

### Read (list)
1. Client GET `/api/todos?page=1&pageSize=10`
2. Repository applies `OrderBy(t => t.CreatedAt)`, Skip/Take, returns `PagedResult<Todo>`
3. Service maps to `TodoDto`

### Toggle
1. Client PATCH `/api/todos/{id}/toggle`
2. Service loads entity, flips `IsCompleted`, saves
3. Returns updated `TodoDto` wrapped in `ApiResponse<TodoDto>`

---

## 5. API Endpoints

| Method  | Route                        | Description              | Auth required |
|---------|------------------------------|--------------------------|---------------|
| `GET`   | `/api/todos`                 | Paginated list           | no            |
| `GET`   | `/api/todos/{id}`            | Get single todo          | no            |
| `POST`  | `/api/todos`                 | Create todo              | no            |
| `PUT`   | `/api/todos/{id}`            | Full update (not IsCompleted) | no       |
| `PATCH` | `/api/todos/{id}/toggle`     | Toggle IsCompleted       | no            |
| `DELETE`| `/api/todos/{id}`            | Delete todo (204)        | no            |

---

## 6. Validation Rules

- `Title`: required, not empty, max 200 characters
- `Description`: optional, max 1000 characters
- `DueDate`: optional; if provided, must be in the future (> UTC now)
- `IsCompleted`: not accepted in Create or PUT body — managed via toggle endpoint only

---

## 7. Business Rules

- Toggling a non-existent todo throws `NotFoundException` → 404
- PUT does not accept or modify `IsCompleted` — the field is ignored if present in the request body

### Acceptance Scenarios

**Scenario: Create with valid data (title only)**
- Given: POST `/api/todos` with `{ "title": "Buy milk" }`
- When: the request is processed
- Then: returns 201 with `TodoDto` where `isCompleted = false`, `dueDate = null`

**Scenario: Create with all fields**
- Given: POST `/api/todos` with `{ "title": "Submit report", "description": "Q1 report", "dueDate": "2027-01-01T00:00:00Z" }`
- When: the request is processed
- Then: returns 201 with all fields populated correctly

**Scenario: Create with empty title**
- Given: POST `/api/todos` with `{ "title": "" }`
- When: the request is processed
- Then: returns 400 with a validation error identifying the `Title` field

**Scenario: Create with past DueDate**
- Given: POST `/api/todos` with `{ "title": "Old task", "dueDate": "2020-01-01T00:00:00Z" }`
- When: the request is processed
- Then: returns 400 with a validation error identifying the `DueDate` field

**Scenario: Toggle completion (false → true)**
- Given: a Todo with `id = 1` exists and `isCompleted = false`
- When: PATCH `/api/todos/1/toggle`
- Then: returns 200 with `TodoDto` where `isCompleted = true`

**Scenario: Toggle non-existent todo**
- Given: no Todo with `id = 999` exists
- When: PATCH `/api/todos/999/toggle`
- Then: returns 404 with NotFoundException message

**Scenario: Get by non-existent ID**
- Given: GET `/api/todos/{id}` where no Todo with that ID exists
- When: the request is processed
- Then: returns 404 with NotFoundException message

**Scenario: Get paginated list**
- Given: 25 todos exist in the database
- When: GET `/api/todos?page=2&pageSize=10`
- Then: returns 200 with `PagedResult` containing 10 items, `totalCount = 25`, `totalPages = 3`

---

## 8. Authorization

- none — all endpoints are public

---

## 9. Frontend UI

### Design reference

No Figma design — standard table + form dialog pattern.

### Description

Paginated table page with:
- Page header: "Todos" title on the left, "New Todo" button on the right
- Search bar below the header — search query is stored in Redux (`searchQuery`) but is client-side only; the backend list endpoint has no search parameter
- Table columns: **Title**, **Description** (truncated), **Due Date**, **Status** (badge: "Completed" / "Pending"), **Created At**, **Actions**
- Actions column: dropdown with **Edit**, **Toggle Complete**, and **Delete**
- "New Todo" and "Edit" open a modal form dialog with fields: Title (required), Description (optional textarea), Due Date (optional date picker)
- "Toggle Complete" sends PATCH immediately (no confirmation dialog) and refreshes the list
- "Delete" opens a confirmation dialog; on confirm sends DELETE and refreshes the list
- Skeleton loader while fetching; empty state with message "No todos yet" and a "Create your first todo" button when the list is empty

### 10. Redux UI state

- `searchQuery: string`
- `selectedIds: number[]`

---

## 11. File Locations

### Backend

| File                 | Path                                                                          |
|----------------------|-------------------------------------------------------------------------------|
| Entity               | `backend/src/Backend.Api/Features/Todos/Todo.cs`                              |
| DTOs                 | `backend/src/Backend.Api/Features/Todos/TodoDtos.cs`                          |
| Validator            | `backend/src/Backend.Api/Features/Todos/TodosValidator.cs`                    |
| Repository interface | `backend/src/Backend.Api/Features/Todos/ITodosRepository.cs`                  |
| Repository           | `backend/src/Backend.Api/Features/Todos/TodosRepository.cs`                   |
| Service interface    | `backend/src/Backend.Api/Features/Todos/ITodosService.cs`                     |
| Service              | `backend/src/Backend.Api/Features/Todos/TodosService.cs`                      |
| Controller           | `backend/src/Backend.Api/Features/Todos/TodosController.cs`                   |

### Frontend

| File              | Path                                                                              | Notes |
|-------------------|-----------------------------------------------------------------------------------|-------|
| Page component    | `frontend/src/features/todos/components/todos-page.tsx`                           | |
| Table component   | `frontend/src/features/todos/components/todos-table.tsx`                          | |
| Form dialog       | `frontend/src/features/todos/components/todo-form-dialog.tsx`                     | |
| Delete dialog     | `frontend/src/features/todos/components/todo-delete-dialog.tsx`                   | |
| Pagination hook   | `frontend/src/features/todos/hooks/use-todos-pagination.ts`                       | |
| Form hook         | `frontend/src/features/todos/hooks/use-todo-form.ts`                              | Imports and re-exports `TodoDto` from `@/api/generated/models` |
| Hooks barrel      | `frontend/src/features/todos/hooks/index.ts`                                      | Re-exports hooks |
| Redux slice       | `frontend/src/features/todos/store/todos-slice.ts`                                | |
| Store barrel      | `frontend/src/features/todos/store/index.ts`                                      | Re-exports slice and actions |
| Feature barrel    | `frontend/src/features/todos/index.ts`                                            | Re-exports all public components and hooks |
| Route             | `frontend/src/routes/todos/index.tsx`                                             | |
| Generated API     | `frontend/src/api/generated/todos/`                                               | |

---

## 12. Tests

**Tests written:** yes

### Backend Unit Tests

| Test | Description |
|------|-------------|
| `CreateAsync_WithValidData_ReturnsTodoDto` | Happy path — entity saved and DTO returned |
| `CreateAsync_WithEmptyTitle_ThrowsValidationException` | Validation rejects empty title |
| `CreateAsync_WithPastDueDate_ThrowsValidationException` | Validation rejects past due date |
| `GetAllAsync_ReturnsPaginatedResult` | Correct page and total count |
| `GetByIdAsync_WithInvalidId_ThrowsNotFoundException` | 404 for missing entity |
| `ToggleAsync_FlipsIsCompleted` | false → true, then true → false |
| `ToggleAsync_WithInvalidId_ThrowsNotFoundException` | 404 when todo not found |

### Frontend Tests

| Test | Description |
|------|-------------|
| `TodosPage renders table with rows` | Integration: table populated from mocked query |
| `Form dialog submits create request` | Correct payload sent on submit |
| `Toggle action calls PATCH toggle endpoint` | Action dropdown triggers toggle |

---

## Migration Name

`AddTodoEntity`

---

> **Marker convention:** Both marker types must be resolved before running `/scaffold-feature`:
> - `<!-- TODO: [what is missing] -->` — AI flags missing info that may have a reasonable default
> - `[NEEDS CLARIFICATION: [question] ]` — requires a specific human answer before scaffolding

## Checklist

### Backend
- [x] Entity created in `Features/Todos/Todo.cs`
- [x] DTOs created in `Features/Todos/TodoDtos.cs`
- [x] Validator created in `Features/Todos/TodosValidator.cs`
- [x] Repository interface + implementation created
- [x] Service interface + implementation created
- [x] Controller created with correct routes (including toggle)
- [x] Registered in `Program.cs`
- [x] Migration created and applied

### API Sync
- [x] `npm run api:sync` run from repo root
- [x] `api/generated/todos/` folder generated by Orval

### Frontend
- [x] `features/todos/` folder created with all layers
- [x] Table columns match spec above
- [x] Form fields match spec above
- [x] Redux slice registered in `store/index.ts`
- [x] Route added in `routes/todos/index.tsx`
- [x] Link added to navigation in `components/layout/app-layout.tsx`
