# Feature Specification: ExpenseTracker

**Last Updated:** `2026-03-13`
**Tests written:** no

---

## 1. Entity

**Name:** `ExpenseTracker`
**Table name (plural):** `ExpenseTrackers`

### Fields

| Property      | C# Type    | Required | Constraints                                                                    | Notes                                             |
| ------------- | ---------- | -------- | ------------------------------------------------------------------------------ | ------------------------------------------------- |
| `Amount`      | `decimal`  | yes      | > 0, ≤ 999999.99, 2 decimal places                                             |                                                   |
| `Category`    | `string`   | yes      | max 50 chars; must be one of: Food, Transport, Utilities, Entertainment, Other | Validated via FluentValidation                    |
| `Description` | `string?`  | no       | max 500 chars                                                                  | Optional free-text                                |
| `Date`        | `DateTime` | yes      | —                                                                              | When the expense occurred                         |
| `UserId`      | `int`      | yes      | FK → User                                                                      | Set from JWT `sub` claim, never from request body |

> `Id` (int), `CreatedAt`, `UpdatedAt` are inherited from `BaseEntity` — do not add them.

---

## Relationships

- Belongs to `User` (`UserId` FK, required, navigation property `User`)

---

## 2. Core Values & Principles

- Shared visibility: every user (and anonymous visitors) can browse all expenses regardless of who submitted them
- Ownership integrity: only the submitter or an admin may modify or delete an expense
- `UserId` is system-assigned from the JWT token — it is never client-supplied, preventing impersonation
- Category values are validated against a fixed allow-list to keep data clean

---

## 3. Architecture Decisions

- Standard layered pattern: Controller → Service → Repository → EF Core
- Ownership and admin-override checks live in the **Service layer** (not controller or repository) to keep authorization logic centralized and testable
- A new `ForbiddenAccessException` is introduced in `Common/Exceptions/` and mapped to HTTP 403 in `ErrorHandlingMiddleware`
- Category is stored as a plain string (not a C# enum) for API simplicity; validated via FluentValidation `.Must()` rule

---

## 4. Data Flow

### Create

1. Client POST `/api/expense-trackers` with `{ amount, category, description, date }`
2. Controller extracts `userId` from JWT `sub` claim
3. Controller calls `ExpenseTrackersService.CreateAsync(request, userId, ct)`
4. Service validates via FluentValidation, maps to entity (sets `UserId`), calls Repository
5. Repository saves to DB, returns entity
6. Service maps to `ExpenseTrackerDto` (includes submitter name), returns `ApiResponse<ExpenseTrackerDto>`

### Read (list)

1. Client GET `/api/expense-trackers?page=1&pageSize=20` (no auth required)
2. Repository queries with `.Include(e => e.User)`, `.AsNoTracking()`, `.OrderByDescending(e => e.Date)`, `.Skip()/.Take()`
3. Service maps to DTOs with submitter name

### Update

1. Client PUT `/api/expense-trackers/{id}` with `{ amount, category, description, date }`
2. Controller extracts `userId` and `role` from JWT
3. Service fetches expense, checks `expense.UserId == userId || role == "Admin"` — throws `ForbiddenAccessException` if neither
4. Service validates, updates, saves, returns updated DTO

### Delete

1. Client DELETE `/api/expense-trackers/{id}`
2. Same ownership/admin check as Update
3. Repository deletes, returns 204

---

## 5. API Endpoints

| Method   | Route                        | Description                  | Auth required |
| -------- | ---------------------------- | ---------------------------- | ------------- |
| `GET`    | `/api/expense-trackers`      | Paginated list (public)      | no            |
| `GET`    | `/api/expense-trackers/{id}` | Get single expense (public)  | no            |
| `POST`   | `/api/expense-trackers`      | Create new expense           | yes           |
| `PUT`    | `/api/expense-trackers/{id}` | Update (owner or admin only) | yes           |
| `DELETE` | `/api/expense-trackers/{id}` | Delete (owner or admin only) | yes           |

---

## 6. Validation Rules

- `Amount`: required, must be > 0, must be ≤ 999999.99
- `Category`: required, not empty, max 50 characters, must be one of `Food`, `Transport`, `Utilities`, `Entertainment`, `Other`
- `Description`: optional; when provided, max 500 characters
- `Date`: required

---

## 7. Business Rules

1. **UserId from JWT:** On create, `UserId` is extracted from the JWT `sub` claim — it is never accepted in the request body
2. **Ownership check (Update):** Service verifies `expense.UserId == currentUserId || currentUserRole == "Admin"`. If neither, throws `ForbiddenAccessException`
3. **Ownership check (Delete):** Same rule as Update
4. **Public read:** GET endpoints require no authentication — anyone can view all expenses

### Acceptance Scenarios

**Scenario: Create with valid data**

- Given: an authenticated POST `/api/expense-trackers` with `{ amount: 42.50, category: "Food", description: "Lunch", date: "2026-03-13" }`
- When: the request is processed
- Then: returns 201 with the created expense wrapped in `ApiResponse<ExpenseTrackerDto>`, `userId` matches the JWT `sub` claim, and `submittedBy` shows the user's name

**Scenario: Get all expenses (unauthenticated)**

- Given: a GET `/api/expense-trackers?page=1&pageSize=20` with no auth token
- When: the request is processed
- Then: returns 200 with a paginated list of all expenses with submitter names

**Scenario: Get by ID (unauthenticated)**

- Given: a GET `/api/expense-trackers/5` with no auth token and expense 5 exists
- When: the request is processed
- Then: returns 200 with the single expense wrapped in `ApiResponse<ExpenseTrackerDto>`

**Scenario: Owner updates own expense**

- Given: an authenticated PUT `/api/expense-trackers/5` where `expense.UserId == currentUserId`
- When: the request is processed with valid data
- Then: returns 200 with the updated expense

**Scenario: Admin updates any expense**

- Given: an authenticated PUT `/api/expense-trackers/5` where the current user has role "Admin" but does not own the expense
- When: the request is processed with valid data
- Then: returns 200 with the updated expense

**Scenario: Non-owner non-admin tries to update**

- Given: an authenticated PUT `/api/expense-trackers/5` where `expense.UserId != currentUserId` and role != "Admin"
- When: the request is processed
- Then: returns 403 Forbidden

**Scenario: Owner deletes own expense**

- Given: an authenticated DELETE `/api/expense-trackers/5` where `expense.UserId == currentUserId`
- When: the request is processed
- Then: returns 204 No Content

**Scenario: Admin deletes any expense**

- Given: an authenticated DELETE `/api/expense-trackers/5` where the current user has role "Admin"
- When: the request is processed
- Then: returns 204 No Content

**Scenario: Non-owner non-admin tries to delete**

- Given: an authenticated DELETE `/api/expense-trackers/5` where `expense.UserId != currentUserId` and role != "Admin"
- When: the request is processed
- Then: returns 403 Forbidden

**Scenario: Create with amount ≤ 0**

- Given: an authenticated POST `/api/expense-trackers` with `{ amount: -5, ... }`
- When: the request is processed
- Then: returns 400 with a validation error on Amount

**Scenario: Create with amount > 999999.99**

- Given: an authenticated POST `/api/expense-trackers` with `{ amount: 1000000, ... }`
- When: the request is processed
- Then: returns 400 with a validation error on Amount

**Scenario: Create with invalid category**

- Given: an authenticated POST `/api/expense-trackers` with `{ category: "Shopping", ... }`
- When: the request is processed
- Then: returns 400 with a validation error on Category

**Scenario: Get by non-existent ID**

- Given: a GET `/api/expense-trackers/99999` where no expense with ID 99999 exists
- When: the request is processed
- Then: returns 404 with NotFoundException message

**Scenario: Unauthenticated create attempt**

- Given: a POST `/api/expense-trackers` with no auth token
- When: the request is processed
- Then: returns 401 Unauthorized

---

## 8. Authorization

- GET endpoints are **public** — no authentication required (`[AllowAnonymous]`)
- POST, PUT, DELETE require `[Authorize]`
- PUT and DELETE enforce ownership in the **service layer**: `expense.UserId == currentUserId || role == "Admin"`
- Admin role bypasses ownership check on update and delete

---

## 9. Frontend UI

### Design reference

No Figma design — standard table + form dialog pattern.

### Description

Page header with "Expense Tracker" title and a "New Expense" button (visible only when logged in). Below the header, a filter bar with a search input (searches description, debounced) and a category dropdown filter (options: All, Food, Transport, Utilities, Entertainment, Other).

Paginated table with columns: Amount (formatted as currency), Category, Description, Date (formatted), Submitted By (user's first + last name), and an Actions column with Edit/Delete dropdown (visible only if current user is the owner or an admin — hidden when not logged in).

Create/Edit opens a modal dialog with fields: Amount (number input), Category (select dropdown with the 5 allowed values), Description (textarea, optional), Date (date picker). Delete opens a confirmation dialog.

Skeleton loading while fetching. Empty state message when no expenses exist.

### 10. Redux UI state

- `searchQuery: string` — description search input
- `categoryFilter: string` — active category filter ("all" | "Food" | "Transport" | "Utilities" | "Entertainment" | "Other")

---

## 11. File Locations

### Backend

| File                 | Path                                                                             |
| -------------------- | -------------------------------------------------------------------------------- |
| Entity               | `backend/src/Backend.Api/Features/ExpenseTrackers/ExpenseTracker.cs`             |
| DTOs                 | `backend/src/Backend.Api/Features/ExpenseTrackers/ExpenseTrackerDtos.cs`         |
| Validator            | `backend/src/Backend.Api/Features/ExpenseTrackers/ExpenseTrackersValidator.cs`   |
| Repository interface | `backend/src/Backend.Api/Features/ExpenseTrackers/IExpenseTrackersRepository.cs` |
| Repository           | `backend/src/Backend.Api/Features/ExpenseTrackers/ExpenseTrackersRepository.cs`  |
| Service interface    | `backend/src/Backend.Api/Features/ExpenseTrackers/IExpenseTrackersService.cs`    |
| Service              | `backend/src/Backend.Api/Features/ExpenseTrackers/ExpenseTrackersService.cs`     |
| Controller           | `backend/src/Backend.Api/Features/ExpenseTrackers/ExpenseTrackersController.cs`  |
| ForbiddenException   | `backend/src/Backend.Api/Common/Exceptions/ForbiddenAccessException.cs`          |

### Frontend

| File            | Path                                                                                  |
| --------------- | ------------------------------------------------------------------------------------- |
| Page component  | `frontend/src/features/expense-trackers/components/expense-trackers-page.tsx`         |
| Table component | `frontend/src/features/expense-trackers/components/expense-trackers-table.tsx`        |
| Form dialog     | `frontend/src/features/expense-trackers/components/expense-tracker-form-dialog.tsx`   |
| Delete dialog   | `frontend/src/features/expense-trackers/components/expense-tracker-delete-dialog.tsx` |
| Pagination hook | `frontend/src/features/expense-trackers/hooks/use-expense-trackers-pagination.ts`     |
| Redux slice     | `frontend/src/features/expense-trackers/store/expense-trackers-slice.ts`              |
| Route           | `frontend/src/routes/expense-trackers/index.tsx`                                      |
| Generated API   | `frontend/src/api/generated/expense-trackers/`                                        |

---

## 12. Tests

**Tests written:** no

### Backend Unit Tests

| Test                                                    | Description                             |
| ------------------------------------------------------- | --------------------------------------- |
| `CreateAsync_WithValidData_ReturnsExpenseTrackerDto`    | Happy path create                       |
| `CreateAsync_SetsUserIdFromParameter`                   | UserId comes from JWT, not request body |
| `GetAllAsync_ReturnsPaginatedResult`                    | Correct page and total                  |
| `GetByIdAsync_WithInvalidId_ThrowsNotFoundException`    | 404 for missing entity                  |
| `UpdateAsync_AsOwner_ReturnsUpdatedDto`                 | Owner can update own expense            |
| `UpdateAsync_AsAdmin_ReturnsUpdatedDto`                 | Admin can update any expense            |
| `UpdateAsync_AsNonOwner_ThrowsForbiddenAccessException` | 403 for non-owner non-admin             |
| `DeleteAsync_AsOwner_Succeeds`                          | Owner can delete own expense            |
| `DeleteAsync_AsAdmin_Succeeds`                          | Admin can delete any expense            |
| `DeleteAsync_AsNonOwner_ThrowsForbiddenAccessException` | 403 for non-owner non-admin             |

### Frontend Tests

| Test                                 | Description                                    |
| ------------------------------------ | ---------------------------------------------- |
| `ExpenseTrackersPage renders table`  | Integration: table populated from mocked query |
| `Form dialog submits create request` | Correct payload sent                           |

---

## Seed Data

- Reuse existing `admin@example.com` admin user (seeded by `DataSeeder.SeedAdminUserAsync`)
- Seed 3 regular users: `user1@example.com`, `user2@example.com`, `user3@example.com` (password: `password123`)
- Seed ~10 sample expenses spread across all 4 users and all 5 categories
- Follow existing idempotent pattern with `AnyAsync()` guard

---

## Migration Name

`AddExpenseTrackerEntity`

---

## Checklist

### Backend

- [ ] Entity created in `Features/ExpenseTrackers/ExpenseTracker.cs`
- [ ] DTOs created in `Features/ExpenseTrackers/ExpenseTrackerDtos.cs`
- [ ] Validator created in `Features/ExpenseTrackers/ExpenseTrackersValidator.cs`
- [ ] Repository interface + implementation created
- [ ] Service interface + implementation created
- [ ] Controller created with correct routes and auth attributes
- [ ] ForbiddenAccessException added to Common/Exceptions and ErrorHandlingMiddleware updated
- [ ] Registered in `Program.cs`
- [ ] Migration created and applied
- [ ] Seed data added for test users and expenses

### API Sync

- [ ] `npm run api:sync` run from repo root
- [ ] `api/generated/expense-trackers/` folder generated by Orval

### Frontend

- [ ] `features/expense-trackers/` folder created with all layers
- [ ] Table columns match spec above
- [ ] Form fields match spec above
- [ ] Category filter dropdown implemented
- [ ] Actions column conditionally visible based on auth state
