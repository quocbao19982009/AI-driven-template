# Feature Specification: Room Bookings

**Last Updated:** `2026-03-20`
**Status:** Scaffolded
**Tests written:** no

---

## 1. Entities

This feature introduces three entities: **Location**, **Room**, and **Booking**.

---

### Entity: Location

**Name:** `Location`
**Table name (plural):** `Locations`

#### Fields

| Property | C# Type  | Required | Constraints    | Notes |
|----------|----------|----------|----------------|-------|
| `Name`   | `string` | yes      | max 200 chars  |       |

> `Id` (int), `CreatedAt`, `UpdatedAt` are inherited from `BaseEntity` — do not add them.

#### Relationships

- has many `Room`s (one-to-many; a Location cannot be deleted if any Rooms reference it)

---

### Entity: Room

**Name:** `Room`
**Table name (plural):** `Rooms`

#### Fields

| Property     | C# Type  | Required | Constraints                        | Notes                                             |
|--------------|----------|----------|------------------------------------|---------------------------------------------------|
| `Name`       | `string` | yes      | max 200 chars                      |                                                   |
| `Capacity`   | `int`    | yes      | > 0                                |                                                   |
| `LocationId` | `int`    | yes      | FK → Location                      |                                                   |
| `Purpose`    | `string` | no       | max 500 chars                      |                                                   |
| `ImagePath`  | `string` | no       | relative path under uploads/rooms/ | GUID filename; nullable; set by service on upload |

> `Id` (int), `CreatedAt`, `UpdatedAt` are inherited from `BaseEntity` — do not add them.

#### Relationships

- belongs to `Location` (LocationId FK, required)
- has many `Booking`s (one-to-many; a Room cannot be deleted if it has existing Bookings)

---

### Entity: Booking

**Name:** `Booking`
**Table name (plural):** `Bookings`

#### Fields

| Property    | C# Type        | Required | Constraints                  | Notes                               |
|-------------|----------------|----------|------------------------------|-------------------------------------|
| `RoomId`    | `int`          | yes      | FK → Room                    |                                     |
| `StartTime` | `DateTime`     | yes      | must be before EndTime       | stored as UTC                       |
| `EndTime`   | `DateTime`     | yes      | must be after StartTime      | stored as UTC                       |
| `BookedBy`  | `string`       | yes      | max 200 chars                | name or identifier of the requester |
| `Purpose`   | `string`       | no       | max 500 chars                |                                     |

> `Id` (int), `CreatedAt`, `UpdatedAt` are inherited from `BaseEntity` — do not add them.

#### Relationships

- belongs to `Room` (RoomId FK, required)

---

## 2. Core Values & Principles

- A room belongs to exactly one location; a location is a reference table that groups rooms.
- Room images are optional but managed server-side: uploaded files are stored in `wwwroot/uploads/rooms/` with GUID filenames; the old image is deleted when a new one is uploaded.
- Bookings are time-bounded reservations; overlapping bookings for the same room are never allowed.
- Cascade delete is blocked at the service layer: rooms cannot be deleted while bookings exist; locations cannot be deleted while rooms exist.
- All endpoints are public — no authentication is required for any operation.

---

## 3. Architecture Decisions

- Standard layered pattern: Controller → Service → Repository → EF Core
- Three independent feature folders under `Features/`: `Locations/`, `Rooms/`, `Bookings/`
- Room POST/PUT use `multipart/form-data` (IFormFile) instead of JSON for image upload support
- Static files served from `wwwroot/uploads/rooms/` via `app.UseStaticFiles()`
- Image storage handled in the service layer (not repository); GUID filename generated on upload; old file deleted on update
- Overlap check runs in the service layer using a repository query before saving
- Cascade-delete protection checked in the service layer before calling repository delete

---

## 4. Data Flow

### Location — Create
1. Client POST `/api/locations` with `{ "name": "..." }`
2. Controller calls `LocationsService.CreateAsync`
3. Service validates, maps to entity, calls `LocationsRepository.CreateAsync`
4. Returns `ApiResponse<LocationDto>` (201)

### Location — Delete
1. Client DELETE `/api/locations/{id}`
2. Service checks `RoomsRepository.ExistsForLocationAsync(id)` — throws `ValidationException` if any rooms reference it
3. Repository deletes; returns 204

### Room — Create
1. Client POST `/api/rooms` as multipart/form-data (fields + optional IFormFile image)
2. Controller binds `[FromForm] CreateRoomRequest`
3. Service validates; if image present, validates MIME (JPEG/PNG) and size (≤ 5 MB), saves file to `wwwroot/uploads/rooms/{guid}.{ext}`, sets `ImagePath`
4. Repository saves entity; returns `ApiResponse<RoomDto>` (201)

### Room — Update
1. Client PUT `/api/rooms/{id}` as multipart/form-data
2. Service loads existing room; if new image provided, deletes old file (if any), saves new file; updates fields
3. Repository saves; returns `ApiResponse<RoomDto>`

### Room — Delete
1. Service checks `BookingsRepository.ExistsForRoomAsync(id)` — throws `ValidationException` if bookings exist
2. If room has ImagePath, deletes the physical file
3. Repository deletes; returns 204

### Room — List (paginated)
1. Client GET `/api/rooms?page=1&pageSize=10&search=&locationId=&sortBy=name&sortDir=asc`
2. Repository applies `WHERE Name CONTAINS search`, `WHERE LocationId = locationId`, `ORDER BY {sortBy} {sortDir}`, `Skip/Take`
3. Returns `PagedResult<Room>`; Service maps to `PagedResult<RoomDto>`

### Booking — Create
1. Client POST `/api/bookings` with JSON body
2. Service validates StartTime < EndTime
3. Service calls `BookingsRepository.HasOverlapAsync(roomId, startTime, endTime, excludeId: null)` — throws `ValidationException` if overlap exists
4. Repository saves; returns `ApiResponse<BookingDto>` (201)

### Booking — Update
1. Service validates StartTime < EndTime
2. Overlap check excludes the booking being updated (`excludeId = id`)
3. Repository saves; returns `ApiResponse<BookingDto>`

### Booking — List (paginated)
1. Client GET `/api/bookings?page=1&pageSize=10&roomId=&fromDate=&toDate=`
2. Repository applies optional filters, `ORDER BY StartTime ASC`, `Skip/Take`
3. Returns `PagedResult<BookingDto>`

---

## 5. API Endpoints

### Locations

| Method   | Route                   | Description              | Auth required | Body / Notes              |
|----------|-------------------------|--------------------------|---------------|---------------------------|
| `GET`    | `/api/locations`        | Full (unpaged) list      | no            |                           |
| `GET`    | `/api/locations/{id}`   | Get single location      | no            |                           |
| `POST`   | `/api/locations`        | Create location          | no            | JSON                      |
| `PUT`    | `/api/locations/{id}`   | Update location          | no            | JSON                      |
| `DELETE` | `/api/locations/{id}`   | Delete location          | no            | 409 if rooms exist        |

### Rooms

| Method   | Route               | Description                              | Auth required | Body / Notes                                    |
|----------|---------------------|------------------------------------------|---------------|-------------------------------------------------|
| `GET`    | `/api/rooms`        | Paginated list with search/filter/sort   | no            | `?page&pageSize&search&locationId&sortBy&sortDir` |
| `GET`    | `/api/rooms/all`    | Unpaged list for dropdowns               | no            |                                                 |
| `GET`    | `/api/rooms/{id}`   | Get single room                          | no            |                                                 |
| `POST`   | `/api/rooms`        | Create room (with optional image)        | no            | `multipart/form-data`                           |
| `PUT`    | `/api/rooms/{id}`   | Update room (with optional image swap)   | no            | `multipart/form-data`                           |
| `DELETE` | `/api/rooms/{id}`   | Delete room                              | no            | 409 if bookings exist                           |

### Bookings

| Method   | Route                  | Description                    | Auth required | Body / Notes                        |
|----------|------------------------|--------------------------------|---------------|-------------------------------------|
| `GET`    | `/api/bookings`        | Paginated list with filters    | no            | `?page&pageSize&roomId&fromDate&toDate` |
| `GET`    | `/api/bookings/{id}`   | Get single booking             | no            |                                     |
| `POST`   | `/api/bookings`        | Create booking                 | no            | JSON                                |
| `PUT`    | `/api/bookings/{id}`   | Update booking                 | no            | JSON                                |
| `DELETE` | `/api/bookings/{id}`   | Delete booking                 | no            | 204                                 |

---

## 6. Validation Rules

### Location
- `Name`: required, not empty, max 200 characters

### Room
- `Name`: required, not empty, max 200 characters
- `Capacity`: required, must be > 0
- `LocationId`: required, must reference an existing Location
- `Purpose`: optional, max 500 characters
- `Image` (upload): optional; if provided, MIME type must be `image/jpeg` or `image/png`, size ≤ 5 MB

### Booking
- `RoomId`: required, must reference an existing Room
- `StartTime`: required
- `EndTime`: required
- `StartTime` must be strictly before `EndTime` (service-layer rule)
- `BookedBy`: required, not empty, max 200 characters
- `Purpose`: optional, max 500 characters

---

## 7. Business Rules

1. **No overlapping bookings**: For a given Room, no two bookings may overlap. A new booking `[S, E)` overlaps an existing `[S2, E2)` if `S < E2 && E > S2`. This check excludes the booking being updated.
2. **Room delete blocked**: A Room cannot be deleted if one or more Bookings reference it — return a `ValidationException` (mapped to 409 or 400).
3. **Location delete blocked**: A Location cannot be deleted if one or more Rooms reference it — return a `ValidationException`.
4. **Image management**: When a Room is updated and a new image is uploaded, the old file (if any) is deleted from disk before the new one is saved. When a Room is deleted, its image file is also deleted.
5. **Static file serving**: `wwwroot/uploads/rooms/` is served as static files so the frontend can render image thumbnails via the path stored in `ImagePath`.

### Acceptance Scenarios

**Scenario: Create location with valid name**
- Given: POST `/api/locations` with `{ "name": "Building A" }`
- When: the request is processed
- Then: returns 201 with the created LocationDto wrapped in ApiResponse

**Scenario: Create location with empty name**
- Given: POST `/api/locations` with `{ "name": "" }`
- When: the request is processed
- Then: returns 400 with a validation error identifying the Name field

**Scenario: Delete location that has rooms**
- Given: a Location with id=1 has at least one Room referencing it
- When: DELETE `/api/locations/1`
- Then: returns 400 (ValidationException) with message indicating rooms still exist

**Scenario: Delete location with no rooms**
- Given: a Location with id=2 has no rooms
- When: DELETE `/api/locations/2`
- Then: returns 204

**Scenario: Create room with valid data and no image**
- Given: POST `/api/rooms` multipart/form-data with Name, Capacity=10, LocationId=1, no file
- When: the request is processed
- Then: returns 201 with RoomDto (ImagePath is null)

**Scenario: Create room with valid image**
- Given: POST `/api/rooms` multipart with a valid JPEG ≤ 5 MB
- When: the request is processed
- Then: returns 201 with RoomDto where ImagePath is a non-empty relative path under uploads/rooms/

**Scenario: Create room with oversized image**
- Given: POST `/api/rooms` with an image file > 5 MB
- When: the request is processed
- Then: returns 400 with a validation error about image size

**Scenario: Create room with invalid image type**
- Given: POST `/api/rooms` with a file whose MIME type is not image/jpeg or image/png
- When: the request is processed
- Then: returns 400 with a validation error about image type

**Scenario: Delete room with existing bookings**
- Given: a Room with id=1 has at least one Booking
- When: DELETE `/api/rooms/1`
- Then: returns 400 (ValidationException) with message indicating bookings still exist

**Scenario: Delete room with no bookings**
- Given: a Room with id=2 has no bookings
- When: DELETE `/api/rooms/2`
- Then: returns 204; its image file (if any) is deleted from disk

**Scenario: Get paginated rooms with search**
- Given: GET `/api/rooms?search=conf&page=1&pageSize=10`
- When: the request is processed
- Then: returns 200 with a PagedResult containing only rooms whose Name contains "conf" (case-insensitive)

**Scenario: Create booking with valid non-overlapping times**
- Given: POST `/api/bookings` with RoomId=1, StartTime=10:00, EndTime=11:00 and no existing booking in that range for Room 1
- When: the request is processed
- Then: returns 201 with BookingDto

**Scenario: Create booking with StartTime >= EndTime**
- Given: POST `/api/bookings` with StartTime=11:00, EndTime=10:00
- When: the request is processed
- Then: returns 400 with a validation error: StartTime must be before EndTime

**Scenario: Create booking that overlaps an existing booking**
- Given: Room 1 already has a booking from 10:00–11:00; POST `/api/bookings` with Room 1, 10:30–11:30
- When: the request is processed
- Then: returns 400 (ValidationException) with message about overlapping booking

**Scenario: Update booking without creating overlap**
- Given: Booking id=5 for Room 1 from 10:00–11:00; PUT with StartTime=10:00, EndTime=12:00 (no other bookings in that window)
- When: the request is processed
- Then: returns 200 with updated BookingDto (overlap check excludes booking id=5)

**Scenario: Get booking by non-existent ID**
- Given: GET `/api/bookings/9999` where no booking with that ID exists
- When: the request is processed
- Then: returns 404 with NotFoundException message

---

## 8. Authorization

- **All endpoints are public** — no authentication or role check is required for any Location, Room, or Booking operation.
- Every controller uses `[AllowAnonymous]` at the class level.

---

## 9. Frontend UI

### Design reference

No Figma design provided.

### Description

#### Rooms Page (`/rooms`) — two tabs

**Tab 1: "Rooms"**
- Header row: page title "Rooms" on the left; "Manage Locations" button and "New Room" button on the right.
- Search bar (debounced, 300 ms) and Location filter dropdown above the table.
- Data table columns: thumbnail image (small, or placeholder icon if none), Name, Capacity, Location, Purpose, Actions.
- Actions column: Edit button / Delete button per row.
- Sortable columns: Name, Capacity (click header to toggle asc/desc).
- Pagination controls below the table.
- "New Room" opens a create dialog; clicking Edit opens an edit dialog.
- Delete shows a confirmation dialog (warns if operation fails due to existing bookings).

**Room Create/Edit Dialog**
- Fields: Name (text), Capacity (number), Location (dropdown populated from `/api/locations`), Purpose (textarea, optional), Image (file input with preview of current image; optional).
- On submit: sends multipart/form-data. On success, invalidates the rooms query.

**Tab 2: "Calendar"**
- Room selector dropdown at the top (populated from `/api/rooms/all`).
- Calendar view (e.g., FullCalendar or similar) showing bookings for the selected room.
- Clicking an empty time slot opens Create Booking dialog pre-filled with the selected room and clicked time range.
- Clicking an existing booking event shows a Booking Detail dialog with edit/delete actions.

#### Locations Management (modal or slide-over, accessible via "Manage Locations" button)
- Inline table: columns Name, Actions (Edit inline / Delete).
- "Add Location" row at the bottom or a button that appends an inline editable row.
- Deletion that fails (rooms exist) shows an error message inline.

#### Bookings Page (`/bookings`)
- Header: "Bookings" title; "New Booking" button.
- Filters: Room dropdown, date-range pickers (From / To).
- Data table columns: Room name, Start Time, End Time, Booked By, Purpose, Actions (Edit / Delete).
- Pagination.
- Create/Edit dialog: Room dropdown, Start Time picker, End Time picker, Booked By (text), Purpose (textarea, optional).
- Delete confirmation dialog.

---

## 10. Redux UI State

### Rooms slice
- `searchQuery: string`
- `locationFilter: number | null`
- `sortBy: 'name' | 'capacity' | 'createdAt'`
- `sortDir: 'asc' | 'desc'`
- `activeTab: 'rooms' | 'calendar'`
- `selectedRoomId: number | null` (for calendar tab room selector)

### Bookings slice
- `roomFilter: number | null`
- `fromDate: string | null`
- `toDate: string | null`

---

## 11. File Locations

### Backend

#### Locations Feature

| File                 | Path                                                                          |
|----------------------|-------------------------------------------------------------------------------|
| Entity               | `backend/src/Backend.Api/Features/Locations/Location.cs`                      |
| DTOs                 | `backend/src/Backend.Api/Features/Locations/LocationDtos.cs`                  |
| Validator            | `backend/src/Backend.Api/Features/Locations/LocationsValidator.cs`            |
| Repository interface | `backend/src/Backend.Api/Features/Locations/ILocationsRepository.cs`          |
| Repository           | `backend/src/Backend.Api/Features/Locations/LocationsRepository.cs`           |
| Service interface    | `backend/src/Backend.Api/Features/Locations/ILocationsService.cs`             |
| Service              | `backend/src/Backend.Api/Features/Locations/LocationsService.cs`              |
| Controller           | `backend/src/Backend.Api/Features/Locations/LocationsController.cs`           |

#### Rooms Feature

| File                 | Path                                                                          |
|----------------------|-------------------------------------------------------------------------------|
| Entity               | `backend/src/Backend.Api/Features/Rooms/Room.cs`                              |
| DTOs                 | `backend/src/Backend.Api/Features/Rooms/RoomDtos.cs`                          |
| Validator            | `backend/src/Backend.Api/Features/Rooms/RoomsValidator.cs`                    |
| Repository interface | `backend/src/Backend.Api/Features/Rooms/IRoomsRepository.cs`                  |
| Repository           | `backend/src/Backend.Api/Features/Rooms/RoomsRepository.cs`                   |
| Service interface    | `backend/src/Backend.Api/Features/Rooms/IRoomsService.cs`                     |
| Service              | `backend/src/Backend.Api/Features/Rooms/RoomsService.cs`                      |
| Controller           | `backend/src/Backend.Api/Features/Rooms/RoomsController.cs`                   |

#### Bookings Feature

| File                 | Path                                                                          |
|----------------------|-------------------------------------------------------------------------------|
| Entity               | `backend/src/Backend.Api/Features/Bookings/Booking.cs`                        |
| DTOs                 | `backend/src/Backend.Api/Features/Bookings/BookingDtos.cs`                    |
| Validator            | `backend/src/Backend.Api/Features/Bookings/BookingsValidator.cs`              |
| Repository interface | `backend/src/Backend.Api/Features/Bookings/IBookingsRepository.cs`            |
| Repository           | `backend/src/Backend.Api/Features/Bookings/BookingsRepository.cs`             |
| Service interface    | `backend/src/Backend.Api/Features/Bookings/IBookingsService.cs`               |
| Service              | `backend/src/Backend.Api/Features/Bookings/BookingsService.cs`                |
| Controller           | `backend/src/Backend.Api/Features/Bookings/BookingsController.cs`             |

### Frontend

#### Rooms

| File                       | Path                                                                                   |
|----------------------------|----------------------------------------------------------------------------------------|
| Page component             | `frontend/src/features/rooms/components/rooms-page.tsx`                                |
| Rooms table                | `frontend/src/features/rooms/components/rooms-table.tsx`                               |
| Room form dialog           | `frontend/src/features/rooms/components/room-form-dialog.tsx`                          |
| Room delete dialog         | `frontend/src/features/rooms/components/room-delete-dialog.tsx`                        |
| Calendar tab               | `frontend/src/features/rooms/components/rooms-calendar.tsx`                            |
| Locations manager          | `frontend/src/features/rooms/components/locations-manager.tsx`                         |
| Redux slice                | `frontend/src/features/rooms/store/rooms-slice.ts`                                     |
| Route                      | `frontend/src/routes/rooms/index.tsx`                                                  |

#### Bookings

| File                       | Path                                                                                   |
|----------------------------|----------------------------------------------------------------------------------------|
| Page component             | `frontend/src/features/bookings/components/bookings-page.tsx`                          |
| Bookings table             | `frontend/src/features/bookings/components/bookings-table.tsx`                         |
| Booking form dialog        | `frontend/src/features/bookings/components/booking-form-dialog.tsx`                    |
| Booking delete dialog      | `frontend/src/features/bookings/components/booking-delete-dialog.tsx`                  |
| Redux slice                | `frontend/src/features/bookings/store/bookings-slice.ts`                               |
| Route                      | `frontend/src/routes/bookings/index.tsx`                                               |

---

## 12. Tests

**Tests written:** no

### Backend Unit Tests

#### Locations
| Test | Description |
|------|-------------|
| `CreateAsync_WithValidName_ReturnsLocationDto` | Happy path |
| `CreateAsync_WithEmptyName_ThrowsValidationException` | Validation: empty name |
| `DeleteAsync_WithRoomsExisting_ThrowsValidationException` | Business rule: cascade block |
| `DeleteAsync_WithNoRooms_Succeeds` | Happy path delete |

#### Rooms
| Test | Description |
|------|-------------|
| `CreateAsync_WithValidData_ReturnsRoomDto` | Happy path |
| `CreateAsync_WithOversizedImage_ThrowsValidationException` | Image size rule |
| `CreateAsync_WithInvalidImageType_ThrowsValidationException` | Image MIME rule |
| `DeleteAsync_WithBookingsExisting_ThrowsValidationException` | Business rule: cascade block |
| `GetAllAsync_WithSearchFilter_ReturnsFilteredResult` | Paginated search |

#### Bookings
| Test | Description |
|------|-------------|
| `CreateAsync_WithValidData_ReturnsBookingDto` | Happy path |
| `CreateAsync_WithStartAfterEnd_ThrowsValidationException` | Time order rule |
| `CreateAsync_WithOverlappingBooking_ThrowsValidationException` | Overlap rule |
| `UpdateAsync_DoesNotFlagSelfAsOverlap` | Overlap check excludes self |
| `GetByIdAsync_WithInvalidId_ThrowsNotFoundException` | 404 for missing booking |

### Frontend Tests

| Test | Description |
|------|-------------|
| `RoomsPage renders table with room rows` | Integration: table populated from mocked query |
| `Room form dialog submits multipart/form-data create request` | Correct payload sent |
| `BookingsPage renders table with booking rows` | Integration: table populated |
| `Booking form dialog rejects StartTime >= EndTime` | Client-side validation |

---

## Migration Name

`AddRoomBookingEntities`

> Creates tables: `Locations`, `Rooms` (with FK to Locations), `Bookings` (with FK to Rooms).

---

> **Marker convention:** Both marker types must be resolved before running `/scaffold-feature`:
> - `<!-- TODO: [what is missing] -->` — AI flags missing info that may have a reasonable default
> - `[NEEDS CLARIFICATION: [question] ]` — requires a specific human answer before scaffolding

## Checklist

### Backend
- [x] Location entity + CRUD scaffolded
- [x] Room entity + CRUD scaffolded (multipart/form-data, image handling)
- [x] Booking entity + CRUD scaffolded (overlap check, cascade block)
- [x] Static files configured in Program.cs for wwwroot/uploads/rooms/
- [x] All three features registered in Program.cs
- [x] Migration `AddRoomBookingEntities` created (apply with `dotnet ef database update`)

### API Sync
- [x] `npm run api:sync` run from repo root
- [x] `api/generated/locations/`, `api/generated/rooms/`, `api/generated/bookings/` generated

### Frontend
- [x] Rooms page with two tabs (Rooms table + Calendar)
- [x] Locations manager component
- [x] Bookings page
- [x] Redux slices registered in `store/store.ts`
- [x] Routes added for `/rooms` and `/bookings`
- [x] Navigation links added
