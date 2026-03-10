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

## Customization

### Color Scheme & Theme

Colors are defined as CSS variables in `src/index.css`. The project uses the shadcn/ui token system with Tailwind CSS v4 and `oklch` color values.

**To change the color scheme**, edit the `:root` (light) and `.dark` (dark mode) blocks in `src/index.css`:

```css
:root {
  --primary: oklch(0.21 0.006 285.885); /* main brand color */
  --primary-foreground: oklch(0.985 0 0); /* text on primary */
  --background: oklch(1 0 0); /* page background */
  --foreground: oklch(0.141 0.005 285.823); /* default text */
  /* ... */
}
```

The easiest way to generate a new palette is the [shadcn themes tool](https://ui.shadcn.com/themes) — paste the generated CSS variables directly into `src/index.css`.

**To change border radius**, edit the `--radius` variable:

```css
:root {
  --radius: 0.625rem; /* increase for rounder, decrease for sharper */
}
```

All radius variants (`--radius-sm`, `--radius-lg`, etc.) are derived from this single value.

---

### Adding shadcn Components

```bash
npx shadcn add <component>   # run from frontend/
```

Components are added to `src/components/ui/`. Browse available components at [ui.shadcn.com/docs/components](https://ui.shadcn.com/docs/components).

---

### Tailwind CSS

This project uses **Tailwind CSS v4** with CSS-first configuration — there is no `tailwind.config.js`. All customization is done in `src/index.css` under `@theme inline`:

```css
@theme inline {
  /* add custom design tokens here */
  --color-brand: oklch(0.55 0.2 250);
  --font-display: "Inter", sans-serif;
}
```

Custom tokens added here are automatically available as Tailwind utility classes (e.g., `text-brand`, `font-display`).

---

### Prettier

Formatting rules are in `frontend/.prettierrc`:

```json
{
  "singleQuote": false,
  "semi": true,
  "tabWidth": 2,
  "trailingComma": "es5"
}
```

Files ignored by Prettier are listed in `frontend/.prettierignore` (Orval output and TanStack Router generated files are excluded by default).

Run formatting:

```bash
npm run format        # fix files in place
npm run format:check  # check only (CI)
```

---

### ESLint

Rules are in `frontend/eslint.config.js`. The config includes:

- `typescript-eslint` — TypeScript-aware rules
- `eslint-plugin-react-hooks` — hooks rules
- `eslint-plugin-react-refresh` — HMR safety
- `@tanstack/eslint-plugin-query` — React Query best practices
- `eslint-config-prettier` — disables rules that conflict with Prettier

To add a custom rule, add it to the main `rules` block:

```js
{
  files: ["**/*.{ts,tsx}"],
  rules: {
    "no-console": "warn",
  },
}
```

---

### Translations / Locales

Supported languages are defined in `src/locales/`. Currently: `en.json` (English) and `fi.json` (Finnish).

**To add a new language:**

1. Create `src/locales/<locale>.json` with the same keys as `en.json`
2. Import and register it in `src/providers/i18n.ts`

**To change the default language**, update the `lng` option in the i18n provider.

---

## Adding a New Feature

The full workflow is AI-driven. From the repo root:

1. `/scaffold-feature-frontend <feature-name>` — after `api:sync` has been run
2. `/add-translations <feature-name>` — adds keys to both `en.json` and `fi.json`

See the root `README.md` and `docs/ai-workflow.md` for the complete workflow.
