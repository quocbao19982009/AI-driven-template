# Feature Specification: Expense

**Last Updated:** `2026-03-10`
**Tests written:** no

---

## 1. Entity

**Name:** `Expense`
**Table name (plural):** `Expenses`

### Fields

| Property      | C# Type    | Required | Constraints                                                                 | Notes                          |
|---------------|------------|----------|-----------------------------------------------------------------------------|--------------------------------|
| `Title`       | `string`   | yes      | max 200 chars                                                               |                                |
| `Amount`      | `decimal`  | yes      | > 0                                                                         | e.g. 49.99                     |
| `Category`    | `string`   | yes      | max 100 chars; allowed values: Food, Transport, Housing, Health, Entertainment, Other | enum-like string   |
| `Date`        | `DateTime` | yes      | must not be in the future                                                   | date of the expense            |
| `Description` | `string`   | no       | max 500 chars                                                               | optional notes                 |

> `Id` (int), `CreatedAt`, `UpdatedAt` are inherited from `BaseEntity` — do not add them.

---

## Relationships

- none

---

## 2. Core Values & Principles

- Expenses are global records — no user ownership or authentication required
- Amount must always be a positive value; zero or negative amounts are invalid
- Category is constrained to a fixed set of values to enable consistent grouping and filtering
- Date must not be in the future — expenses are recorded after they occur

---

## 3. Architecture Decisions

### String-based Category instead of a separate Category entity

**Decision:** Store category as a constrained string on the Expense entity rather than a separate Categories table with a FK.
**Alternatives Considered:** Separate `Category` entity with a FK; C# enum mapped to int.
**Rationale:** The category list is small and fixed. A separate table adds join overhead and migration complexity with no benefit at this scale. A string is readable in the database and easy to filter on.

### No soft delete

**Decision:** Deleting an expense performs a hard delete.
**Alternatives Considered:** Soft delete with `DeletedAt` timestamp.
**Rationale:** Expense records are personal tracking data with no audit or compliance requirement. Hard delete keeps the schema simple and avoids filtering deleted rows on every query.

---

## 4. Data Flow

### Create
1. Client POST `/api/expenses` with request body
2. Controller calls Service
3. Service validates (amount > 0, category in allowed list, date not future), maps to entity, calls Repository
4. Repository saves, returns entity
5. Service maps to DTO, returns `ApiResponse<ExpenseDto>`

### Read (list)
1. Client GET `/api/expenses?page=1&pageSize=10&category=Food&dateFrom=...&dateTo=...`
2. Repository applies optional category + date range filters, `OrderByDescending(e => e.Date)`, Skip/Take
3. Returns `PagedResult<Expense>`
4. Service maps to `PagedResult<ExpenseDto>`

---

## 5. API Endpoints

| Method   | Route                | Description                              | Auth required |
|----------|----------------------|------------------------------------------|---------------|
| `GET`    | `/api/expenses`      | Paginated list (filter by category/date) | no            |
| `GET`    | `/api/expenses/{id}` | Get single expense                       | no            |
| `POST`   | `/api/expenses`      | Create new expense                       | no            |
| `PUT`    | `/api/expenses/{id}` | Full update                              | no            |
| `DELETE` | `/api/expenses/{id}` | Delete expense (204)                     | no            |

---

## 6. Validation Rules

- `Title`: required, not empty, max 200 characters
- `Amount`: required, must be > 0
- `Category`: required, must be one of: `Food`, `Transport`, `Housing`, `Health`, `Entertainment`, `Other`
- `Date`: required, must not be in the future
- `Description`: optional, max 500 characters

---

## 7. Business Rules

- none beyond validation

### Acceptance Scenarios

**Scenario: Create with valid data**
- Given: a POST `/api/expenses` with Title = "Groceries", Amount = 45.50, Category = "Food", Date = today
- When: the request is processed
- Then: returns 201 with the created Expense wrapped in `ApiResponse<ExpenseDto>`

**Scenario: Create with negative amount**
- Given: a POST `/api/expenses` with Amount = -10
- When: the request is processed
- Then: returns 400 with a validation error identifying the Amount field

**Scenario: Create with invalid category**
- Given: a POST `/api/expenses` with Category = "Shopping" (not in allowed list)
- When: the request is processed
- Then: returns 400 with a validation error identifying the Category field

**Scenario: Create with future date**
- Given: a POST `/api/expenses` with Date = tomorrow
- When: the request is processed
- Then: returns 400 with a validation error identifying the Date field

**Scenario: Get by non-existent ID**
- Given: a GET `/api/expenses/{id}` where no Expense with that ID exists
- When: the request is processed
- Then: returns 404 with NotFoundException message ("Expense {id} not found")

**Scenario: List filtered by category**
- Given: a GET `/api/expenses?category=Food&page=1&pageSize=10`
- When: the request is processed
- Then: returns 200 with only Food-category expenses, ordered by Date descending, with correct pagination metadata

---

## 8. Authorization

- none — all endpoints are public

---

## 9. Frontend UI

### Design reference

<!-- TODO: attach a Figma link or image if available — write "none" to skip -->
none

### Description

Page titled "Expenses" with a "New Expense" button in the top-right.

**Filters bar** (below header): category dropdown (All / Food / Transport / Housing / Health / Entertainment / Other) and a date-range picker (date from / date to). Filters trigger a re-fetch.

**Table columns:** Date, Title, Category (badge), Amount (right-aligned, formatted as currency), Description (truncated), and an Actions dropdown (Edit / Delete).

**Create / Edit** opens a modal dialog with fields: Title (text input), Amount (number input), Category (select), Date (date picker), Description (textarea, optional).

**Delete** opens a confirmation dialog: "Are you sure you want to delete this expense?" with Cancel and Delete buttons.

Empty state: centered message "No expenses yet. Add your first expense." with a "New Expense" button.

Skeleton loading while fetching.

### 10. Redux UI state

- `searchQuery: string`
- `selectedCategory: string` — active category filter ("" = All)
- `dateFrom: string` — ISO date string or ""
- `dateTo: string` — ISO date string or ""

---

## 11. File Locations

### Backend

| File                 | Path                                                                         |
|----------------------|------------------------------------------------------------------------------|
| Entity               | `backend/src/Backend.Api/Features/Expenses/Expense.cs`                       |
| DTOs                 | `backend/src/Backend.Api/Features/Expenses/ExpenseDtos.cs`                   |
| Validator            | `backend/src/Backend.Api/Features/Expenses/ExpensesValidator.cs`             |
| Repository interface | `backend/src/Backend.Api/Features/Expenses/IExpensesRepository.cs`           |
| Repository           | `backend/src/Backend.Api/Features/Expenses/ExpensesRepository.cs`            |
| Service interface    | `backend/src/Backend.Api/Features/Expenses/IExpensesService.cs`              |
| Service              | `backend/src/Backend.Api/Features/Expenses/ExpensesService.cs`               |
| Controller           | `backend/src/Backend.Api/Features/Expenses/ExpensesController.cs`            |

### Frontend

| File              | Path                                                                              |
|-------------------|-----------------------------------------------------------------------------------|
| Page component    | `frontend/src/features/expenses/components/expenses-page.tsx`                     |
| Table component   | `frontend/src/features/expenses/components/expenses-table.tsx`                    |
| Form dialog       | `frontend/src/features/expenses/components/expense-form-dialog.tsx`               |
| Delete dialog     | `frontend/src/features/expenses/components/expense-delete-dialog.tsx`             |
| Pagination hook   | `frontend/src/features/expenses/hooks/use-expenses-pagination.ts`                 |
| Redux slice       | `frontend/src/features/expenses/store/expenses-slice.ts`                          |
| Route             | `frontend/src/routes/expenses/index.tsx`                                          |
| Generated API     | `frontend/src/api/generated/expenses/`                                            |

---

## 12. Tests

**Tests written:** no

### Backend Unit Tests

| Test | Description |
|------|-------------|
| `CreateAsync_WithValidData_ReturnsExpenseDto` | Happy path create |
| `CreateAsync_WithNegativeAmount_ThrowsValidationException` | Amount ≤ 0 rejected |
| `CreateAsync_WithInvalidCategory_ThrowsValidationException` | Category not in allowed list |
| `CreateAsync_WithFutureDate_ThrowsValidationException` | Future date rejected |
| `GetAllAsync_ReturnsPaginatedResult` | Correct page and total |
| `GetAllAsync_FilteredByCategory_ReturnsOnlyMatchingExpenses` | Category filter works |
| `GetByIdAsync_WithInvalidId_ThrowsNotFoundException` | 404 for missing entity |

### Frontend Tests

| Test | Description |
|------|-------------|
| `ExpensesPage renders table` | Integration: table populated from mocked query |
| `Form dialog submits create request` | Correct payload sent on submit |
| `Category filter triggers re-fetch` | Filter dropdown change fires query with updated params |

---

## Migration Name

`AddExpenseEntity`

---

> **Marker convention:** Both marker types must be resolved before running `/scaffold-feature`:
> - `<!-- TODO: [what is missing] -->` — AI flags missing info that may have a reasonable default
> - `[NEEDS CLARIFICATION: [question] ]` — requires a specific human answer before scaffolding

## Checklist

### Backend
- [ ] Entity created in `Features/Expenses/Expense.cs`
- [ ] DTOs created in `Features/Expenses/ExpenseDtos.cs`
- [ ] Validator created in `Features/Expenses/ExpensesValidator.cs`
- [ ] Repository interface + implementation created
- [ ] Service interface + implementation created
- [ ] Controller created with correct routes
- [ ] Registered in `Program.cs`
- [ ] Migration created and applied

### API Sync
- [ ] `npm run api:sync` run from repo root
- [ ] `api/generated/expenses/` folder generated by Orval

### Frontend
- [ ] `features/expenses/` folder created with all layers
- [ ] Table columns match spec above
- [ ] Form fields match spec above
- [ ] Redux slice registered in `store/index.ts`
- [ ] Route added in `routes/expenses/index.tsx`
- [ ] Link added to navigation in `components/layout/app-layout.tsx`
