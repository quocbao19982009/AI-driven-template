# AI-Driven Full-Stack Template

A production-ready starter that lets an AI agent scaffold complete features — backend, frontend, migrations, translations, and tests — from a single spec file.

- **Backend:** ASP.NET Core 10, Entity Framework Core, PostgreSQL
- **Frontend:** React + TypeScript, Vite, Redux Toolkit, React Query, shadcn/ui
- **AI tooling:** Claude Code CLI with purpose-built skills and agents for this stack

---

## How This Template Works

1. Write a feature spec (`/create-spec products`)
2. AI scaffolds the complete backend from the spec (`/scaffold-feature products`)
3. Sync the API contract to the frontend (`npm run api:sync` from repo root)
4. AI scaffolds the complete frontend from generated hooks (`/scaffold-feature-frontend products`)

The bridge between backend and frontend is Swagger → Orval: after every backend change, `api:sync` exports the OpenAPI spec and regenerates TypeScript query hooks under `frontend/src/api/generated/`. Those hooks are never written by hand.

See [`docs/WORKFLOW_VISUAL.md`](docs/WORKFLOW_VISUAL.md) for diagrams.

---

## Prerequisites

- .NET 10 SDK
- Node.js 20+
- Docker (recommended) — or a local PostgreSQL installation
- Claude Code CLI: `npm install -g @anthropic-ai/claude-code`

---

## Day 1 Setup

**1. Clone and install root dependencies**
```bash
git clone <repo-url>
cd <repo>
npm install
```

**2. Start the database**
```bash
docker compose -f docker/docker-compose.yml up -d
```
PostgreSQL will be available on port `5433`. pgAdmin is available at http://localhost:5050 (email: `admin@admin.com`, password: `admin`).

**3. Set the JWT secret key**

Open `backend/src/Backend.Api/appsettings.json` and find:
```json
"Jwt": {
  "Key": "CHANGE-THIS-TO-A-SECURE-KEY-AT-LEAST-32-CHARS!"
}
```
Replace the placeholder with a real secret before running. In production, use environment variables or `dotnet user-secrets` — never commit real secrets to source control.

**4. Set up the frontend environment**
```bash
cp frontend/.env.example frontend/.env
```
The default `VITE_API_URL=http://localhost:5054` matches the backend's default port.

**5. Restore, migrate, and run the backend**
```bash
cd backend/src/Backend.Api
dotnet restore
dotnet tool restore
dotnet ef database update
dotnet run
```
The API will be running at http://localhost:5054. Swagger UI is at http://localhost:5054/swagger.

**6. Install and run the frontend**
```bash
cd frontend
npm install
npm run dev
```
The app will be available at http://localhost:5173.

**7. Verify the API sync bridge**
```bash
# Run from the repo root, not from frontend/
npm run api:sync
```
This should complete without errors. If it does, the Swagger → Orval pipeline is working correctly.

---

## AI Skills and Agents

This is where the template's value lives. Use these instead of writing boilerplate.

### Skills — invoke with `/skill-name`

| Skill | When to use | What it does |
|---|---|---|
| `/create-spec products` | Start of any new feature | Creates `feature_docs/feature-spec-products.md` from the spec template |
| `/scaffold-feature products` | After the spec is filled in | Generates complete backend: entity, DTOs, validator, repository, service, controller, tests |
| `/scaffold-feature-frontend products` | After `api:sync` completes | Generates complete frontend: page, table, dialogs, hooks, Redux slice, route, translations |
| `/api-sync` | After any backend change | Runs Swagger export + Orval regeneration (same as `npm run api:sync`) |
| `/add-migration AddProductEntity` | After adding a backend entity or field | Creates, reviews, and applies an EF Core migration |
| `/add-translations products` | After scaffolding the frontend | Adds all translation keys to both `en.json` and `fi.json` |
| `/fix-issue "description"` | When something breaks | Diagnoses the problem, fixes it, and updates the spec if behavior changed |
| `/quality-check` | Before any PR | Runs build, tests, lint, TypeScript, translations check, and api sync verification |

### Agents — invoke by describing the task

| What to say | Agent | What it does |
|---|---|---|
| "Review my code on [branch]" | `pr-reviewer` | Checks SOLID principles, all CLAUDE.md conventions, and coding style; returns a severity-rated report |
| "Sync the spec after my changes" | `spec-sync` | Keeps `feature_docs/` in sync with code changes |
| "Write tests for [class]" | `unit-tester` | Writes tests using Equivalence Partitioning; handles both .NET (xUnit) and React (Vitest) |

Full step-by-step walkthroughs: [`docs/ai-workflow.md`](docs/ai-workflow.md)
Diagrams: [`docs/WORKFLOW_VISUAL.md`](docs/WORKFLOW_VISUAL.md)

---

## The Golden Rule

> **Backend first → `npm run api:sync` → Frontend.**

The frontend consumes Orval-generated hooks that only exist after the sync runs. Trying to build the frontend before syncing is the most common Day 1 mistake — the hooks simply won't be there.

---

## Repository Map

```
.
├── .claude/                         # AI config — CLAUDE.md, skills/, agents/
├── backend/src/Backend.Api/
│   ├── Features/                    # One folder per feature
│   │   └── _FeatureTemplate/        # AI reads this when scaffolding backend
│   └── Common/                      # BaseEntity, ApiResponse<T>, exceptions
├── frontend/src/
│   ├── api/generated/               # Orval output — NEVER edit manually
│   ├── features/                    # One folder per feature
│   │   └── _template-feature/       # AI reads this when scaffolding frontend
│   ├── locales/                     # en.json, fi.json — all UI strings
│   ├── store/                       # Redux root + typed hooks
│   └── components/layout/           # Navigation — add links here for new features
├── feature_docs/                    # Feature specs (one file per feature)
│   └── FEATURES.md                  # Index — AI reads this first on every request
├── docs/                            # All documentation
└── docker/docker-compose.yml        # PostgreSQL on :5433, pgAdmin on :5050
```

---

## Customizing This Template

| What to change | Where |
|---|---|
| Database connection | `backend/src/Backend.Api/appsettings.json` → `ConnectionStrings.DefaultConnection` |
| JWT secret key | Same file → `Jwt.Key` (use env vars or user-secrets in production) |
| Frontend API URL | `frontend/.env` → `VITE_API_URL` |
| CORS origins | `appsettings.json` → `Cors.AllowedOrigins` |
| UI text (English) | `frontend/src/locales/en.json` |
| Add or remove a language | Add a locale file, update `frontend/src/providers/` |
| Coding style rules AI enforces | `docs/coding-style.md` — edit, commit; AI follows the updated rules next session |
| Navigation links | `frontend/src/components/layout/app-layout.tsx` |
| Redux slices | `frontend/src/store/store.ts` |
| Add a shadcn/ui component | `npx shadcn add [component]` (run from `frontend/`) |
| AI project-wide conventions | `.claude/CLAUDE.md` |

---

## Documentation Index

| File | What's in it |
|---|---|
| [`docs/ai-workflow.md`](docs/ai-workflow.md) | Full step-by-step AI workflow walkthroughs |
| [`docs/WORKFLOW_VISUAL.md`](docs/WORKFLOW_VISUAL.md) | Architecture and workflow diagrams |
| [`docs/coding-style.md`](docs/coding-style.md) | Naming, formatting, Tailwind, and testing style rules enforced by AI |
| [`docs/feature-generation/backend.md`](docs/feature-generation/backend.md) | Backend scaffolding steps, layer pattern, exception handling |
| [`docs/feature-generation/frontend.md`](docs/feature-generation/frontend.md) | Frontend scaffolding steps, file tree, file roles |
| [`docs/feature-generation/implementation-checklist.md`](docs/feature-generation/implementation-checklist.md) | Full implementation checklist for every feature |
| [`feature_docs/FEATURES.md`](feature_docs/FEATURES.md) | Feature index — AI reads this first on every request |
| [`feature_docs/feature-spec-template.md`](feature_docs/feature-spec-template.md) | Blank spec template (copied by `/create-spec`) |
| [`.claude/CLAUDE.md`](.claude/CLAUDE.md) | AI-facing project conventions (critical — do not delete) |
