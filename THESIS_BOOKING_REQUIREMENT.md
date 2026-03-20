Implement a Meeting Room Booking feature for a full-stack web application.

Backend (.NET 10 / C# Web API — Entity Framework Core / PostgreSQL):

- Three entities:
  - Location (Name) — used as a reference table for rooms.
  - Room (Name, Capacity, LocationId FK to Location, Purpose, ImagePath).
  - Booking (RoomId, StartTime, EndTime, BookedBy, Purpose).
- CRUD endpoints for Locations under /api/locations (unpaged list — no pagination needed).
- CRUD endpoints for Rooms under /api/rooms:
  - GET /api/rooms — paginated, supports search by name (contains), filter by locationId, sortBy (name/capacity/createdAt), sortDir (asc/desc).
  - GET /api/rooms/all — unpaged list for dropdowns.
  - POST/PUT accept multipart/form-data to support optional image upload (JPEG/PNG, max 5 MB).
  - Image stored in wwwroot/uploads/rooms/ with a GUID filename; old image deleted on update.
  - Serve static files so the frontend can display images.
- CRUD endpoints for Bookings under /api/bookings. List supports optional filters: roomId, fromDate, toDate. Paginated.
- Authorization: all endpoints for Locations, Rooms, and Bookings are public — no authentication required.
- Business rules:
  - StartTime must be before EndTime.
  - No two bookings for the same room may overlap.
  - A room cannot be deleted if it has existing bookings.
  - A location cannot be deleted if any rooms reference it.
    Frontend (React / TypeScript — your choice of libraries):
- A Rooms page at /rooms with two tabs:
  - Tab 1 "Rooms": search input (debounced), location filter dropdown, sortable columns, data table (thumbnail, Name, Capacity, Location, Purpose, Actions), pagination, "New Room" button, "Manage Locations" button.
  - Tab 2 "Calendar": room selector dropdown, calendar view of bookings; clicking an empty slot opens the Create Booking dialog pre-filled with that room and time; clicking an event shows booking detail.
- Locations management accessible from the Rooms page: table with inline add/edit/delete; deletion shows an error if rooms reference the location.
- Room create/edit dialog: Name, Capacity, Location dropdown, Purpose, optional image upload with preview.
- A Bookings page at /bookings with a data table, room filter dropdown, date range filters, and create/edit/delete dialogs.
- Fetch data from the API. Manage filter state in the component or however you see fit.
- Use React and TypeScript. Choose your own libraries for data fetching, state management, UI components, and styling. The goal is to see what you produce without prescribed conventions.
