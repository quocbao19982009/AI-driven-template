# Feature Index

> **AI Workflow:** Read this file first on every request. Identify the relevant feature, then read only its spec file. Do not read all spec files. When a new feature spec is created, add a row to the table below. There is no separate `architectures/` folder — specs in `feature_docs/` are the single source of truth.

> **Spec structure:** Each spec file is a single combined document covering both behavior (entity fields, endpoints, validation, UI) and architecture (Core Values & Principles, Architecture Decisions, Data Flow).

| Feature | Spec File | Data Model | Summary | Status |
|---|---|---|---|---|
| **Feature (Example)** | `feature-spec-feature-example.md` | — | Template/example entity with a single `Name` field. Used as the canonical reference for generating new features. | Example only |
| **Meeting Room Booking** | `meeting-room-booking/feature-spec-meeting-room-booking.md` | Location, Room, Booking | CRUD for locations (unpaged), rooms (paginated, image upload), and bookings (paginated, overlap check). Rooms page with table + calendar tab. Bookings page with filters. | Spec ready |
