# Feature Spec: Flashcards

## 1. Entity

**`Flashcard`** extends `BaseEntity` (provides `Id`, `CreatedAt`, `UpdatedAt`).

| Field                | C# Type      | Required | Constraints    |
|----------------------|-------------|----------|----------------|
| `FinnishWord`        | `string`    | yes      | max 500 chars  |
| `EnglishTranslation` | `string`    | yes      | max 500 chars  |
| `Category`           | `string`    | yes      | max 100 chars  |
| `NextReviewDate`     | `DateTime?` | no       |                |
| `LastReviewedAt`     | `DateTime?` | no       |                |

No relationships. Category is a plain string (no FK).

## 2. Core Values & Principles

- Simple vocabulary learning tool — no spaced repetition algorithm
- No authentication required — all endpoints public
- Category as free-text string with datalist suggestions in UI

## 3. Architecture Decisions

- Standard Controller → Service → Repository → DbContext pattern
- No `/all` unpaged endpoint (max 10 cards per category keeps data small)
- `PATCH /{id}/review` for marking cards as reviewed (sets `LastReviewedAt`, clears `NextReviewDate`)

## 4. Data Flow

1. Frontend calls API via Orval-generated hooks
2. Controller delegates to `FlashcardsService`
3. Service validates via FluentValidation, checks business rules, calls repository
4. Repository executes EF Core queries with `.AsNoTracking()` on reads

## 5. API Endpoints

| Method  | Route                          | Description                                      |
|---------|--------------------------------|--------------------------------------------------|
| GET     | `/api/flashcards`              | Paginated list. Query: `page`, `pageSize`, `search`, `category` |
| GET     | `/api/flashcards/{id}`         | Single card by ID                                |
| POST    | `/api/flashcards`              | Create (enforces 10-card-per-category limit)     |
| PUT     | `/api/flashcards/{id}`         | Update (enforces limit if category changes)      |
| DELETE  | `/api/flashcards/{id}`         | Delete                                           |
| PATCH   | `/api/flashcards/{id}/review`  | Mark reviewed: `LastReviewedAt = now`, `NextReviewDate = null` |

## 6. Validation Rules

- `FinnishWord`: required, max 500 chars
- `EnglishTranslation`: required, max 500 chars
- `Category`: required, max 100 chars
- Page: >= 1
- PageSize: 1–100

## 7. Business Rules

1. **Max 10 cards per category** — enforced on create and on update (only when category changes). Returns validation error if limit exceeded.
2. **PATCH /review** — sets `LastReviewedAt = DateTime.UtcNow`, clears `NextReviewDate`. No scheduling logic.
3. **No auth** — all endpoints are public.

## 8. Authorization

None. All endpoints are public (no `[Authorize]`).

## 9. Frontend UI Description

Two-tab layout on `/flashcards`:

### Manage Tab
- Header with title, description
- Tab buttons (Manage / Study)
- Search input + category filter dropdown
- "New Flashcard" button → opens form dialog
- Table with columns: ID, Finnish, English, Category, Last Reviewed, Actions (edit/delete)
- Pagination controls

### Study Tab
- Category picker dropdown
- Flip-card UI: shows Finnish word, click to reveal English translation
- Previous / Next navigation buttons with card counter
- "Restart" button to go back to first card
- "Mark as Reviewed" button to call PATCH /review

### Form Dialog
- Fields: Finnish Word, English Translation, Category (with datalist for existing categories)
- Create and Edit modes

### Delete Dialog
- Confirmation with flashcard's Finnish word displayed

## 10. Redux UI State

```typescript
interface FlashcardsUiState {
  searchQuery: string;
  selectedIds: number[];
  activeCategoryFilter: string;
  activeTab: "manage" | "study";
  studyCategory: string;
}
```

## 11. File Locations

### Backend (`backend/src/Backend.Api/Features/Flashcards/`)
- `Flashcard.cs` — entity
- `FlashcardDtos.cs` — DTO record + request classes
- `FlashcardsValidator.cs` — FluentValidation
- `IFlashcardsRepository.cs` — repository interface
- `FlashcardsRepository.cs` — EF Core implementation
- `IFlashcardsService.cs` — service interface
- `FlashcardsService.cs` — business logic
- `FlashcardsController.cs` — REST controller

### Frontend (`frontend/src/features/flashcards/`)
- `store/flashcards-slice.ts` — Redux slice
- `store/index.ts` — barrel
- `hooks/use-flashcards-pagination.ts` — paginated query hook
- `hooks/use-flashcard-categories.ts` — distinct categories hook
- `hooks/index.ts` — barrel
- `components/flashcards-page.tsx` — main page with tabs
- `components/flashcards-table.tsx` — data table
- `components/flashcard-form-dialog.tsx` — create/edit dialog
- `components/flashcard-delete-dialog.tsx` — delete confirmation
- `components/flashcard-study-mode.tsx` — flip-card study UI
- `index.ts` — feature barrel

### Route
- `frontend/src/routes/flashcards/index.tsx`

## 12. Tests

No tests implemented yet.
