# Feature Spec: Factories

## 1. Entity

**Factory** extends `BaseEntity` (`Id: int`, `CreatedAt: DateTime`, `UpdatedAt: DateTime`)

| Field      | Type   | Constraints                                      |
| ---------- | ------ | ------------------------------------------------ |
| `Name`     | string | Required, unique (case-insensitive), varchar(200) |
| `TimeZone` | string | Required, IANA timezone identifier, varchar(100) |

**Database:**
- Unique index on `Name` (case-insensitive collation or handled at the service layer)
- Table name: `Factories`

---

## 2. Core Values & Principles

- Factories represent physical locations where reservations take place.
- A factory name must be globally unique to prevent ambiguity in reservations and scheduling views.
- TimeZone is stored as an IANA timezone ID (e.g., `Europe/Helsinki`, `America/New_York`) to support accurate local-time display across regions.
- Deletion is soft in impact: the factory record is removed, but all historical reservations retain a `FactoryDisplayName` snapshot so reporting is never broken.
- Endpoints are public — no authentication is required, consistent with the overall system's open-access design.

---

## 3. Architecture Decisions

- Follows the standard layered pattern: `Controller → Service → Repository → EF Core`.
- `IFactoriesRepository` / `FactoriesRepository` handle all data access; the service layer owns business rule enforcement (uniqueness check).
- All read queries use `.AsNoTracking()` per project convention.
- All paginated queries include `.OrderBy()` before `.Skip()/.Take()`.
- Uniqueness of `Name` is enforced in the service layer (case-insensitive `string.Equals` or `.ToLower()` comparison) in addition to a DB-level unique index.
- `ApiResponse<T>` wraps every controller response.
- `CancellationToken` is threaded through every async method.
- Feature registered in `Program.cs` with `AddScoped<IFactoriesRepository, FactoriesRepository>()` and `AddScoped<IFactoriesService, FactoriesService>()`.

---

## 4. Data Flow

### Create
1. Client POST `/api/factories` with `CreateFactoryRequest { Name, TimeZone }`.
2. Controller calls `IFactoriesService.CreateAsync(request, ct)`.
3. Service checks for existing factory with same name (case-insensitive). Throws `ValidationException` if duplicate.
4. Service maps request to `Factory` entity, calls `IFactoriesRepository.CreateAsync(entity, ct)`.
5. Repository adds entity, calls `SaveChangesAsync`.
6. Service maps entity to `FactoryDto`, returns `ApiResponse<FactoryDto>.Ok(dto)`.

### Read (Paginated)
1. Client GET `/api/factories?page=1&pageSize=10&search=...`.
2. Repository applies optional name filter, `.OrderBy(f => f.Name)`, `.Skip()/.Take()`, returns `PagedResult<Factory>`.
3. Service maps to `PagedResult<FactoryDto>`.

### Read (Unpaged)
1. Client GET `/api/factories/all`.
2. Repository returns all factories ordered by name as `List<Factory>`.
3. Service maps to `List<FactoryDto>`.

### Update
1. Client PUT `/api/factories/{id}` with `UpdateFactoryRequest { Name, TimeZone }`.
2. Service loads entity. Throws `NotFoundException` if not found.
3. Service checks uniqueness of new Name excluding current entity. Throws `ValidationException` if duplicate.
4. Service updates fields, calls repository `UpdateAsync`.
5. Service queries all `Reservation` rows where `FactoryId == entity.Id` and sets `FactoryDisplayName = request.Name` on each. Calls `_context.SaveChangesAsync` if any rows were updated.
6. Returns updated `FactoryDto`.

### Delete
1. Client DELETE `/api/factories/{id}`.
2. Service loads entity. Throws `NotFoundException` if not found.
3. Repository (or service via EF) sets `Reservation.FactoryId = null` for all reservations linked to this factory. The `FactoryDisplayName` snapshot on each reservation is left untouched.
4. Factory record is deleted from the database.
5. Returns `ApiResponse<bool>.Ok(true)`.

---

## 5. API Endpoints

| Method | Route                   | Request Body            | Response                      | Description                        |
| ------ | ----------------------- | ----------------------- | ----------------------------- | ---------------------------------- |
| GET    | `/api/factories`        | —                       | `ApiResponse<PagedResult<FactoryDto>>` | Paginated list with optional search |
| GET    | `/api/factories/all`    | —                       | `ApiResponse<List<FactoryDto>>`        | Full unpaged list (for dropdowns)   |
| GET    | `/api/factories/{id}`   | —                       | `ApiResponse<FactoryDto>`              | Single factory by ID               |
| POST   | `/api/factories`        | `CreateFactoryRequest`  | `ApiResponse<FactoryDto>`              | Create new factory                 |
| PUT    | `/api/factories/{id}`   | `UpdateFactoryRequest`  | `ApiResponse<FactoryDto>`              | Update existing factory             |
| DELETE | `/api/factories/{id}`   | —                       | `ApiResponse<bool>`                    | Delete factory (soft-impact)        |

### Query Parameters (GET /api/factories)
| Param      | Type   | Default | Description                        |
| ---------- | ------ | ------- | ---------------------------------- |
| `page`     | int    | 1       | Page number (1-based)              |
| `pageSize` | int    | 10      | Items per page                     |
| `search`   | string | —       | Optional name filter (contains)    |

### DTOs

```csharp
public record CreateFactoryRequest(string Name, string TimeZone);
public record UpdateFactoryRequest(string Name, string TimeZone);

public record FactoryDto(int Id, string Name, string TimeZone, DateTime CreatedAt, DateTime UpdatedAt);
```

---

## 6. Validation Rules

| Field      | Rules                                                         |
| ---------- | ------------------------------------------------------------- |
| `Name`     | Required, not empty, max length 200                           |
| `TimeZone` | Required, not empty, max length 100                           |

- Validation is enforced via FluentValidation validators for `CreateFactoryRequest` and `UpdateFactoryRequest`.
- Name uniqueness (case-insensitive) is enforced in the service layer and returns a `ValidationException`.

---

## 7. Business Rules

1. **Name uniqueness:** Factory names must be unique across the system, enforced case-insensitively (e.g., `"Factory A"` and `"factory a"` are considered duplicates).
2. **Soft-delete impact on Reservations:** When a factory is deleted, all `Reservation` records that reference this factory have their `FactoryId` set to `null`. The `FactoryDisplayName` column on each `Reservation` is kept in sync with the factory's current name: `FactoriesService.UpdateAsync` cascades renames to all linked reservations. On factory deletion, the last known name is preserved (frozen) as the permanent historical record.
3. **TimeZone validity:** The `TimeZone` field is stored as-provided. The system does not validate that the IANA ID maps to a known timezone at the API layer, but the frontend restricts input to a known IANA timezone list.
4. **Cascade null on delete:** Implemented via EF Core's `OnDelete(DeleteBehavior.SetNull)` on `Reservation.FactoryId` foreign key configuration.

---

## 8. Authorization

All factory endpoints are **public** — no authentication or authorization is required. No `[Authorize]` attributes are applied to the controller.

---

## 9. Frontend UI Description

### Route
`/factories`

### Factory List Page
- Page header: "Factories"
- Search input (debounced, filters by name via `search` query param)
- "New Factory" button (opens Create dialog)
- Data table with columns:
  - `ID` (sortable)
  - `Name` (sortable)
  - `Time Zone`
  - `Created At` (formatted date)
  - Actions: Edit icon button, Delete icon button
- Pagination controls

### Create / Edit Dialog
- Modal dialog titled "Create Factory" or "Edit Factory"
- Fields:
  - `Name` — text input, required
  - `Time Zone` — text input or select from IANA timezone list, required
- Submit button: "Create" or "Save"
- Cancel button
- Displays server-side validation errors inline
- On successful update, React Query invalidates factories, personnel, **and reservations** queries to reflect the renamed factory in the reservations table immediately.

### Delete Dialog
- Confirmation dialog: "Are you sure you want to delete {Name}? All reservations associated with this factory will lose the factory link but will retain the display name."
- Confirm and Cancel buttons

---

## 10. Redux UI State

**Slice:** `factories` (in `store/slices/factoriesSlice.ts`)

| State Field   | Type       | Purpose                                        |
| ------------- | ---------- | ---------------------------------------------- |
| `searchQuery` | `string`   | Current value of the search input              |
| `selectedIds` | `number[]` | IDs of rows selected (for future bulk actions) |

Server state (factory list, single factory) lives exclusively in React Query via Orval-generated hooks. Redux holds UI-only state.

---

## 11. File Locations

### Backend
```
Features/
  Factories/
    FactoriesController.cs
    IFactoriesService.cs
    FactoriesService.cs
    IFactoriesRepository.cs
    FactoriesRepository.cs
    Dtos/
      FactoryDto.cs
      CreateFactoryRequest.cs
      UpdateFactoryRequest.cs
    Validators/
      CreateFactoryRequestValidator.cs
      UpdateFactoryRequestValidator.cs
```

### Frontend
```
src/
  features/
    factories/
      FactoriesPage.tsx
      FactoriesTable.tsx
      FactoryFormDialog.tsx
      DeleteFactoryDialog.tsx
      hooks/
        useFactoriesQuery.ts      (wraps Orval-generated hook)
        useFactoryMutations.ts
  store/
    slices/
      factoriesSlice.ts
  locales/
    en.json                       (factories.* keys)
    fi.json                       (factories.* keys)
```

### Generated (do not edit)
```
src/
  api/
    generated/
      factories.ts                (Orval-generated — never edit manually)
```

---

## 12. Tests

### Backend Unit Tests

| Test | Description |
| ---- | ----------- |
| `CreateAsync_WithUniqueName_ReturnsFactoryDto` | Happy path: creates and returns dto |
| `CreateAsync_WithDuplicateName_ThrowsValidationException` | Duplicate name (case-insensitive) throws |
| `CreateAsync_WithDuplicateNameDifferentCase_ThrowsValidationException` | Case-insensitive check |
| `GetAllAsync_ReturnsPaginatedResult` | Returns correct page and total count |
| `GetByIdAsync_WithValidId_ReturnsFactoryDto` | Returns dto for existing factory |
| `GetByIdAsync_WithInvalidId_ThrowsNotFoundException` | Throws for missing factory |
| `UpdateAsync_WithValidData_ReturnsUpdatedDto` | Update succeeds |
| `UpdateAsync_WithDuplicateName_ThrowsValidationException` | Name clash on update throws |
| `DeleteAsync_WithValidId_NullsReservationFactoryId` | Reservations have FactoryId set to null |
| `DeleteAsync_WithValidId_PreservesFactoryDisplayName` | FactoryDisplayName snapshot is unchanged |
| `DeleteAsync_WithInvalidId_ThrowsNotFoundException` | Throws for missing factory |

### Frontend Tests

| Test | Description |
| ---- | ----------- |
| `FactoriesPage renders table with factory rows` | Integration: table populated from mocked query |
| `Search input filters factories` | Debounced search updates query param |
| `FactoryFormDialog submits create request` | Form submits with correct payload |
| `FactoryFormDialog shows validation errors` | Server errors displayed inline |
| `DeleteFactoryDialog calls delete mutation on confirm` | Confirm triggers mutation |
| `factoriesSlice sets searchQuery` | Redux slice unit test |
