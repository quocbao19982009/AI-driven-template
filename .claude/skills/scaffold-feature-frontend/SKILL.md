---
name: scaffold-feature-frontend
description: Scaffold the frontend for a feature after api:sync has been run. Creates page, table, dialogs, hooks, Redux slice, route, navigation, translations, and tests. Second half of the full-stack workflow.
argument-hint: "[feature-name]"
disable-model-invocation: true
allowed-tools: "Read, Write, Edit, Bash, Glob, Grep, mcp__shadcn__search_items_in_registries, mcp__shadcn__view_items_in_registries, mcp__shadcn__get_add_command_for_items"
---

# Scaffold Frontend Feature

Scaffold the complete frontend for the `$ARGUMENTS` feature.

## Prerequisites

### Step P1: Verify Orval output exists

Check for files at:
```
frontend/src/api/generated/$ARGUMENTS/
```
If missing or empty, **stop immediately**:
> "No Orval output found at `frontend/src/api/generated/$ARGUMENTS/`. Run `/api-sync` first."

### Step P2: Read the feature spec
```
feature_docs/$ARGUMENTS/feature-spec-$ARGUMENTS.md
```

### Step P3: Completeness gate — scan for unresolved markers

Scan the full spec text for:
- `<!-- TODO:`
- `[NEEDS CLARIFICATION:`

If ANY markers are found, **stop immediately** with this refusal (do NOT proceed to Step 1):

> **Cannot scaffold frontend: the spec has unresolved markers.**
>
> Resolve each of the following before re-running `/scaffold-feature-frontend $ARGUMENTS`:
>
> [List every found marker verbatim, one per line, prefixed with the nearest `##` or `###` heading it appears under]
>
> Run `/clarify-spec $ARGUMENTS` to resolve them interactively, or edit `feature_docs/$ARGUMENTS/feature-spec-$ARGUMENTS.md` directly.

Only proceed to Step 1 when no markers remain.

## Step 1: Read the Templates

Read ALL of these files to understand the patterns:

**Frontend feature template:**
- `frontend/src/features/_template-feature/components/features-page.tsx`
- `frontend/src/features/_template-feature/components/features-table.tsx`
- `frontend/src/features/_template-feature/components/feature-form-dialog.tsx`
- `frontend/src/features/_template-feature/components/feature-delete-dialog.tsx`
- `frontend/src/features/_template-feature/hooks/use-feature-pagination.ts`
- `frontend/src/features/_template-feature/hooks/use-feature-form.ts`
- `frontend/src/features/_template-feature/hooks/index.ts`
- `frontend/src/features/_template-feature/store/features-slice.ts`
- `frontend/src/features/_template-feature/store/index.ts`
- `frontend/src/features/_template-feature/index.ts`

**Test templates:**
- `frontend/src/features/_template-feature/components/__tests__/features-page.test.tsx`
- `frontend/src/features/_template-feature/hooks/__tests__/use-feature-form.test.ts`

**Existing references:**
- `frontend/src/locales/en.json` (for translation key structure)
- `frontend/src/locales/fi.json`
- `frontend/src/store/store.ts` (for Redux slice registration)
- `frontend/src/components/layout/app-layout.tsx` (for navigation)
- `frontend/src/routes/features/index.tsx` (for route pattern)

**Read the Orval-generated hooks** to understand available API functions:
- `frontend/src/api/generated/$ARGUMENTS/` (all files)

## Step 2: Generate the Feature Module

Create all files under `frontend/src/features/[feature-name]/`:

**Naming conventions (example: feature-name = "products"):**
- `components/products-page.tsx` — page layout with header and action button
- `components/products-table.tsx` — data table with skeleton loading and pagination
- `components/product-form-dialog.tsx` — create/edit dialog (dual-mode)
- `components/product-delete-dialog.tsx` — confirmation delete dialog
- `components/__tests__/products-page.test.tsx` — page component tests
- `hooks/use-product-pagination.ts` — wraps Orval list query with page state
- `hooks/use-product-form.ts` — wraps react-hook-form + Zod from Orval schema
- `hooks/index.ts` — barrel export
- `hooks/__tests__/use-product-form.test.ts` — form hook tests
- `store/products-slice.ts` — Redux UI-only state (searchQuery, selectedIds)
- `store/index.ts` — barrel export
- `index.ts` — barrel export

## Step 3: Critical Conventions (MUST FOLLOW)

- **All visible UI text** must use `t()` from `useTranslation()` — NEVER hardcode English text in JSX
- **Server state** (API data) lives in React Query via Orval hooks — NEVER duplicate into Redux
- **Redux slices** hold UI-only state: searchQuery, selectedIds, activeTab, etc.
- **Use typed hooks:** `useAppDispatch` and `useAppSelector` from `store/hooks.ts` — never raw `useDispatch`/`useSelector`
- **Import Orval hooks** from `@/api/generated/$ARGUMENTS/` — never from `api/generated/feature/`
- **Use `cn()`** from `@/lib/utils` for Tailwind class merging
- **Use `useDebounce`** from `@/hooks/use-debounce` for search input debouncing

## Step 4: Register Redux Slice

Add the new slice to `frontend/src/store/store.ts`:
- Import the reducer from the new feature's store
- Add it to the `reducer` object in `configureStore`

## Step 5: Add Route

Create a thin route file at `frontend/src/routes/[feature-name]/index.tsx`:
- Import the page component from the feature module
- Export it as the route component
- Keep the route file thin — all logic stays in the feature module

## Step 6: Add Navigation

Add a navigation link in `frontend/src/components/layout/app-layout.tsx`:
- Use `t("nav.[feature-name]")` for the link text
- Follow the pattern of existing nav links

## Step 7: Add Translations

Add translation keys to BOTH locale files:

**`frontend/src/locales/en.json`:**
- Add a top-level namespace matching the feature name (e.g., `"products": { ... }`)
- Mirror the structure used by the `"features"` namespace
- Include keys for: title, description, new[Entity], table columns, form labels, delete dialog, toast messages
- Add `"nav.[feature-name]"` key in the `"nav"` namespace

**`frontend/src/locales/fi.json`:**
- Add the same keys with actual Finnish translations (not English placeholders)
- Use the `"features"` namespace in `fi.json` as reference for translation style

## Step 8: Summary

After generating all files, provide a summary of:
- All files created
- All files modified (store, navigation, locales, routes)
- Reminder to verify the UI by running `cd frontend && npm run dev`
