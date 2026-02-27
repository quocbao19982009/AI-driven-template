# Feature Spec: Personnel

## 1. Entity

**Person** extends `BaseEntity` (`Id: int`, `CreatedAt: DateTime`, `UpdatedAt: DateTime`)

| Field               | Type                     | Constraints                                        |
| ------------------- | ------------------------ | -------------------------------------------------- |
| `PersonalId`        | string                   | Required, unique (case-insensitive), varchar(100)  |
| `FullName`          | string                   | Required, varchar(200)                             |
| `Email`             | string                   | Required, unique (case-insensitive), valid format, varchar(300) |
| `AllowedFactories`  | `ICollection<Factory>`   | Many-to-many via implicit join table `PersonFactory` |

**Database:**
- Table name: `Personnel`
- Unique index on `PersonalId`
- Unique index on `Email`
- Join table `PersonFactory` with columns `PersonId (FK)` and `FactoryId (FK)` — EF Core implicit many-to-many configuration

---

## 2. Core Values & Principles

- A person represents an individual who can be assigned to reservations at one or more allowed factories.
- `PersonalId` is a business identifier (e.g., employee number, badge ID) distinct from the database `Id`.
- `AllowedFactories` restricts which factories a person can be assigned to in a reservation, enforcing a business-level access control rule at the data layer.
- Deletion is soft in impact: the person record is removed but all historical reservation data retains a `PersonDisplayName` snapshot, preserving reporting integrity.
- Endpoints are public — no authentication required.

---

## 3. Architecture Decisions

- Standard layered pattern: `Controller → Service → Repository → EF Core`.
- `IPersonnelRepository` / `PersonnelRepository` handle all data access.
- `IPersonnelService` / `PersonnelService` own business rule enforcement: uniqueness of `PersonalId` and `Email`, and resolution of `AllowedFactoryIds` to factory entities.
- All read queries use `.AsNoTracking()`.
- All paginated queries include `.OrderBy()` before `.Skip()/.Take()`.
- Many-to-many relationship (`Person ↔ Factory`) configured via EF Core implicit join table — no explicit join entity required.
- Uniqueness of `PersonalId` and `Email` enforced at the service layer in addition to DB-level unique indexes.
- `ApiResponse<T>` wraps every controller response.
- `CancellationToken` threaded through every async method.
- Registered in `Program.cs`:
  ```csharp
  builder.Services.AddScoped<IPersonnelRepository, PersonnelRepository>();
  builder.Services.AddScoped<IPersonnelService, PersonnelService>();
  ```

---

## 4. Data Flow

### Create
1. Client POST `/api/personnel` with `CreatePersonRequest { PersonalId, FullName, Email, AllowedFactoryIds }`.
2. Controller calls `IPersonnelService.CreateAsync(request, ct)`.
3. Service checks `PersonalId` uniqueness (case-insensitive). Throws `ValidationException` if duplicate.
4. Service checks `Email` uniqueness (case-insensitive). Throws `ValidationException` if duplicate.
5. Service loads `Factory` entities for each ID in `AllowedFactoryIds`. Throws `ValidationException` for any invalid factory ID.
6. Service constructs `Person` entity with resolved factory collection.
7. Repository adds entity, calls `SaveChangesAsync`.
8. Service maps to `PersonDto` (including `AllowedFactories: List<FactoryDto>`), returns `ApiResponse<PersonDto>.Ok(dto)`.

### Read (Paginated)
1. Client GET `/api/personnel?page=1&pageSize=10&search=...`.
2. Repository applies optional search filter (on `FullName` or `PersonalId`), `.OrderBy(p => p.FullName)`, `.Skip()/.Take()`.
3. Repository eagerly loads `AllowedFactories` via `.Include()`.
4. Service maps to `PagedResult<PersonDto>`.

### Read (Unpaged)
1. Client GET `/api/personnel/all`.
2. Repository returns all persons ordered by `FullName` with `AllowedFactories` included.
3. Service maps to `List<PersonDto>`.

### Update
1. Client PUT `/api/personnel/{id}` with `UpdatePersonRequest { PersonalId, FullName, Email, AllowedFactoryIds }`.
2. Service loads entity (with `AllowedFactories`). Throws `NotFoundException` if not found.
3. Service checks `PersonalId` uniqueness excluding current entity.
4. Service checks `Email` uniqueness excluding current entity.
5. Service resolves new `AllowedFactoryIds` to factory entities, replaces the collection.
6. Repository updates entity, calls `SaveChangesAsync`.
7. Returns updated `PersonDto`.

### Delete
1. Client DELETE `/api/personnel/{id}`.
2. Service loads entity. Throws `NotFoundException` if not found.
3. Service sets `PersonId = null` on all `ReservationPerson` records linked to this person. `PersonDisplayName` snapshots are left untouched.
4. Person record is deleted from the database (cascade deletes rows in `PersonFactory` join table automatically).
5. Returns `ApiResponse<bool>.Ok(true)`.

---

## 5. API Endpoints

| Method | Route                    | Request Body            | Response                               | Description                         |
| ------ | ------------------------ | ----------------------- | -------------------------------------- | ----------------------------------- |
| GET    | `/api/personnel`         | —                       | `ApiResponse<PagedResult<PersonDto>>`  | Paginated list with optional search |
| GET    | `/api/personnel/all`     | —                       | `ApiResponse<List<PersonDto>>`         | Full unpaged list (for dropdowns)   |
| GET    | `/api/personnel/{id}`    | —                       | `ApiResponse<PersonDto>`               | Single person by ID                 |
| POST   | `/api/personnel`         | `CreatePersonRequest`   | `ApiResponse<PersonDto>`               | Create new person                   |
| PUT    | `/api/personnel/{id}`    | `UpdatePersonRequest`   | `ApiResponse<PersonDto>`               | Update existing person              |
| DELETE | `/api/personnel/{id}`    | —                       | `ApiResponse<bool>`                    | Delete person (soft-impact)         |

### Query Parameters (GET /api/personnel)
| Param      | Type   | Default | Description                                       |
| ---------- | ------ | ------- | ------------------------------------------------- |
| `page`     | int    | 1       | Page number (1-based)                             |
| `pageSize` | int    | 10      | Items per page                                    |
| `search`   | string | —       | Optional filter on `FullName` or `PersonalId`     |

### DTOs

```csharp
public record CreatePersonRequest(
    string PersonalId,
    string FullName,
    string Email,
    List<int> AllowedFactoryIds
);

public record UpdatePersonRequest(
    string PersonalId,
    string FullName,
    string Email,
    List<int> AllowedFactoryIds
);

public record PersonDto(
    int Id,
    string PersonalId,
    string FullName,
    string Email,
    List<FactoryDto> AllowedFactories,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
```

---

## 6. Validation Rules

| Field               | Rules                                                              |
| ------------------- | ------------------------------------------------------------------ |
| `PersonalId`        | Required, not empty, max length 100, must be unique               |
| `FullName`          | Required, not empty, max length 200                               |
| `Email`             | Required, not empty, valid email format, max length 300, must be unique |
| `AllowedFactoryIds` | Must be a valid list (empty list is allowed); each ID must exist  |

- FluentValidation validators enforce format rules for `CreatePersonRequest` and `UpdatePersonRequest`.
- Uniqueness of `PersonalId` and `Email` is enforced in the service layer.
- Invalid factory IDs in `AllowedFactoryIds` result in a `ValidationException`.

---

## 7. Business Rules

1. **PersonalId uniqueness:** Must be globally unique across all persons, case-insensitively.
2. **Email uniqueness:** Must be globally unique across all persons, case-insensitively.
3. **AllowedFactories:** A person can be assigned to zero or more factories. The collection controls which factories the person may appear in when building a reservation.
4. **Reservation filtering:** When creating or editing a reservation, only persons whose `AllowedFactories` includes the selected factory are eligible to be assigned.
5. **Soft-delete impact on ReservationPerson:** When a person is deleted:
   - All `ReservationPerson` records where `PersonId = deletedPersonId` have `PersonId` set to `null`.
   - The `PersonDisplayName` snapshot stored on each `ReservationPerson` is never modified — it remains as the permanent historical record.
   - The `PersonFactory` join table rows are removed automatically by EF cascade on entity deletion.
6. **No reservation cascade delete:** Deleting a person does NOT delete any reservations or `ReservationPerson` records — only the `PersonId` foreign key is nulled.

---

## 8. Authorization

All personnel endpoints are **public** — no authentication or authorization is required. No `[Authorize]` attributes are applied.

---

## 9. Frontend UI Description

### Route
`/personnel`

### Personnel List Page
- Page header: "Personnel"
- Search input (debounced, filters on full name or personal ID via `search` query param)
- "New Person" button (opens Create dialog)
- Data table with columns:
  - `ID`
  - `Personal ID`
  - `Full Name`
  - `Email`
  - `Allowed Factories` — rendered as a list of factory name badges
  - Actions: Edit icon button, Delete icon button
- Pagination controls

### Create / Edit Dialog
- Modal dialog titled "Create Person" or "Edit Person"
- Fields:
  - `Personal ID` — text input, required
  - `Full Name` — text input, required
  - `Email` — email input, required
  - `Allowed Factories` — multi-select badge picker. Displays all available factories. Selected factories appear as removable badges. Source data from `/api/factories/all`.
- Submit button: "Create" or "Save"
- Cancel button
- Server-side validation errors displayed inline

### Delete Dialog
- Confirmation dialog: "Are you sure you want to delete {FullName}? This person will be removed from all reservations but reservation history will be preserved."
- Confirm and Cancel buttons

---

## 10. Redux UI State

**Slice:** `personnel` (in `store/slices/personnelSlice.ts`)

| State Field   | Type       | Purpose                                         |
| ------------- | ---------- | ----------------------------------------------- |
| `searchQuery` | `string`   | Current value of the search input               |
| `selectedIds` | `number[]` | IDs of rows selected (for future bulk actions)  |

Server state (person list, single person, factories list for picker) lives exclusively in React Query. Redux holds UI-only state.

---

## 11. File Locations

### Backend
```
Features/
  Personnel/
    PersonnelController.cs
    IPersonnelService.cs
    PersonnelService.cs
    IPersonnelRepository.cs
    PersonnelRepository.cs
    Dtos/
      PersonDto.cs
      CreatePersonRequest.cs
      UpdatePersonRequest.cs
    Validators/
      CreatePersonRequestValidator.cs
      UpdatePersonRequestValidator.cs
```

### Frontend
```
src/
  features/
    personnel/
      PersonnelPage.tsx
      PersonnelTable.tsx
      PersonFormDialog.tsx
      DeletePersonDialog.tsx
      hooks/
        usePersonnelQuery.ts        (wraps Orval-generated hook)
        usePersonnelMutations.ts
  store/
    slices/
      personnelSlice.ts
  locales/
    en.json                         (personnel.* keys)
    fi.json                         (personnel.* keys)
```

### Generated (do not edit)
```
src/
  api/
    generated/
      personnel.ts                  (Orval-generated — never edit manually)
```

---

## 12. Tests

### Backend Unit Tests

| Test | Description |
| ---- | ----------- |
| `CreateAsync_WithValidData_ReturnsPersonDto` | Happy path: creates and returns dto with factories |
| `CreateAsync_WithDuplicatePersonalId_ThrowsValidationException` | Duplicate PersonalId throws |
| `CreateAsync_WithDuplicatePersonalIdDifferentCase_ThrowsValidationException` | Case-insensitive PersonalId check |
| `CreateAsync_WithDuplicateEmail_ThrowsValidationException` | Duplicate Email throws |
| `CreateAsync_WithInvalidFactoryId_ThrowsValidationException` | Non-existent factory ID in list throws |
| `GetAllAsync_ReturnsPaginatedResultWithFactories` | Returns page with AllowedFactories populated |
| `GetByIdAsync_WithValidId_ReturnsPersonDtoWithFactories` | Returns dto with factory list |
| `GetByIdAsync_WithInvalidId_ThrowsNotFoundException` | Throws for missing person |
| `UpdateAsync_WithValidData_UpdatesAllowedFactories` | Factory list is replaced correctly |
| `UpdateAsync_WithDuplicateEmailExcludingSelf_ThrowsValidationException` | Email clash excludes current entity |
| `DeleteAsync_WithValidId_NullsPersonIdOnReservationPerson` | ReservationPerson.PersonId set to null |
| `DeleteAsync_WithValidId_PreservesPersonDisplayName` | PersonDisplayName snapshot unchanged |
| `DeleteAsync_WithInvalidId_ThrowsNotFoundException` | Throws for missing person |

### Frontend Tests

| Test | Description |
| ---- | ----------- |
| `PersonnelPage renders table with person rows` | Integration: table populated from mocked query |
| `Search input filters personnel` | Debounced search updates query param |
| `PersonFormDialog renders factory badge picker` | Factories loaded and displayed as badges |
| `PersonFormDialog submits create request with AllowedFactoryIds` | Correct payload sent on submit |
| `PersonFormDialog shows server validation errors inline` | Error messages appear below fields |
| `DeletePersonDialog shows soft-delete warning message` | Warning text visible in confirmation |
| `DeletePersonDialog calls delete mutation on confirm` | Mutation triggered on confirm |
| `personnelSlice sets searchQuery` | Redux slice unit test |
