# Feature Spec: Flashcards

## 1. Entity

**`FlashcardCategory`** extends `BaseEntity` (provides `Id`, `CreatedAt`, `UpdatedAt`).

| Field  | C# Type  | Required | Constraints              |
|--------|----------|----------|--------------------------|
| `Name` | `string` | yes      | max 100 chars, unique    |

**`Flashcard`** extends `BaseEntity` (provides `Id`, `CreatedAt`, `UpdatedAt`).

| Field                | C# Type               | Required | Constraints   |
|----------------------|-----------------------|----------|---------------|
| `FinnishWord`        | `string`              | yes      | max 500 chars |
| `EnglishTranslation` | `string`              | yes      | max 500 chars |
| `CategoryId`         | `int?`                | no       | FK → `FlashcardCategory` |
| `Category`           | `FlashcardCategory?`  | no       | navigation property |
| `NextReviewDate`     | `DateTime?`           | no       |               |
| `LastReviewedAt`     | `DateTime?`           | no       |               |

**Relationships:**
- `Flashcard` → `FlashcardCategory`: many-to-one (nullable FK). On category delete: `SetNull` (flashcard becomes uncategorized).

## 2. Core Values & Principles

- Simple vocabulary learning tool — no spaced repetition algorithm
- No authentication required — all endpoints public
- Categories are a separate entity with full CRUD; flashcard form uses a dropdown to select category

## 3. Architecture Decisions

- Standard Controller → Service → Repository → DbContext pattern
- `FlashcardCategory` has its own full CRUD stack (8 files)
- `/all` unpaged endpoint on `FlashcardCategories` for dropdown consumption
- `PATCH /{id}/review` for marking cards as reviewed (sets `LastReviewedAt`, clears `NextReviewDate`)
- `FlashcardsService` depends on `IFlashcardCategoriesRepository` to validate category existence

## 4. Data Flow

1. Frontend calls API via Orval-generated hooks
2. Controller delegates to service
3. Service validates via FluentValidation, checks business rules (category exists, card limit), calls repository
4. Repository executes EF Core queries with `.AsNoTracking()` on reads, `.Include(f => f.Category)` for flashcard reads

## 5. API Endpoints

### FlashcardCategories

| Method | Route                              | Description                |
|--------|-------------------------------------|----------------------------|
| GET    | `/api/flashcard-categories`         | Paginated list. Query: `page`, `pageSize`, `search` |
| GET    | `/api/flashcard-categories/all`     | Unpaged list for dropdowns |
| GET    | `/api/flashcard-categories/{id}`    | Single category by ID      |
| POST   | `/api/flashcard-categories`         | Create (unique name check) |
| PUT    | `/api/flashcard-categories/{id}`    | Update (unique name check) |
| DELETE | `/api/flashcard-categories/{id}`    | Delete (FK → SetNull on flashcards) |

### Flashcards

| Method  | Route                          | Description                                      |
|---------|--------------------------------|--------------------------------------------------|
| GET     | `/api/flashcards`              | Paginated list. Query: `page`, `pageSize`, `search`, `categoryId` |
| GET     | `/api/flashcards/{id}`         | Single card by ID (includes category name)       |
| POST    | `/api/flashcards`              | Create (validates category exists, enforces 10-card-per-category limit) |
| PUT     | `/api/flashcards/{id}`         | Update (validates category exists, enforces limit if category changes) |
| DELETE  | `/api/flashcards/{id}`         | Delete                                           |
| PATCH   | `/api/flashcards/{id}/review`  | Mark reviewed: `LastReviewedAt = now`, `NextReviewDate = null` |

## 6. Validation Rules

### FlashcardCategory
- `Name`: required, max 100 chars, unique (checked in service)

### Flashcard
- `FinnishWord`: required, max 500 chars
- `EnglishTranslation`: required, max 500 chars
- `CategoryId`: > 0 when provided (optional)
- Page: >= 1
- PageSize: 1–100

## 7. Business Rules

1. **Max 10 cards per category** — enforced on create and on update (only when category changes). Returns validation error if limit exceeded. Only checked when `CategoryId` is provided.
2. **Category existence check** — on create/update, if `CategoryId` is provided, the category must exist.
3. **PATCH /review** — sets `LastReviewedAt = DateTime.UtcNow`, clears `NextReviewDate`. No scheduling logic.
4. **Unique category name** — enforced on create/update (case-insensitive).
5. **Category delete → SetNull** — deleting a category nullifies `CategoryId` on linked flashcards.
6. **No auth** — all endpoints are public.

## 8. Authorization

None. All endpoints are public (no `[Authorize]`).

## 9. Frontend UI Description

Two-tab layout on `/flashcards`:

### Manage Tab
- Header with title, description
- Tab buttons (Manage / Study)
- Search input + category filter dropdown (uses category objects with id+name)
- "New Flashcard" button → opens form dialog
- Table with columns: ID, Finnish, English, Category Name, Last Reviewed, Actions (edit/delete)
- Pagination controls
- **Category Manager panel** — inline CRUD for categories: list with add/edit/delete inline, delete confirmation dialog

### Study Tab
- Category picker dropdown (uses category objects with id+name)
- Flip-card UI: shows Finnish word, click to reveal English translation
- Previous / Next navigation buttons with card counter
- "Restart" button to go back to first card
- "Mark as Reviewed" button to call PATCH /review

### Form Dialog
- Fields: Finnish Word, English Translation, Category (select dropdown from category entity list)
- Create and Edit modes
- Uses unified form with zod schema

### Delete Dialog
- Confirmation with flashcard's Finnish word displayed

## 10. Redux UI State

```typescript
interface FlashcardsUiState {
  searchQuery: string;
  selectedIds: number[];
  activeCategoryFilter: string; // category ID as string, "" = all
  activeTab: "manage" | "study";
  studyCategory: string; // category ID as string, "" = none selected
}
```

## 11. File Locations

### Backend — FlashcardCategories (`backend/src/Backend.Api/Features/FlashcardCategories/`)
- `FlashcardCategory.cs` — entity
- `FlashcardCategoryDtos.cs` — DTO record + request classes
- `FlashcardCategoriesValidator.cs` — FluentValidation
- `IFlashcardCategoriesRepository.cs` — repository interface
- `FlashcardCategoriesRepository.cs` — EF Core implementation
- `IFlashcardCategoriesService.cs` — service interface
- `FlashcardCategoriesService.cs` — business logic
- `FlashcardCategoriesController.cs` — REST controller

### Backend — Flashcards (`backend/src/Backend.Api/Features/Flashcards/`)
- `Flashcard.cs` — entity (CategoryId FK + Category nav property)
- `FlashcardDtos.cs` — DTO record (includes `categoryId`, `categoryName`) + request classes
- `FlashcardsValidator.cs` — FluentValidation
- `IFlashcardsRepository.cs` — repository interface
- `FlashcardsRepository.cs` — EF Core implementation (`.Include(f => f.Category)`)
- `IFlashcardsService.cs` — service interface
- `FlashcardsService.cs` — business logic (depends on `IFlashcardCategoriesRepository`)
- `FlashcardsController.cs` — REST controller

### Frontend — Flashcard Categories (`frontend/src/features/flashcard-categories/`)
- `components/flashcard-category-manager.tsx` — inline CRUD panel

### Frontend — Flashcards (`frontend/src/features/flashcards/`)
- `store/flashcards-slice.ts` — Redux slice
- `store/index.ts` — barrel
- `hooks/use-flashcards-pagination.ts` — paginated query hook (uses `categoryId`)
- `hooks/use-flashcard-categories.ts` — calls `useGetApiFlashcardCategoriesAll`
- `hooks/index.ts` — barrel
- `components/flashcards-page.tsx` — main page with tabs + category manager
- `components/flashcards-table.tsx` — data table (displays `categoryName`)
- `components/flashcard-form-dialog.tsx` — create/edit dialog (select dropdown for category)
- `components/flashcard-delete-dialog.tsx` — delete confirmation
- `components/flashcard-study-mode.tsx` — flip-card study UI (filters by `categoryId`)
- `index.ts` — feature barrel

### Route
- `frontend/src/routes/flashcards/index.tsx`

## 12. Tests

No tests implemented yet.
