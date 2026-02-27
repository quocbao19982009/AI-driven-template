# Feature Spec: Reservations

## 1. Entity

### Reservation
Extends `BaseEntity` (`Id: int`, `CreatedAt: DateTime`, `UpdatedAt: DateTime`)

| Field                  | Type                           | Constraints                                                   |
| ---------------------- | ------------------------------ | ------------------------------------------------------------- |
| `FactoryId`            | `int?`                         | Nullable FK to `Factory`. Set to null when factory is deleted |
| `FactoryDisplayName`   | string                         | Snapshot of factory name at creation time, varchar(200)       |
| `Factory`              | `Factory?`                     | Optional navigation property                                  |
| `StartTime`            | `DateTime`                     | UTC, required                                                 |
| `EndTime`              | `DateTime`                     | UTC, required, must be after `StartTime`                      |
| `ReservationPersonnel` | `ICollection<ReservationPerson>` | Navigation property to assigned personnel                   |

**Database:**
- Table name: `Reservations`
- `FactoryId` FK configured with `OnDelete(DeleteBehavior.SetNull)`

### ReservationPerson
Extends `BaseEntity` (`Id: int`, `CreatedAt: DateTime`, `UpdatedAt: DateTime`)

| Field                 | Type           | Constraints                                                     |
| --------------------- | -------------- | --------------------------------------------------------------- |
| `ReservationId`       | `int`          | Required FK to `Reservation`                                    |
| `PersonId`            | `int?`         | Nullable FK to `Person`. Set to null when person is deleted     |
| `PersonDisplayName`   | string         | Snapshot of person's full name at creation time, varchar(300)   |
| `Reservation`         | `Reservation`  | Navigation property                                             |
| `Person`              | `Person?`      | Optional navigation property                                    |

**Database:**
- Table name: `ReservationPersonnel`
- `PersonId` FK configured with `OnDelete(DeleteBehavior.SetNull)`
- `ReservationId` FK configured with `OnDelete(DeleteBehavior.Cascade)` — deleting a reservation removes its `ReservationPerson` rows

---

## 2. Core Values & Principles

- A reservation represents a time-bounded booking of one or more personnel at a factory.
- Display name snapshots (`FactoryDisplayName`, `PersonDisplayName`) ensure historical reporting accuracy regardless of deletions or renames.
- The system prevents double-booking: no person may have overlapping reservations.
- Personnel eligibility is factory-scoped: only persons with the target factory in their `AllowedFactories` may be assigned.
- Soft-delete cascades from Factory and Person are handled at the FK level — reservation records themselves are never silently destroyed.
- Endpoints are public — no authentication required.

---

## 3. Architecture Decisions

- Standard layered pattern: `Controller → Service → Repository → EF Core`.
- `IReservationsRepository` / `ReservationsRepository` handle all data access.
- `IReservationsService` / `ReservationsService` enforce all business rules: overlap detection, factory-eligibility check, snapshot capture.
- All read queries use `.AsNoTracking()`.
- All paginated queries include `.OrderBy()` before `.Skip()/.Take()`.
- Overlap detection query runs inside the service before any write.
- `ApiResponse<T>` wraps every controller response.
- `CancellationToken` threaded through every async method.
- Registered in `Program.cs`:
  ```csharp
  builder.Services.AddScoped<IReservationsRepository, ReservationsRepository>();
  builder.Services.AddScoped<IReservationsService, ReservationsService>();
  ```

---

## 4. Data Flow

### Create
1. Client POST `/api/reservations` with `CreateReservationRequest { FactoryId, StartTime, EndTime, PersonIds }`.
2. Controller calls `IReservationsService.CreateAsync(request, ct)`.
3. Service validates `StartTime < EndTime`. Throws `ValidationException` if not.
4. Service validates `PersonIds` is non-empty. Throws `ValidationException` if empty.
5. Service loads `Factory` by `FactoryId`. Throws `NotFoundException` if not found.
6. Service loads each `Person` by ID. Throws `ValidationException` for any invalid person ID.
7. Service checks each person's `AllowedFactories` contains the target factory. Throws `ValidationException` listing ineligible persons.
8. Service checks each person for overlapping reservations: `existingReservation.EndTime > request.StartTime && existingReservation.StartTime < request.EndTime`. Throws `ValidationException` listing conflicting persons.
9. Service writes snapshot values: `FactoryDisplayName = factory.Name`, `PersonDisplayName = person.FullName` for each person.
10. Service constructs `Reservation` with `ReservationPersonnel` collection.
11. Repository persists entity, calls `SaveChangesAsync`.
12. Returns `ApiResponse<ReservationDto>.Ok(dto)`.

### Read (Paginated)
1. Client GET `/api/reservations?page=1&pageSize=10&factoryId=&personId=&fromDate=&toDate=`.
2. Repository applies optional filters, `.OrderBy(r => r.StartTime)`, `.Skip()/.Take()`.
3. Repository eagerly loads `ReservationPersonnel` and `Factory`.
4. Service maps to `PagedResult<ReservationDto>`.

### Read (Single)
1. Client GET `/api/reservations/{id}`.
2. Repository loads reservation with `ReservationPersonnel` included. Throws `NotFoundException` if absent.
3. Service maps to `ReservationDto`.

### Update
1. Client PUT `/api/reservations/{id}` with `UpdateReservationRequest { FactoryId, StartTime, EndTime, PersonIds }`.
2. Service loads existing reservation (with personnel). Throws `NotFoundException` if absent.
3. Service re-validates all business rules (StartTime < EndTime, persons eligible, no overlaps excluding the current reservation from overlap check).
4. Service refreshes snapshots if factory or persons have changed.
5. Service replaces `ReservationPersonnel` collection, updates time fields and factory reference.
6. Repository persists, returns updated `ReservationDto`.

### Delete
1. Client DELETE `/api/reservations/{id}`.
2. Service loads reservation. Throws `NotFoundException` if absent.
3. Repository deletes reservation. EF cascade deletes all child `ReservationPerson` rows.
4. Returns `ApiResponse<bool>.Ok(true)`.

---

## 5. API Endpoints

| Method | Route                       | Request Body                 | Response                                 | Description                           |
| ------ | --------------------------- | ---------------------------- | ---------------------------------------- | ------------------------------------- |
| GET    | `/api/reservations`         | —                            | `ApiResponse<PagedResult<ReservationDto>>` | Paginated list with optional filters |
| GET    | `/api/reservations/{id}`    | —                            | `ApiResponse<ReservationDto>`            | Single reservation by ID              |
| POST   | `/api/reservations`         | `CreateReservationRequest`   | `ApiResponse<ReservationDto>`            | Create new reservation                |
| PUT    | `/api/reservations/{id}`    | `UpdateReservationRequest`   | `ApiResponse<ReservationDto>`            | Update existing reservation           |
| DELETE | `/api/reservations/{id}`    | —                            | `ApiResponse<bool>`                      | Delete reservation and its personnel  |

### Query Parameters (GET /api/reservations)
| Param       | Type     | Default | Description                                    |
| ----------- | -------- | ------- | ---------------------------------------------- |
| `page`      | int      | 1       | Page number (1-based)                          |
| `pageSize`  | int      | 10      | Items per page                                 |
| `factoryId` | int?     | —       | Filter by factory ID                           |
| `personId`  | int?     | —       | Filter to reservations that include this person |
| `fromDate`  | DateTime | —       | Filter where `StartTime >= fromDate` (UTC)     |
| `toDate`    | DateTime | —       | Filter where `EndTime <= toDate` (UTC)         |

### DTOs

```csharp
public record CreateReservationRequest(
    int FactoryId,
    DateTime StartTime,
    DateTime EndTime,
    List<int> PersonIds
);

public record UpdateReservationRequest(
    int FactoryId,
    DateTime StartTime,
    DateTime EndTime,
    List<int> PersonIds
);

public record ReservationPersonDto(
    int? PersonId,
    string PersonDisplayName
);

public record ReservationDto(
    int Id,
    int? FactoryId,
    string FactoryDisplayName,
    DateTime StartTime,
    DateTime EndTime,
    double DurationHours,
    List<ReservationPersonDto> Personnel,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
```

`DurationHours` is computed in the service: `(EndTime - StartTime).TotalHours`.

---

## 6. Validation Rules

| Field       | Rules                                                                        |
| ----------- | ---------------------------------------------------------------------------- |
| `FactoryId` | Required, must reference an existing factory                                 |
| `StartTime` | Required, must be before `EndTime`                                           |
| `EndTime`   | Required, must be after `StartTime`                                          |
| `PersonIds` | Required, must contain at least one ID; each ID must reference an existing person |

- FluentValidation validates format/presence for `CreateReservationRequest` and `UpdateReservationRequest`.
- Business-level validations (overlap, eligibility) are enforced in the service layer and return `ValidationException`.

---

## 7. Business Rules

1. **Time ordering:** `StartTime` must be strictly less than `EndTime`. Reservations of zero or negative duration are rejected.
2. **Minimum personnel:** At least one person must be assigned to a reservation.
3. **Factory eligibility:** Every person in `PersonIds` must have the target factory listed in their `AllowedFactories`. Persons not allowed at the factory are rejected with a descriptive error naming the ineligible persons.
4. **No overlapping reservations per person:** For each person in `PersonIds`, the system queries existing `ReservationPerson` records for that person. An overlap exists when:
   ```
   existingReservation.EndTime > newStartTime AND existingReservation.StartTime < newEndTime
   ```
   On update, the current reservation is excluded from the overlap check.
5. **FactoryDisplayName snapshot:** Written once at creation time from `Factory.Name`. Never updated, even if the factory is renamed. This preserves the historical record.
6. **PersonDisplayName snapshot:** Written once at creation time from `Person.FullName`. Never updated, even if the person's name changes.
7. **Soft-delete from Factory:** When a factory is deleted, EF sets `Reservation.FactoryId = null` via `DeleteBehavior.SetNull`. `FactoryDisplayName` is preserved.
8. **Soft-delete from Person:** When a person is deleted, the service sets `ReservationPerson.PersonId = null` for all their records. `PersonDisplayName` is preserved.
9. **Hard-delete of Reservation:** When a reservation itself is deleted, all child `ReservationPerson` rows are cascade-deleted (no soft behaviour — the reservation and its assignments are permanently removed).

---

## 8. Authorization

All reservation endpoints are **public** — no authentication or authorization is required. No `[Authorize]` attributes are applied.

---

## 9. Frontend UI Description

### Route
`/reservations`

### Reservations List Page
- Page header: "Reservations"
- Filter bar:
  - Factory dropdown (populated from `/api/factories/all`, "All Factories" default option)
  - Person dropdown (populated from `/api/personnel/all`, "All Personnel" default option)
  - Date range pickers: "From" and "To" (UTC date inputs)
- "New Reservation" button (opens Create dialog)
- Data table with columns:
  - `Factory` — shows `FactoryDisplayName`; if `FactoryId` is null, displays name with a "Deleted" badge
  - `Start Time` — formatted local datetime
  - `End Time` — formatted local datetime
  - `Duration` — e.g., "4.5 hrs"
  - `Personnel` — list of display name badges; deleted persons shown with "Deleted" badge
  - Actions: Edit icon button, Delete icon button
- Pagination controls

### Create / Edit Dialog
- Modal dialog titled "Create Reservation" or "Edit Reservation"
- Fields:
  - `Factory` — select dropdown (from `/api/factories/all`), required
  - `Start Time` — datetime picker (UTC), required
  - `End Time` — datetime picker (UTC), required
  - `Personnel` — multi-select badge picker. Populated with persons whose `AllowedFactories` includes the selected factory (re-fetches when factory selection changes). Required, min 1.
- Submit button: "Create" or "Save"
- Cancel button
- Server-side validation errors displayed inline

### Delete Dialog
- Confirmation dialog: "Are you sure you want to delete this reservation? All personnel assignments for this reservation will also be permanently deleted."
- Confirm and Cancel buttons

---

## 10. Redux UI State

**Slice:** `reservations` (in `store/slices/reservationsSlice.ts`)

| State Field     | Type       | Purpose                                             |
| --------------- | ---------- | --------------------------------------------------- |
| `factoryFilter` | `number \| null` | Currently selected factory ID filter           |
| `personFilter`  | `number \| null` | Currently selected person ID filter            |
| `selectedIds`   | `number[]` | IDs of rows selected (for future bulk actions)      |

Date range filters are managed as local component state (not Redux) as they are ephemeral UI values tied to the filter bar.

Server state lives exclusively in React Query. Redux holds UI-only filter state.

---

## 11. File Locations

### Backend
```
Features/
  Reservations/
    ReservationsController.cs
    IReservationsService.cs
    ReservationsService.cs
    IReservationsRepository.cs
    ReservationsRepository.cs
    Dtos/
      ReservationDto.cs
      ReservationPersonDto.cs
      CreateReservationRequest.cs
      UpdateReservationRequest.cs
    Validators/
      CreateReservationRequestValidator.cs
      UpdateReservationRequestValidator.cs
```

### Frontend
```
src/
  features/
    reservations/
      ReservationsPage.tsx
      ReservationsTable.tsx
      ReservationFormDialog.tsx
      DeleteReservationDialog.tsx
      ReservationsFilterBar.tsx
      hooks/
        useReservationsQuery.ts       (wraps Orval-generated hook)
        useReservationMutations.ts
  store/
    slices/
      reservationsSlice.ts
  locales/
    en.json                           (reservations.* keys)
    fi.json                           (reservations.* keys)
```

### Generated (do not edit)
```
src/
  api/
    generated/
      reservations.ts                 (Orval-generated — never edit manually)
```

---

## 12. Tests

### Backend Unit Tests

| Test | Description |
| ---- | ----------- |
| `CreateAsync_WithValidData_ReturnsReservationDto` | Happy path: creates with snapshots, returns dto |
| `CreateAsync_WithStartTimeAfterEndTime_ThrowsValidationException` | Time ordering enforced |
| `CreateAsync_WithNoPersonIds_ThrowsValidationException` | Empty personnel list rejected |
| `CreateAsync_WithInvalidFactoryId_ThrowsNotFoundException` | Non-existent factory throws |
| `CreateAsync_WithPersonNotAllowedAtFactory_ThrowsValidationException` | Eligibility check enforced |
| `CreateAsync_WithOverlappingPersonReservation_ThrowsValidationException` | Overlap detection triggers |
| `CreateAsync_SetsFactoryDisplayNameSnapshot` | FactoryDisplayName written from Factory.Name |
| `CreateAsync_SetsPersonDisplayNameSnapshot` | PersonDisplayName written from Person.FullName |
| `GetAllAsync_WithFactoryFilter_ReturnsFilteredResults` | FactoryId filter applied correctly |
| `GetAllAsync_WithPersonFilter_ReturnsFilteredResults` | PersonId filter applied correctly |
| `GetAllAsync_WithDateRangeFilter_ReturnsFilteredResults` | FromDate/ToDate filters applied |
| `GetByIdAsync_WithValidId_ReturnsReservationDtoWithPersonnel` | Includes ReservationPersonnel |
| `GetByIdAsync_WithInvalidId_ThrowsNotFoundException` | Throws for missing reservation |
| `UpdateAsync_ExcludesCurrentReservationFromOverlapCheck` | Self-overlap not triggered on update |
| `UpdateAsync_WithChangedFactory_UpdatesSnapshot` | Snapshot refreshed when factory changes |
| `DeleteAsync_WithValidId_CascadeDeletesReservationPersonnel` | Child rows removed |
| `DeleteAsync_WithInvalidId_ThrowsNotFoundException` | Throws for missing reservation |
| `DurationHours_ComputedCorrectly` | (EndTime - StartTime).TotalHours matches expected |

### Frontend Tests

| Test | Description |
| ---- | ----------- |
| `ReservationsPage renders table with reservation rows` | Integration: table populated from mocked query |
| `Factory filter dropdown updates factoryFilter in Redux` | Filter selection dispatches Redux action |
| `Person filter dropdown updates personFilter in Redux` | Filter selection dispatches Redux action |
| `ReservationFormDialog personnel picker filters by selected factory` | Badge picker re-fetches on factory change |
| `ReservationFormDialog submits correct payload` | PersonIds and factory included in request |
| `ReservationFormDialog shows validation errors inline` | Server errors displayed per field |
| `Deleted factory shown with Deleted badge in table` | Null FactoryId renders badge |
| `Deleted person shown with Deleted badge in personnel list` | Null PersonId renders badge |
| `DeleteReservationDialog shows hard-delete warning` | Cascade delete warning text visible |
| `DeleteReservationDialog calls delete mutation on confirm` | Mutation triggered on confirm |
| `reservationsSlice sets factoryFilter` | Redux slice unit test |
| `reservationsSlice sets personFilter` | Redux slice unit test |
