# Feature Index

> **AI Workflow:** Read this file first on every request. Identify the relevant feature, then read only its spec file. Do not read all spec files. When a new feature spec is created, add a row to the table below. There is no separate `architectures/` folder — specs in `feature_docs/` are the single source of truth.

> **Spec structure:** Each spec file is a single combined document covering both behavior (entity fields, endpoints, validation, UI) and architecture (Core Values & Principles, Architecture Decisions, Data Flow).

| Feature | Spec File | Data Model | Summary | Status |
|---|---|---|---|---|
| **Feature (Example)** | `feature-spec-feature-example.md` | — | Template/example entity with a single `Name` field. Used as the canonical reference for generating new features. | Example only |
| **Todos** | `todos/feature-spec-todos.md` | — | CRUD for todo items. Fields: `Title`, `Description`, `IsCompleted`, `DueDate`, `Priority`. Supports toggle-complete via `PATCH /toggle`. | Active |
| **Factories** | `factories/feature-spec-factories.md` | — | CRUD for production facilities. Fields: `Name` (unique), `TimeZone`. Soft-delete: existing reservations retain `FactoryDisplayName` snapshot. Unpaged list via `GET /api/factories/all`. | Active |
| **Personnel** | `personnel/feature-spec-personnel.md` | `personnel/feature-data-model-personnel.md` | CRUD for personnel with many-to-many relationship to Factories. Fields: `PersonalId` (unique), `FullName`, `Email` (unique). Soft-delete: associated `ReservationPerson` records have `PersonId` nulled (display name retained). | Active |
| **Reservations** | `reservations/feature-spec-reservations.md` | `reservations/feature-data-model-reservations.md` | Scheduling reservations linking personnel to factories. Validates overlap per person, factory membership. Stores display name snapshots at creation time. Filters: factory, person, date range. | Active |
| **Scheduling Overview** | `scheduling/feature-spec-scheduling.md` | — | Read-only summary views. `GET /api/scheduling/by-person` (reservations per person with total hours). `GET /api/scheduling/by-factory` (total hours + reservation count per factory). Frontend: two-tab view. | Active |
| **Flashcards** | `flashcards/feature-spec-flashcards.md` | — | Finnish vocabulary flashcard system. CRUD + flip-card study mode. `FlashcardCategory` entity with full CRUD. `Flashcard` has nullable FK to category (SetNull on delete). Max 10 cards per category. PATCH review endpoint. Two-tab UI (Manage/Study) with inline category manager. No auth. | Active |
| **Authentication & Authorization** | `auth/feature-spec-auth.md` | — | JWT auth + role-based authorization plan. Roles: Admin, Manager, Employee. `POST /api/auth/login|refresh|logout`. User management for Admins. Role-gated write actions on all existing endpoints. | Plan only |
