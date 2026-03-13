# Frontend Feature Generation Guide

> Referenced by CLAUDE.md. Describes the steps, file tree, and file roles for scaffolding a new frontend feature.

---

## File Tree

Each feature lives in `frontend/src/features/<feature-name>/` (kebab-case):

```
features/<feature-name>/
├── components/
│   ├── <feature-name>-page.tsx       → Full page — header + table + dialogs
│   ├── <feature-name>-table.tsx      → Paginated data table
│   ├── <feature>-form-dialog.tsx     → Create/edit dialog (react-hook-form + Zod)
│   └── <feature>-delete-dialog.tsx   → Delete confirmation dialog
├── hooks/
│   ├── use-<feature>-pagination.ts   → Pagination state + React Query wiring
│   └── use-<feature>-form.ts         → Form setup, submit handler, mutation calls
└── store/
    └── <feature>-slice.ts            → Redux UI-only state (search, selectedIds)
```

Route file (TanStack Router file-based routing):

```
routes/<feature-name>/index.tsx       → Thin route file — imports and renders the page component
```

---

## File Roles

| File | Purpose |
|---|---|
| `<feature>-page.tsx` | Composes header, table, and dialogs. Reads Redux UI state. No data fetching. |
| `<feature>-table.tsx` | Renders paginated rows. Calls pagination hook. Handles empty/loading states. |
| `<feature>-form-dialog.tsx` | Create + edit in one dialog. Controlled by `open` + `onClose` props. |
| `<feature>-delete-dialog.tsx` | Confirmation dialog. Takes entity id + name. Calls delete mutation on confirm. |
| `use-<feature>-pagination.ts` | Owns `page` state, calls Orval list hook, returns data + pagination controls. |
| `use-<feature>-form.ts` | Initializes react-hook-form with Zod schema, submits create/update mutation. |
| `<feature>-slice.ts` | Redux slice — `searchQuery`, `selectedIds`. No server data. |
| `routes/.../index.tsx` | One-liner that renders the page component inside the route. |

---

## Steps

### 1. Prerequisites

- `npm run api:sync` must be run first — Orval-generated hooks must exist in `api/generated/<feature>/`
- Never write data-fetching code by hand — always use the generated hooks

### 2. Redux slice

- UI-only state: `searchQuery: string`, `selectedIds: number[]`
- Register the slice in `store/store.ts`

### 3. Hooks

- `use-<feature>-pagination.ts` — wraps the Orval list hook; owns page/pageSize state
- `use-<feature>-form.ts` — builds from the Orval-generated Zod schema using `.extend()` to add translated error messages

### 4. Components

- All component files use **kebab-case** filenames (`feature-page.tsx`, not `FeaturePage.tsx`)
- All exports are **named exports** (no default exports)
- All visible strings go through `t()` from `useTranslation()` — never hardcode text
- Use `cn()` from `lib/utils.ts` for Tailwind class merging

### 5. Route

TanStack Router file-based routing — create `routes/<feature>/index.tsx`:
```tsx
import { createFileRoute } from "@tanstack/react-router";
import { FeaturePage } from "@/features/<feature>/components/<feature>-page";

export const Route = createFileRoute("/<feature>/")({
  component: FeaturePage,
});
```

### 6. Navigation

Add a nav link in `components/layout/app-layout.tsx` using a translation key (`t("nav.<feature>")`).

### 7. Translations

Add keys to **both** `src/locales/en.json` and `src/locales/fi.json`:
```json
"<feature>": {
  "title": "...",
  "newItem": "...",
  "table": { "id": "ID", "name": "Name", "createdAt": "Created At", "empty": "No items found." },
  "form": { "createTitle": "...", "editTitle": "...", "name": "Name", "namePlaceholder": "..." },
  "delete": { "title": "...", "description": "..." },
  "toast": { "created": "...", "updated": "...", "deleted": "...", "error": "..." }
}
```

---

## Key Infrastructure (do not reinvent)

| Hook / Utility | Location | Purpose |
|---|---|---|
| `useAppDispatch` / `useAppSelector` | `store/hooks.ts` | Typed Redux hooks |
| `apiFetch` | `api/mutator/apiFetch.ts` | All HTTP — Orval uses it automatically |
| `cn()` | `lib/utils.ts` | Tailwind class merging |
| `useDebounce` | `hooks/use-debounce.ts` | Debounce search input |
| `useTranslation` | `react-i18next` | All UI strings |

---

## Reference Implementation

See `features/_template-feature/` for the canonical example of all component, hook, and store files.
