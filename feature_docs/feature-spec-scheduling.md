# Feature Spec: Scheduling

## 1. Entity

Scheduling has **no dedicated entity**. It is a read-only aggregation layer that reads directly from `Reservation`, `ReservationPerson`, `Factory`, and `Person` via `ApplicationDbContext`. No migrations, no new tables.

---

## 2. Core Values & Principles

- Scheduling is a reporting view, not a data-management feature. It surfaces derived metrics (total hours, reservation counts) from existing reservation data.
- Because Factory and Person records can be deleted with their IDs nulled on reservations, the scheduling view must handle null IDs gracefully — deleted entities are shown using their display name snapshots with a "Deleted" indicator.
- Data is always fresh from the database — no caching layer, no Redux state. Every page load triggers a new query.
- The two views (By Person / By Factory) answer different operational questions: "Who is working and when?" vs. "How loaded is each factory?"
- Endpoints are public — no authentication required.

---

## 3. Architecture Decisions

- Scheduling bypasses the standard repository pattern. The `SchedulingController` depends directly on `ApplicationDbContext` (or a thin `SchedulingQueryService`) to write expressive LINQ projections without forcing artificial repository abstractions for read-only aggregations.
- No `ISchedulingRepository` — this is intentional. The feature is query-only and does not mutate data.
- An optional `SchedulingQueryService` (injected as `Scoped`) can be introduced to keep the controller thin and enable unit testing of LINQ projections.
- All queries use `.AsNoTracking()`.
- No pagination — both endpoints return full result sets. Data volume is bounded by factory and personnel counts, which are expected to remain small.
- `ApiResponse<T>` wraps every controller response.
- `CancellationToken` threaded through every async method.
- Registered in `Program.cs` (if service class is used):
  ```csharp
  builder.Services.AddScoped<SchedulingQueryService>();
  ```

---

## 4. Data Flow

### By Person
1. Client GET `/api/scheduling/by-person`.
2. Controller (or `SchedulingQueryService`) queries `ReservationPerson` joined with `Reservation` and groups by `(PersonId, PersonDisplayName)`.
3. For each group, builds a list of `ReservationSummaryDto` and sums `DurationHours`.
4. Persons whose `PersonId` is null (deleted) are grouped by `PersonDisplayName` — they appear as separate entries with `PersonId = null`.
5. Returns `ApiResponse<List<PersonScheduleDto>>.Ok(result)` ordered by `PersonName`.

### By Factory
1. Client GET `/api/scheduling/by-factory`.
2. Controller queries `Reservation` grouped by `(FactoryId, FactoryDisplayName)`.
3. For each group, counts reservations and computes `TotalHours = sum of (DurationHours * PersonCount)` where `PersonCount` is the number of `ReservationPerson` rows per reservation.
4. Reservations whose `FactoryId` is null (deleted factory) are grouped by `FactoryDisplayName`.
5. Returns `ApiResponse<List<FactoryScheduleDto>>.Ok(result)` ordered by `FactoryName`.

### Duration Calculation
```
DurationHours = (Reservation.EndTime - Reservation.StartTime).TotalHours
```

For By Factory, `TotalHours` represents total person-hours:
```
TotalHours = Sum over all reservations: DurationHours * NumberOfPersonsInThatReservation
```

---

## 5. API Endpoints

| Method | Route                         | Request Body | Response                                | Description                                      |
| ------ | ----------------------------- | ------------ | --------------------------------------- | ------------------------------------------------ |
| GET    | `/api/scheduling/by-person`   | —            | `ApiResponse<List<PersonScheduleDto>>`  | All persons with their reservations and hours    |
| GET    | `/api/scheduling/by-factory`  | —            | `ApiResponse<List<FactoryScheduleDto>>` | All factories with reservation count and hours   |

### DTOs

```csharp
public record ReservationSummaryDto(
    int ReservationId,
    DateTime StartTime,
    DateTime EndTime,
    double DurationHours,
    string FactoryName
);

public record PersonScheduleDto(
    int? PersonId,
    string PersonName,
    List<ReservationSummaryDto> Reservations,
    double TotalHours
);

public record FactoryScheduleDto(
    int? FactoryId,
    string FactoryName,
    int ReservationCount,
    double TotalHours
);
```

**Notes:**
- `PersonId` is `int?` — null indicates the person record has been deleted.
- `FactoryId` is `int?` — null indicates the factory record has been deleted.
- `PersonName` comes from `ReservationPerson.PersonDisplayName` (the snapshot, not a live join to `Person.FullName`).
- `FactoryName` in `ReservationSummaryDto` comes from `Reservation.FactoryDisplayName` (snapshot).
- `FactoryName` in `FactoryScheduleDto` comes from `Reservation.FactoryDisplayName` (snapshot).

---

## 6. Validation Rules

No input validation — both endpoints accept no request body or query parameters. The data returned is derived entirely from the existing reservation dataset.

---

## 7. Business Rules

1. **Duration computation:** `DurationHours = (Reservation.EndTime - Reservation.StartTime).TotalHours`. Computed in memory after the query returns, or as a projected LINQ expression.

2. **By-Factory TotalHours semantics:** Represents total person-hours, not calendar hours. A 4-hour reservation with 3 persons contributes 12 person-hours to the factory total.
   ```
   TotalHours = Sum(DurationHours_i * PersonCount_i)
   ```

3. **By-Person TotalHours semantics:** Represents the total calendar hours that person is booked across all their reservations (sum of individual reservation durations).

4. **Deleted factory handling:** Reservations with `FactoryId = null` are grouped under their `FactoryDisplayName` snapshot. They appear in the By-Factory view with `FactoryId = null` and the preserved display name. The frontend renders a "Deleted" badge.

5. **Deleted person handling:** `ReservationPerson` records with `PersonId = null` are grouped by `PersonDisplayName`. They appear in the By-Person view with `PersonId = null`. The frontend renders a "Deleted" badge.

6. **Ordering:** By-Person results ordered by `PersonName` ascending. By-Factory results ordered by `FactoryName` ascending.

7. **No deduplication of deleted snapshots:** If multiple deleted persons shared the same `PersonDisplayName`, they will appear as separate entries in the By-Person view (grouped by `PersonId`, which is null for all). This is an acceptable edge case for a deleted-data scenario.

---

## 8. Authorization

All scheduling endpoints are **public** — no authentication or authorization is required. No `[Authorize]` attributes are applied.

---

## 9. Frontend UI Description

### Route
`/scheduling`

### Scheduling Page
- Page header: "Scheduling"
- Two-tab layout:
  - Tab 1: "By Person"
  - Tab 2: "By Factory"

### By Person Tab
- Renders a list of expandable cards, one per person.
- Card header (collapsed state):
  - Person name (`PersonName`)
  - If `PersonId` is null: "Deleted" badge rendered next to the name
  - Total hours summary: e.g., "Total: 12.5 hrs"
- Card expanded state (click to expand):
  - Table of reservations for that person with columns:
    - `Start Time` — formatted local datetime
    - `End Time` — formatted local datetime
    - `Duration` — e.g., "4.0 hrs"
    - `Factory` — from `FactoryName` in `ReservationSummaryDto`
- Cards are ordered alphabetically by person name.
- Empty state: "No scheduling data available."

### By Factory Tab
- Renders a data table with columns:
  - `Factory` — `FactoryName`; if `FactoryId` is null, renders name with "Deleted" badge
  - `Reservations` — `ReservationCount`
  - `Total Hours` — `TotalHours` formatted to 1 decimal place (e.g., "36.0 hrs")
- Table is ordered alphabetically by factory name.
- Empty state: "No scheduling data available."

### General
- No search, filter, or pagination controls — the view always shows the full dataset.
- No create/edit/delete actions — scheduling is read-only.
- Data is fetched on page mount via React Query (no manual refresh button required).
- Loading state: skeleton or spinner while query is in flight.

---

## 10. Redux UI State

**No Redux state for Scheduling.**

The scheduling feature is purely read-only with no user-driven filter or selection state. All data lives in React Query. The active tab (By Person / By Factory) is managed as local component state (`useState`) within the `SchedulingPage` component — it does not need to persist across navigation.

---

## 11. File Locations

### Backend

| File | Path |
|------|------|
| DTOs | `backend/src/Backend.Api/Features/Scheduling/SchedulingDtos.cs` |
| Controller | `backend/src/Backend.Api/Features/Scheduling/SchedulingController.cs` |

No service or repository files — the controller queries `ApplicationDbContext` directly.

### Frontend

| File | Path |
|------|------|
| Page component | `frontend/src/features/scheduling/components/scheduling-page.tsx` |
| Page test | `frontend/src/features/scheduling/components/__tests__/scheduling-page.test.tsx` |
| Generated API | `frontend/src/api/generated/scheduling/` |

No Redux slice — no UI state needed. Tab state managed via local `useState`.

---

## 12. Tests

### Backend Unit Tests

All tests target `SchedulingQueryService` (or the controller action directly if no service class is used), using an in-memory EF Core context seeded with test data.

| Test | Description |
| ---- | ----------- |
| `GetByPerson_WithReservations_ReturnsPersonScheduleDtoList` | Happy path: correct persons, reservation lists, and totals |
| `GetByPerson_TotalHours_SumsReservationDurations` | TotalHours = sum of (EndTime - StartTime).TotalHours per person |
| `GetByPerson_WithDeletedPerson_ReturnsNullPersonId` | ReservationPerson with PersonId=null appears with PersonId=null in result |
| `GetByPerson_WithDeletedPerson_UsesPersonDisplayNameSnapshot` | PersonName comes from PersonDisplayName, not live Person.FullName |
| `GetByPerson_WithDeletedFactory_UsesFactoryDisplayNameInSummary` | FactoryName in ReservationSummaryDto comes from FactoryDisplayName snapshot |
| `GetByPerson_OrderedByPersonNameAscending` | Result list sorted alphabetically |
| `GetByFactory_WithReservations_ReturnsFactoryScheduleDtoList` | Happy path: correct factories, counts, and total hours |
| `GetByFactory_TotalHours_IsPersonHours` | TotalHours = sum of (duration * personCount) per reservation |
| `GetByFactory_WithDeletedFactory_ReturnsNullFactoryId` | Reservation with FactoryId=null appears with FactoryId=null in result |
| `GetByFactory_WithDeletedFactory_UsesFactoryDisplayNameSnapshot` | FactoryName comes from FactoryDisplayName snapshot |
| `GetByFactory_OrderedByFactoryNameAscending` | Result list sorted alphabetically |
| `GetByFactory_ReservationCount_MatchesNumberOfReservations` | ReservationCount is accurate per factory group |

### Frontend Tests

| Test | Description |
| ---- | ----------- |
| `SchedulingPage renders By Person tab by default` | Default active tab is "By Person" |
| `SchedulingPage switches to By Factory tab on click` | Tab switch renders ByFactoryTab |
| `ByPersonTab renders expandable cards per person` | One card per PersonScheduleDto |
| `ByPersonTab expands card to show reservation table` | Click on card shows reservation rows |
| `ByPersonTab shows Deleted badge for null PersonId` | Badge visible when PersonId is null |
| `ByPersonTab displays TotalHours in card header` | Total formatted correctly |
| `ByFactoryTab renders table with factory rows` | Rows match mocked FactoryScheduleDto list |
| `ByFactoryTab shows Deleted badge for null FactoryId` | Badge visible when FactoryId is null |
| `ByFactoryTab displays TotalHours to one decimal place` | "36.0 hrs" format verified |
| `SchedulingPage shows empty state when no data` | "No scheduling data available." rendered |
| `SchedulingPage shows loading state while query is in flight` | Skeleton or spinner visible |
