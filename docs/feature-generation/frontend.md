# Frontend Feature Generation

## Directory Structure

```text
frontend/src/
├── api/
│   ├── generated/          # Orval output — do not edit manually
│   └── mutator/            # Custom fetch wrapper (apiFetch.ts)
├── components/
│   ├── ui/                 # Shared shadcn primitives (button, input, etc.)
│   └── layout/             # App shell, navbar, sidebar
├── features/               # Feature-based modules
│   └── [feature-name]/
│       ├── components/     # Feature-specific UI components
│       ├── hooks/          # Feature-specific custom hooks
│       ├── store/          # Feature Redux slice (UI state only)
│       └── index.ts        # Barrel export (public API)
├── hooks/                  # Shared custom hooks (useDebounce, useMediaQuery)
├── lib/                    # Pure utility functions (cn, formatDate, etc.)
├── providers/              # React context providers
├── routes/                 # TanStack Router file-based routes (keep thin)
├── store/                  # Redux root store config + typed hooks
├── types/                  # Shared TypeScript types
├── main.tsx
└── index.css
```

---

## Steps

1. Copy `features/_template-feature/` → `features/[name]/`, rename `Feature` → entity name throughout
2. Update components to match the entity's fields and UI description
3. Update Orval imports: `api/generated/feature/` → `api/generated/[name]/`
4. Update the Redux slice name in `[name]-slice.ts` from `"features"` → `"[name]"`
5. Register the slice in `store/index.ts`
6. Add route file in `routes/[name]/index.tsx` — keep it thin, just render the page component
7. Add translation keys under a `"[name]"` namespace in both `src/locales/en.json` and `src/locales/fi.json` — mirror the `"features"` namespace structure
8. All visible strings must use `t()` — never write hardcoded English text in JSX

---

## File Roles (canonical example: `features/features/`)

| File | Role |
|------|------|
| `features-page.tsx` | Page layout: header, action button, main content |
| `features-table.tsx` | Data table with skeleton loading and pagination |
| `feature-form-dialog.tsx` | Create/edit dialog, dual-mode based on whether a record is passed |
| `feature-delete-dialog.tsx` | Confirmation delete dialog |
| `use-feature-pagination.ts` | Wraps Orval list query, owns page state |
| `use-feature-form.ts` | Wraps react-hook-form + Zod, reuses Orval-generated schema |
| `features-slice.ts` | Redux slice for UI-only state (searchQuery, selectedIds) |
| `index.ts` | Barrel export |
