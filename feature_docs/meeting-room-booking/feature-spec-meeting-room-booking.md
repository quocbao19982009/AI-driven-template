# Feature Specification: Meeting Room Booking

**Last Updated:** `2026-03-06` (implementation complete)
**Tests written:** no
**Status:** Implemented, deployed, migration applied

---

## 1. Entities

This feature has three related entities: **Location**, **Room**, and **Booking**.

### Entity Relationship Diagram

```mermaid
erDiagram
    Location {
        int Id PK
        string Name
        DateTime CreatedAt
        DateTime UpdatedAt
    }

    Room {
        int Id PK
        string Name
        int Capacity
        int LocationId FK
        string Purpose
        string ImagePath
        DateTime CreatedAt
        DateTime UpdatedAt
    }

    Booking {
        int Id PK
        int RoomId FK
        DateTime StartTime
        DateTime EndTime
        string BookedBy
        string Purpose
        DateTime CreatedAt
        DateTime UpdatedAt
    }

    Location ||--o{ Room : "has many"
    Room ||--o{ Booking : "has many"
```

---

### Location

**Table name:** `Locations`

| Property | C# Type  | Required | Constraints   | Notes |
| -------- | -------- | -------- | ------------- | ----- |
| `Name`   | `string` | yes      | max 200 chars |       |

> `Id`, `CreatedAt`, `UpdatedAt` inherited from `BaseEntity`.

---

### Room

**Table name:** `Rooms`

| Property     | C# Type  | Required | Constraints   | Notes                                        |
| ------------ | -------- | -------- | ------------- | -------------------------------------------- |
| `Name`       | `string` | yes      | max 200 chars |                                              |
| `Capacity`   | `int`    | yes      | min 1         |                                              |
| `LocationId` | `int`    | yes      | FK → Location |                                              |
| `Purpose`    | `string` | no       | max 500 chars | Free-text description of intended room use   |
| `ImagePath`  | `string` | no       | max 500 chars | Relative path under `wwwroot/uploads/rooms/` |

> `Id`, `CreatedAt`, `UpdatedAt` inherited from `BaseEntity`.

---

### Booking

**Table name:** `Bookings`

| Property    | C# Type    | Required | Constraints   | Notes                              |
| ----------- | ---------- | -------- | ------------- | ---------------------------------- |
| `RoomId`    | `int`      | yes      | FK → Room     |                                    |
| `StartTime` | `DateTime` | yes      |               | Stored as UTC                      |
| `EndTime`   | `DateTime` | yes      |               | Stored as UTC; must be > StartTime |
| `BookedBy`  | `string`   | yes      | max 200 chars | Name or identifier of booker       |
| `Purpose`   | `string`   | no       | max 500 chars | Reason for the booking             |

> `Id`, `CreatedAt`, `UpdatedAt` inherited from `BaseEntity`.

---

## EF Core Configuration Notes

```csharp
// Restrict delete: prevent deleting a Location if Rooms reference it
modelBuilder.Entity<Room>()
    .HasOne(r => r.Location)
    .WithMany(l => l.Rooms)
    .HasForeignKey(r => r.LocationId)
    .OnDelete(DeleteBehavior.Restrict);

// Restrict delete: prevent deleting a Room if Bookings reference it
modelBuilder.Entity<Booking>()
    .HasOne(b => b.Room)
    .WithMany(r => r.Bookings)
    .HasForeignKey(b => b.RoomId)
    .OnDelete(DeleteBehavior.Restrict);
```

---

## 2. Core Values & Principles

- A booking must always represent a valid, non-overlapping time window for a specific room.
- Room images are optional; the system stores only the file path — not the binary blob.
- Locations are reference data: they must exist before rooms can be assigned to them.
- Rooms with existing bookings cannot be deleted; locations with existing rooms cannot be deleted.
- All times are stored and returned as UTC.

---

## 3. Architecture Decisions

- Standard layered pattern: Controller → Service → Repository → EF Core for all three entities.
- Image upload handled in `RoomsController` via `IFormFile`; service delegates file I/O to a `IFileStorageService` helper (saves to `wwwroot/uploads/rooms/<guid>.<ext>`).
- Static files served via `app.UseStaticFiles()` in `Program.cs` — no separate file controller needed.
- Booking overlap check is performed in `BookingsService` using a DB query before insert/update.
- Locations use an unpaged list endpoint (small reference data); Rooms and Bookings are paginated.

---

## 4. Data Flow

### Rooms — Create (with image)

1. Client POST `/api/rooms` as `multipart/form-data` (fields + optional file).
2. Controller binds `[FromForm] CreateRoomRequest` and optional `IFormFile image`.
3. Service validates request, saves image via `IFileStorageService` → returns relative path.
4. Service maps to `Room` entity, saves via repository.
5. Returns `ApiResponse<RoomDto>` with 201.

### Rooms — Update (with image replacement)

1. Client PUT `/api/rooms/{id}` as `multipart/form-data`.
2. Service loads existing room, deletes old image file if a new one is provided.
3. Saves new image, updates entity fields, persists.

### Bookings — Create (overlap check)

1. Client POST `/api/bookings` with JSON body.
2. Service validates `StartTime < EndTime`.
3. Service queries DB: any booking for same `RoomId` where `StartTime < req.EndTime && EndTime > req.StartTime` → throws `ValidationException` if overlap found.
4. Saves booking, returns `ApiResponse<BookingDto>`.

---

## 5. API Endpoints

### Locations

| Method   | Route                 | Description       | Auth required | Body / Params |
| -------- | --------------------- | ----------------- | ------------- | ------------- |
| `GET`    | `/api/locations`      | Full unpaged list | no            | —             |
| `GET`    | `/api/locations/{id}` | Get single        | no            | —             |
| `POST`   | `/api/locations`      | Create            | no            | `{ name }`    |
| `PUT`    | `/api/locations/{id}` | Update            | no            | `{ name }`    |
| `DELETE` | `/api/locations/{id}` | Delete            | no            | —             |

### Rooms

| Method   | Route             | Description                  | Auth required | Body / Params                                                    |
| -------- | ----------------- | ---------------------------- | ------------- | ---------------------------------------------------------------- |
| `GET`    | `/api/rooms`      | Paginated list               | no            | `?page`, `pageSize`, `search`, `locationId`, `sortBy`, `sortDir` |
| `GET`    | `/api/rooms/all`  | Unpaged list (for dropdowns) | no            | —                                                                |
| `GET`    | `/api/rooms/{id}` | Get single                   | no            | —                                                                |
| `POST`   | `/api/rooms`      | Create (multipart/form-data) | no            | fields + optional `image` file                                   |
| `PUT`    | `/api/rooms/{id}` | Update (multipart/form-data) | no            | fields + optional `image` file                                   |
| `DELETE` | `/api/rooms/{id}` | Delete                       | no            | —                                                                |

**Rooms list query params:**

- `page` (int, default 1), `pageSize` (int, default 10)
- `search` (string) — filters `Name` contains (case-insensitive)
- `locationId` (int?) — exact match filter
- `sortBy` (`name` | `capacity` | `createdAt`, default `createdAt`)
- `sortDir` (`asc` | `desc`, default `asc`)

### Bookings

| Method   | Route                | Description    | Auth required | Body / Params                                       |
| -------- | -------------------- | -------------- | ------------- | --------------------------------------------------- |
| `GET`    | `/api/bookings`      | Paginated list | no            | `?page`, `pageSize`, `roomId`, `fromDate`, `toDate` |
| `GET`    | `/api/bookings/{id}` | Get single     | no            | —                                                   |
| `POST`   | `/api/bookings`      | Create         | no            | JSON body                                           |
| `PUT`    | `/api/bookings/{id}` | Update         | no            | JSON body                                           |
| `DELETE` | `/api/bookings/{id}` | Delete         | no            | —                                                   |

**Bookings list query params:**

- `page` (int, default 1), `pageSize` (int, default 10)
- `roomId` (int?) — filter by room
- `fromDate` (DateTime?) — bookings with `StartTime >= fromDate`
- `toDate` (DateTime?) — bookings with `EndTime <= toDate`

---

## 6. Validation Rules

### Location

- `Name`: required, not empty, max 200 chars

### Room

- `Name`: required, not empty, max 200 chars
- `Capacity`: required, min value 1
- `LocationId`: required, must reference an existing Location
- `Purpose`: optional, max 500 chars
- `Image` (upload): optional; if provided, must be JPEG or PNG, max 5 MB

### Booking

- `RoomId`: required, must reference an existing Room
- `StartTime`: required
- `EndTime`: required; must be strictly after `StartTime`
- `BookedBy`: required, not empty, max 200 chars
- `Purpose`: optional, max 500 chars

---

## 7. Business Rules

1. **No overlapping bookings:** A new or updated booking for a room cannot overlap any existing booking for the same room. Overlap condition: `existingStart < newEnd AND existingEnd > newStart`. Throws `ValidationException("Room is already booked for the requested time slot.")`.
2. **Room deletion guard:** Attempting to delete a Room that has at least one Booking throws `ValidationException("Room cannot be deleted because it has existing bookings.")`.
3. **Location deletion guard:** Attempting to delete a Location that has at least one Room throws `ValidationException("Location cannot be deleted because it has rooms assigned to it.")`.
4. **Image replacement:** On Room update, if a new image is uploaded, the old image file is deleted from disk before the new one is saved.
5. **Image storage path:** `wwwroot/uploads/rooms/<guid>.<ext>` — GUID generated per upload, extension preserved from original filename.

### Acceptance Scenarios

**Scenario: Create booking — no overlap**

- Given: Room 1 is booked 09:00–10:00 on 2026-03-10
- When: POST `/api/bookings` with RoomId=1, Start=10:00, End=11:00
- Then: 201 created, new booking returned

**Scenario: Create booking — overlap**

- Given: Room 1 is booked 09:00–10:00 on 2026-03-10
- When: POST `/api/bookings` with RoomId=1, Start=09:30, End=10:30
- Then: 400 with `"Room is already booked for the requested time slot."`

**Scenario: Delete room with bookings**

- Given: Room 1 has 2 bookings
- When: DELETE `/api/rooms/1`
- Then: 400 with `"Room cannot be deleted because it has existing bookings."`

**Scenario: Delete location with rooms**

- Given: Location "HQ" has 3 rooms
- When: DELETE `/api/locations/1`
- Then: 400 with `"Location cannot be deleted because it has rooms assigned to it."`

**Scenario: Upload oversized image**

- Given: POST `/api/rooms` with a 6 MB PNG
- When: the request is processed
- Then: 400 with image size validation error

**Scenario: Get rooms list with search**

- Given: Rooms named "Board Room", "Board 2", "Cafeteria"
- When: GET `/api/rooms?search=board`
- Then: Returns paginated result with "Board Room" and "Board 2" only

---

## 8. Authorization

- none (all endpoints are public for this implementation)

---

## 9. Frontend UI

### Design reference

none

### Description

#### Rooms Page (`/rooms`)

Two-tab layout:

**Tab 1 — "Rooms"**

- Top bar: search input (debounced 300ms), location filter dropdown ("All Locations" default), "New Room" button (primary), "Manage Locations" button (secondary/outline).
- Data table columns: Thumbnail (40×40 image or placeholder icon), Name (sortable), Capacity (sortable), Location, Purpose (truncated), Actions (Edit | Delete).
- Column header click toggles sort asc/desc.
- Pagination below table.
- Delete confirmation dialog: warns if room has bookings (error returned from API).
- "New Room" / "Edit" opens a form dialog (see below).

**Tab 2 — "Calendar"**

- Room selector dropdown at the top (populated from `/api/rooms/all`).
- Weekly calendar view: 24-row × 7-column HTML table. Week navigation (prev/next/today) in the top bar.
- Bookings fetched for current week (`pageSize: 100`) filtered by selected room.
- Clicking an empty time slot opens Create Booking dialog pre-filled with selected room and clicked hour (1-hour window).
- Clicking an existing booking event opens Booking Detail dialog (showing all fields + Edit / Delete actions).
- Booking events display `bookedBy` as title; click stops propagation to prevent slot-click from firing.

#### Locations Management

- Accessible via "Manage Locations" button on the Rooms tab — opens a Dialog (modal).
- Table with inline rows: Location Name | Edit (opens inline input row) | Delete (trash icon).
- "Add Location" row at the bottom with an input + Save button.
- Delete shows inline error toast if location has rooms.
- Uses `usePostApiLocationsWithJson` and `usePutApiLocationsIdWithJson` (Orval JSON-body hooks).

#### Room Form Dialog (Create / Edit)

Fields:

- Name (text input, required)
- Capacity (number input, required, min 1)
- Location (dropdown, required — populated from locations list)
- Purpose (textarea, optional)
- Image (file input, JPEG/PNG, max 5 MB) — shows current image thumbnail on edit (via `assetUrl()`); clearing removes the image.

Submitted as `multipart/form-data`.

**Implementation note:** Dialog is split into outer `RoomFormDialog` (wraps `<Dialog>`) + inner `RoomFormContent` keyed on `open ? (room?.id ?? "new") : "closed"` — this ensures React state initializes from props without `useEffect`, satisfying React Compiler lint rules.

#### Bookings Page (`/bookings`)

- Top bar: Room filter dropdown ("All Rooms"), Date From / Date To `<input type="date">`, "New Booking" button.
- Data table columns: Room Name, Start Time, End Time, Booked By, Purpose, Actions (Edit | Delete via dropdown).
- Pagination below table.
- Create/Edit dialog: Room dropdown, Start Time datetime-local, End Time datetime-local, Booked By text, Purpose textarea.
- Overlap error displayed inline in the dialog on submit failure (shown below form fields).
- Also reachable from Calendar tab: clicking a booking event opens `BookingDetailDialog`, which has Edit and Delete sub-actions.

**Implementation note:** `BookingFormDialog` uses same outer/inner keyed pattern as `RoomFormDialog`. Key is `open ? (booking?.id ?? defaults?.startTime ?? "new") : "closed"`.

**Booking Detail Dialog (`booking-detail-dialog.tsx`):**

- Opened from calendar tab when clicking a booking event.
- Displays all fields (room, start/end, bookedBy, purpose).
- Has Edit button (opens `BookingFormDialog`) and Delete button (opens `BookingDeleteDialog`).

---

### 10. Redux UI State

#### Rooms slice (`roomsSlice`) — `features/rooms/store/rooms-slice.ts`

- `searchQuery: string`
- `locationIdFilter: number | null`
- `sortBy: 'name' | 'capacity' | 'createdAt'`
- `sortDir: 'asc' | 'desc'`
- `activeTab: 'rooms' | 'calendar'`
- `selectedRoomIdForCalendar: number | null`

#### Bookings slice (`bookingsSlice`) — `features/bookings/store/bookings-slice.ts`

- `roomIdFilter: number | null`
- `fromDate: string | null`
- `toDate: string | null`

Both slices registered in `store/store.ts` as `rooms` and `bookings` keys.

> Server data (paginated rooms, bookings, locations list) lives in React Query — not in Redux.

---

## 11. File Locations

### Backend

| File                           | Path                                                                 |
| ------------------------------ | -------------------------------------------------------------------- |
| Location entity                | `backend/src/Backend.Api/Features/Locations/Location.cs`             |
| Location DTOs                  | `backend/src/Backend.Api/Features/Locations/LocationDtos.cs`         |
| Location validator             | `backend/src/Backend.Api/Features/Locations/LocationsValidator.cs`   |
| Location repository interface  | `backend/src/Backend.Api/Features/Locations/ILocationsRepository.cs` |
| Location repository            | `backend/src/Backend.Api/Features/Locations/LocationsRepository.cs`  |
| Location service interface     | `backend/src/Backend.Api/Features/Locations/ILocationsService.cs`    |
| Location service               | `backend/src/Backend.Api/Features/Locations/LocationsService.cs`     |
| Location controller            | `backend/src/Backend.Api/Features/Locations/LocationsController.cs`  |
| Room entity                    | `backend/src/Backend.Api/Features/Rooms/Room.cs`                     |
| Room DTOs                      | `backend/src/Backend.Api/Features/Rooms/RoomDtos.cs`                 |
| Room validator                 | `backend/src/Backend.Api/Features/Rooms/RoomsValidator.cs`           |
| Room repository interface      | `backend/src/Backend.Api/Features/Rooms/IRoomsRepository.cs`         |
| Room repository                | `backend/src/Backend.Api/Features/Rooms/RoomsRepository.cs`          |
| Room service interface         | `backend/src/Backend.Api/Features/Rooms/IRoomsService.cs`            |
| Room service                   | `backend/src/Backend.Api/Features/Rooms/RoomsService.cs`             |
| Room controller                | `backend/src/Backend.Api/Features/Rooms/RoomsController.cs`          |
| File storage service interface | `backend/src/Backend.Api/Features/Rooms/IFileStorageService.cs`      |
| File storage service           | `backend/src/Backend.Api/Features/Rooms/FileStorageService.cs`       |
| Booking entity                 | `backend/src/Backend.Api/Features/Bookings/Booking.cs`               |
| Booking DTOs                   | `backend/src/Backend.Api/Features/Bookings/BookingDtos.cs`           |
| Booking validator              | `backend/src/Backend.Api/Features/Bookings/BookingsValidator.cs`     |
| Booking repository interface   | `backend/src/Backend.Api/Features/Bookings/IBookingsRepository.cs`   |
| Booking repository             | `backend/src/Backend.Api/Features/Bookings/BookingsRepository.cs`    |
| Booking service interface      | `backend/src/Backend.Api/Features/Bookings/IBookingsService.cs`      |
| Booking service                | `backend/src/Backend.Api/Features/Bookings/BookingsService.cs`       |
| Booking controller             | `backend/src/Backend.Api/Features/Bookings/BookingsController.cs`    |

### Frontend

| File                      | Path                                                                  |
| ------------------------- | --------------------------------------------------------------------- |
| Rooms page                | `frontend/src/features/rooms/components/rooms-page.tsx`               |
| Rooms tab                 | `frontend/src/features/rooms/components/rooms-tab.tsx`                |
| Calendar tab              | `frontend/src/features/rooms/components/calendar-tab.tsx`             |
| Room table                | `frontend/src/features/rooms/components/room-table.tsx`               |
| Room form dialog          | `frontend/src/features/rooms/components/room-form-dialog.tsx`         |
| Room delete dialog        | `frontend/src/features/rooms/components/room-delete-dialog.tsx`       |
| Locations panel           | `frontend/src/features/rooms/components/locations-panel.tsx`          |
| Rooms Redux slice         | `frontend/src/features/rooms/store/rooms-slice.ts`                    |
| Rooms route               | `frontend/src/routes/rooms/index.tsx`                                 |
| Bookings page             | `frontend/src/features/bookings/components/bookings-page.tsx`         |
| Booking table             | `frontend/src/features/bookings/components/booking-table.tsx`         |
| Booking form dialog       | `frontend/src/features/bookings/components/booking-form-dialog.tsx`   |
| Booking delete dialog     | `frontend/src/features/bookings/components/booking-delete-dialog.tsx` |
| Booking detail dialog     | `frontend/src/features/bookings/components/booking-detail-dialog.tsx` |
| Bookings Redux slice      | `frontend/src/features/bookings/store/bookings-slice.ts`              |
| Bookings route            | `frontend/src/routes/bookings/index.tsx`                              |
| Generated API (rooms)     | `frontend/src/api/generated/rooms/`                                   |
| Generated API (locations) | `frontend/src/api/generated/locations/`                               |
| Generated API (bookings)  | `frontend/src/api/generated/bookings/`                                |
| Asset URL helper          | `frontend/src/lib/utils.ts` → `assetUrl()`                            |

---

## 12. Tests

**Tests written:** no

### Backend Unit Tests

| Test                                                                   | Description                                 |
| ---------------------------------------------------------------------- | ------------------------------------------- |
| `BookingsService_CreateAsync_NoOverlap_ReturnsDto`                     | Happy path: non-overlapping booking created |
| `BookingsService_CreateAsync_OverlapExists_ThrowsValidationException`  | Overlap detected → 400                      |
| `BookingsService_CreateAsync_EndBeforeStart_ThrowsValidationException` | Invalid time range → 400                    |
| `RoomsService_DeleteAsync_HasBookings_ThrowsValidationException`       | Room with bookings cannot be deleted        |
| `LocationsService_DeleteAsync_HasRooms_ThrowsValidationException`      | Location with rooms cannot be deleted       |
| `RoomsService_GetAllAsync_ReturnsPaginatedResult`                      | Correct page, total, and order              |
| `RoomsService_GetByIdAsync_InvalidId_ThrowsNotFoundException`          | 404 for missing room                        |

### Frontend Tests

| Test                                              | Description                            |
| ------------------------------------------------- | -------------------------------------- |
| `RoomsPage renders tabs`                          | Both tabs render; switching works      |
| `RoomTable renders rows from mocked query`        | Table columns match spec               |
| `RoomFormDialog submits multipart create request` | Correct FormData sent                  |
| `BookingsPage renders table with filters`         | Room dropdown and date filters present |
| `BookingFormDialog shows overlap error on 400`    | Inline error displayed                 |

---

## Migration Name

`AddMeetingRoomBookingEntities`

---

## Implementation Notes & Gotchas

### Orval-generated hook names

- Room multipart POST/PUT: `usePostApiRooms` / `usePutApiRoomsId` (no `WithFormData` suffix)
- Location / Booking JSON POST/PUT: `usePostApiLocationsWithJson` / `usePutApiLocationsIdWithJson`, `usePostApiBookingsWithJson` / `usePutApiBookingsIdWithJson`

### Dialog state reset (React Compiler compliance)

- Both `RoomFormDialog` and `BookingFormDialog` use an outer shell (provides `<Dialog>`) + inner content component.
- Inner component is keyed on `open ? (entity?.id ?? "new") : "closed"` so state initializes from props directly — no `useEffect` with multiple setState calls needed.

### `SortIcon` component placement

- `SortIcon` is defined **outside** `RoomTable` (above the component function) to satisfy the React Compiler `react-hooks/static-components` lint rule.

### `assetUrl()` helper

- Defined in `lib/utils.ts`. Prepends `VITE_API_URL ?? "http://localhost:5054"` to relative image paths from the backend.
- Used in `room-table.tsx` (thumbnail) and `room-form-dialog.tsx` (edit preview).

### CORS

- `appsettings.json` `AllowedOrigins` must include the frontend dev port (default `5173`, may also be `5174` if occupied).

---

## Checklist

### Backend

- [x] Location entity + DTOs + validator + repository + service + controller created
- [x] Room entity + DTOs + validator + repository + service + controller created
- [x] `IFileStorageService` + `FileStorageService` created; static files middleware configured
- [x] Booking entity + DTOs + validator + repository + service + controller created
- [x] EF Core `OnModelCreating` — Restrict delete configured for Location→Room and Room→Booking
- [x] Overlap check implemented in `BookingsService`
- [x] Deletion guards implemented in `RoomsService` and `LocationsService`
- [x] All three feature sets registered in `Program.cs`
- [x] `wwwroot/uploads/rooms/` directory created at startup in `Program.cs`
- [x] Migration `AddMeetingRoomBookingEntities` created and applied

### API Sync

- [x] `npm run api:sync` run from repo root
- [x] `api/generated/rooms/`, `api/generated/locations/`, `api/generated/bookings/` folders generated

### Frontend

- [x] `features/rooms/` folder created with all layers
- [x] `features/bookings/` folder created with all layers
- [x] Rooms Redux slice registered in `store/store.ts`
- [x] Bookings Redux slice registered in `store/store.ts`
- [x] `/rooms` route added
- [x] `/bookings` route added
- [x] Nav links added in `components/layout/app-layout.tsx`
- [x] Translations added to BOTH `en.json` AND `fi.json`
- [x] Calendar built as plain 24×7 HTML table — no external library needed
- [x] `assetUrl()` helper added to `lib/utils.ts` for image URL resolution
