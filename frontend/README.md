# Frontend

React + TypeScript + Vite frontend for the AI-Driven Full-Stack Template.

## Stack

- **React 19** with React Compiler enabled
- **TypeScript**
- **Vite** — dev server and build tool
- **TanStack Router** — file-based routing
- **React Query** — server state (API data)
- **Redux Toolkit** — UI-only state (search, selected IDs, open panels)
- **shadcn/ui** — component library (Radix UI + Tailwind CSS)
- **react-i18next** — translations (English + Finnish)
- **Orval** — auto-generated React Query hooks from the backend OpenAPI spec

## Getting Started

**1. Create the env file**

Create `frontend/.env` with:

```
VITE_API_URL=http://localhost:5054
```

**2. Install dependencies**

```bash
npm install
```

**3. Run the dev server**

```bash
npm run dev
```

App runs at http://localhost:5173. The backend must be running at `VITE_API_URL` for API calls to work.

## Key Commands

| Command             | What it does                  |
| ------------------- | ----------------------------- |
| `npm run dev`       | Start dev server              |
| `npm run build`     | TypeScript check + Vite build |
| `npm run lint`      | ESLint                        |
| `npm run test`      | Vitest unit tests             |
| `npm run typecheck` | TypeScript type check only    |

> `api:sync` is run from the **repo root**, not from `frontend/`. See root README.

## Project Structure

```
src/
├── api/
│   ├── generated/          # Orval output — NEVER edit manually
│   └── mutator/apiFetch.ts # Base HTTP client — all requests go through here
├── components/
│   ├── layout/             # App shell and navigation
│   └── ui/                 # shadcn/ui components
├── features/
│   └── _template-feature/  # Template — AI copies this when scaffolding
├── hooks/                  # Shared hooks (useDebounce, etc.)
├── locales/                # en.json, fi.json — all UI strings
├── providers/              # React context providers (Query, Store, i18n)
├── routes/                 # TanStack Router file-based routes
├── store/                  # Redux root store + typed hooks
└── types/                  # Shared TypeScript types
```

## Important Conventions

- **Never edit `api/generated/`** — overwritten by `npm run api:sync` on every backend change
- **All UI strings use `t()`** from `useTranslation()` — never hardcode visible text
- **Server state lives in React Query** — never duplicate API data into Redux
- **Redux slices hold UI-only state** — search query, selected IDs, active tab, open panels
- **Always import DTO types from `@/api/generated/models`** — never redefine them locally
- **Add shadcn components** with `npx shadcn add [component]` (run from `frontend/`)

## Adding a New Feature

The full workflow is AI-driven. From the repo root:

1. `/scaffold-feature-frontend <feature-name>` — after `api:sync` has been run
2. `/add-translations <feature-name>` — adds keys to both `en.json` and `fi.json`

See the root `README.md` and `docs/ai-workflow.md` for the complete workflow.
