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

#### Form validation schema rules

Always start from the generated `Post<Feature>Body` schema (from `api/generated/<feature>/<feature>.zod.ts`) and use `.extend()` — never define a schema from scratch.

```ts
// Inside the component so t() is in scope
const featureFormSchema = PostApiFeatureBody.extend({
  name: z.string().min(1, t("feature.validation.nameRequired")).max(200),
  // override other fields with translated messages as needed
});
type FeatureFormValues = z.infer<typeof featureFormSchema>;
```

**Zod v4 notes (the project uses Zod v4):**
- Use `error:` not `invalid_type_error:` in `z.number({ error: "..." })`
- For `datetime-local` inputs (which produce `"2024-01-01T10:00"` without a `Z`), override the datetime field to `z.string().min(1, t(...))` — the generated schema uses `zod.iso.datetime({})` which rejects local datetime strings

**Cross-field validation** (e.g. start < end) is UI-only — add via `.refine()` after `.extend()`:
```ts
.refine(
  (data) => new Date(data.startTime) < new Date(data.endTime),
  { message: t("feature.validation.startBeforeEnd"), path: ["endTime"] }
)
```

The schema and its `type FormValues` must be declared **inside** the component function so `t()` is in scope.

### 3.5 Cache Invalidation Design

Before writing any dialog component, answer these three questions and note the answers:

1. **Self-invalidation** — Which Orval-generated query key covers this feature's list? (e.g. `getGetFactoriesQueryKey()`)
   Every mutation must call `queryClient.invalidateQueries({ queryKey: getGet<Feature>QueryKey() })` in `onSuccess`.

2. **Cross-feature invalidation** — Does mutating this feature change data displayed by *another* feature?
   If yes, import that feature's query key function and add a second `invalidateQueries` call in the same `onSuccess`.

3. **Reverse dependency** — Does this feature embed data from another entity (e.g. User name, category label)?
   If yes, that other entity's mutation dialogs must also invalidate this feature's query key.

#### Pattern (standard case)

```ts
import { useQueryClient } from "@tanstack/react-query";
import {
  usePostApiFactories,
  getGetApiFactoriesQueryKey,
} from "@/api/generated/factories/factories";

const queryClient = useQueryClient();

const createMutation = usePostApiFactories({
  mutation: {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: getGetApiFactoriesQueryKey() });
      // If this mutation also affects another feature's cached data, add:
      // queryClient.invalidateQueries({ queryKey: getGetApi<OtherFeature>QueryKey() });
      onOpenChange(false);
      toast.success(t("factories.toast.created"));
    },
  },
});
```

Always use the Orval-generated `getGet<Feature>QueryKey()` — never hardcode query key strings.

`invalidateQueries` marks the cache stale and triggers a background refetch the next time the query is observed. Use `refetchQueries` only if you need an immediate refetch even when the query is not currently mounted.

---

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
