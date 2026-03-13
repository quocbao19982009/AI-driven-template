# Feature Specification: Feature

**Last Updated:** `2026-02-19`
**Tests written:** yes

<!--
This is the FILLED-IN EXAMPLE spec for the built-in Feature entity.
It documents the _FeatureTemplate backend + features/features/ frontend exactly as-is.

Use this as a reference when filling out your own feature-spec-[name].md.
-->

---

## 1. Entity

**Name:** `Feature`
**Table name (plural):** `Features`

### Fields

| Property | C# Type  | Required | Constraints   | Notes |
| -------- | -------- | -------- | ------------- | ----- |
| `Name`   | `string` | yes      | max 200 chars |       |

> `Id` (int), `CreatedAt`, `UpdatedAt` are inherited from `BaseEntity` — do not add them.

---

## Relationships

- none

---

## 2. Core Values & Principles

- Serves as the built-in reference implementation — every pattern here is the canonical example for new features
- Single `Name` field keeps the example minimal so developers focus on the layered architecture, not domain complexity
- All CRUD operations are public (no auth) to simplify onboarding and testing

---

## 3. Architecture Decisions

- Standard layered pattern: Controller → Service → Repository → EF Core
- No custom business logic — this is a pure CRUD reference implementation

---

## 4. Data Flow

### Create

1. Client POST `/api/features` with `{ "name": "..." }`
2. Controller delegates to `FeaturesService.CreateAsync`
3. Service validates via FluentValidation, maps to entity, calls `FeaturesRepository.CreateAsync`
4. Repository saves to DB, returns entity
5. Service maps to `FeatureDto`, returns `ApiResponse<FeatureDto>`

### Read (list)

1. Client GET `/api/features?page=1&pageSize=10`
2. Repository applies `.OrderBy(f => f.Id)`, `.Skip()/.Take()`, returns `PagedResult<Feature>`
3. Service maps to DTOs, returns `ApiResponse<PagedResult<FeatureDto>>`

---

## 5. API Endpoints

| Method   | Route                | Description         | Auth required |
| -------- | -------------------- | ------------------- | ------------- |
| `GET`    | `/api/features`      | Paginated list      | no            |
| `GET`    | `/api/features/{id}` | Get single record   | no            |
| `POST`   | `/api/features`      | Create new record   | no            |
| `PUT`    | `/api/features/{id}` | Full update         | no            |
| `DELETE` | `/api/features/{id}` | Delete record (204) | no            |

---

## 6. Validation Rules

- `Name`: required, not empty, max 200 characters (both Create and Update)

---

## 7. Business Rules

- none

### Acceptance Scenarios

**Scenario: Create with valid data**

- Given: a POST /api/features with Name = "My Feature" (≤ 200 chars)
- When: the request is processed
- Then: returns 201 with the created Feature entity wrapped in ApiResponse<FeatureDto>

**Scenario: Create with empty name**

- Given: a POST /api/features with Name = "" or whitespace only
- When: the request is processed
- Then: returns 400 with a validation error identifying the Name field

**Scenario: Get by non-existent ID**

- Given: a GET /api/features/{id} where no Feature with that ID exists
- When: the request is processed
- Then: returns 404 with NotFoundException message ("Feature {id} not found")

---

## 8. Authorization

- none

---

## 9. Frontend UI

### Design reference

No Figma design — this is the built-in template example.

### Description

Paginated table with header ("Features" title + "New Feature" button). Columns: ID, Name, Created At, and action dropdown (Edit / Delete). Create/Edit opens a modal with a single Name field. Delete opens a confirmation dialog. Skeleton loading while fetching; empty state message when no records.

### 10. Redux UI state

- `searchQuery: string`
- `selectedIds: string[]`

---

## 11. File Locations

### Backend

| File                 | Path                                                                       |
| -------------------- | -------------------------------------------------------------------------- |
| Entity               | `backend/src/Backend.Api/Features/_FeatureTemplate/Feature.cs`             |
| DTOs                 | `backend/src/Backend.Api/Features/_FeatureTemplate/FeatureDtos.cs`         |
| Validator            | `backend/src/Backend.Api/Features/_FeatureTemplate/FeaturesValidator.cs`   |
| Repository interface | `backend/src/Backend.Api/Features/_FeatureTemplate/IFeatureRepository.cs`  |
| Repository           | `backend/src/Backend.Api/Features/_FeatureTemplate/FeatureRepository.cs`   |
| Service interface    | `backend/src/Backend.Api/Features/_FeatureTemplate/IFeatureService.cs`     |
| Service              | `backend/src/Backend.Api/Features/_FeatureTemplate/FeatureService.cs`      |
| Controller           | `backend/src/Backend.Api/Features/_FeatureTemplate/FeatureController.cs`   |

### Frontend

| File            | Path                                                                          |
| --------------- | ----------------------------------------------------------------------------- |
| Page component  | `frontend/src/features/_template-feature/components/feature-page.tsx`         |
| Table component | `frontend/src/features/_template-feature/components/feature-table.tsx`        |
| Form dialog     | `frontend/src/features/_template-feature/components/feature-form-dialog.tsx`  |
| Delete dialog   | `frontend/src/features/_template-feature/components/feature-delete-dialog.tsx`|
| Pagination hook | `frontend/src/features/_template-feature/hooks/use-feature-pagination.ts`     |
| Redux slice     | `frontend/src/features/_template-feature/store/feature-slice.ts`              |
| Route           | `frontend/src/routes/features/index.tsx`                                      |
| Generated API   | `frontend/src/api/generated/features/`                                        |

---

## 12. Tests

**Tests written:** yes

### Backend Unit Tests

| Test                                                 | Description            |
| ---------------------------------------------------- | ---------------------- |
| `CreateAsync_WithValidData_ReturnsFeatureDto`        | Happy path             |
| `GetAllAsync_ReturnsPaginatedResult`                 | Correct page and total |
| `GetByIdAsync_WithInvalidId_ThrowsNotFoundException` | 404 for missing entity |
