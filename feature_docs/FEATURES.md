# Feature Index

> **AI Workflow:** Read this file first on every request. Identify the relevant feature, then read only its spec file. Do not read all spec files. When a new feature spec is created, add a row to the table below. There is no separate `architectures/` folder — specs in `feature_docs/` are the single source of truth.

> **Spec structure:** Each spec file is a single combined document covering both behavior (entity fields, endpoints, validation, UI) and architecture (Core Values & Principles, Architecture Decisions, Data Flow).

| Feature | Spec File | Summary | Status |
|---|---|---|---|
| **Feature (Example)** | `feature-spec-feature-example.md` | Template/example entity with a single `Name` field. Used as the canonical reference for generating new features. | Example only |
| **Todos** | `feature-spec-todos.md` | CRUD for todo items. Fields: `Title`, `Description`, `IsCompleted`, `DueDate`, `Priority`. Supports toggle-complete via `PATCH /toggle`. | Active |
| **Factories** | `feature-spec-factories.md` | CRUD for production facilities. Fields: `Name` (unique), `TimeZone`. Soft-delete: existing reservations retain `FactoryDisplayName` snapshot. Unpaged list via `GET /api/factories/all`. | Active |
| **Personnel** | `feature-spec-personnel.md` | CRUD for personnel with many-to-many relationship to Factories. Fields: `PersonalId` (unique), `FullName`, `Email` (unique). Soft-delete: associated `ReservationPerson` records have `PersonId` nulled (display name retained). | Active |
| **Reservations** | `feature-spec-reservations.md` | Scheduling reservations linking personnel to factories. Validates overlap per person, factory membership. Stores display name snapshots at creation time. Filters: factory, person, date range. | Active |
| **Scheduling Overview** | `feature-spec-scheduling.md` | Read-only summary views. `GET /api/scheduling/by-person` (reservations per person with total hours). `GET /api/scheduling/by-factory` (total hours + reservation count per factory). Frontend: two-tab view. | Active |
