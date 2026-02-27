---
globs: frontend/src/**/*.{ts,tsx}
---

# Frontend Critical Conventions (DO NOT DEVIATE)

- Never install new npm packages without checking if an existing library already covers it
- Never edit files under `api/generated/` — they are overwritten by Orval on every `api:sync`
  > Why: Orval regenerates the entire `api/generated/` folder from the backend OpenAPI spec on every `npm run api:sync`. Any manual edits are silently deleted. Customizations belong in `api/mutator/apiFetch.ts` or wrapper hooks, not in generated files.
- After any backend DTO or endpoint change, run `npm run api:sync` from the repo root before touching the frontend
- Server state (API data) lives in React Query — never duplicate it into Redux
  > Why: Duplicating server state into Redux creates two sources of truth that drift apart. Stale Redux state silently overrides fresh API data — components read cached Redux values instead of the latest server response, causing bugs that are hard to reproduce.
- Redux slices hold UI-only state only: search query, selected IDs, active tab, open panels
- Never hardcode visible UI text — all strings must go through `t()` from `useTranslation()` (react-i18next)
- When adding translation keys, always update BOTH `src/locales/en.json` AND `src/locales/fi.json`

---

## Key Infrastructure — use these, do not reinvent

| Type / Hook                         | Location                  | Purpose                                                                                |
| ----------------------------------- | ------------------------- | -------------------------------------------------------------------------------------- |
| `useAppDispatch` / `useAppSelector` | `store/hooks.ts`          | Typed Redux hooks — always use these, never raw `useDispatch`/`useSelector`            |
| `apiFetch`                          | `api/mutator/apiFetch.ts` | All HTTP calls go through this — Orval uses it automatically                           |
| `cn()`                              | `lib/utils.ts`            | Tailwind class merging utility                                                         |
| `useDebounce`                       | `hooks/use-debounce.ts`   | Debounce input values before triggering queries                                        |
| `useTranslation`                    | `react-i18next`           | All UI strings must use `const { t } = useTranslation()` — never hardcode visible text |
