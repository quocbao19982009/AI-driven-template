# AI-Driven Template — Presentation

> A production-ready fullstack template where AI and developer conventions are designed together from day one.

---

## Table of Contents

- [AI-Driven Template — Presentation](#ai-driven-template--presentation)
  - [Table of Contents](#table-of-contents)
- [Week 1 — AI in This Template](#week-1--ai-in-this-template)
  - [1. Philosophy: AI as a Structured Collaborator](#1-philosophy-ai-as-a-structured-collaborator)
  - [2. CLAUDE.md — The AI's Instruction Manual](#2-claudemd--the-ais-instruction-manual)
    - [Clarify Before You Code — The 6 Questions](#clarify-before-you-code--the-6-questions)
  - [3. Rules — Scoped Conventions](#3-rules--scoped-conventions)
    - [`backend.md` — applies to `backend/**/*.cs`](#backendmd--applies-to-backendcs)
    - [`frontend.md` — applies to `frontend/src/**/*.{ts,tsx}`](#frontendmd--applies-to-frontendsrctstsx)
  - [4. Skills — Micro-Task Agents](#4-skills--micro-task-agents)
    - [Why Micro-Tasks?](#why-micro-tasks)
  - [5. Feature Spec System](#5-feature-spec-system)
    - [Structure](#structure)
    - [The 12 Mandatory Spec Sections](#the-12-mandatory-spec-sections)
    - [Spec Sync Rule](#spec-sync-rule)
  - [6. AI Workflows End-to-End](#6-ai-workflows-end-to-end)
    - [Workflow 1: New Feature](#workflow-1-new-feature)
    - [Workflow 2: Add Field to Existing Entity](#workflow-2-add-field-to-existing-entity)
    - [Workflow 3: Fix a Bug](#workflow-3-fix-a-bug)
    - [Workflow 4: Change a Coding Convention](#workflow-4-change-a-coding-convention)
    - [Workflow 5: Code Review](#workflow-5-code-review)
  - [7. MCP Servers](#7-mcp-servers)
    - [Why Context7?](#why-context7)
    - [shadcn MCP](#shadcn-mcp)
  - [8. Settings \& Permissions](#8-settings--permissions)
    - [What settings.json controls](#what-settingsjson-controls)
    - [What settings.local.json controls](#what-settingslocaljson-controls)
    - [Design Intent](#design-intent)
  - [9. Memory System](#9-memory-system)
- [Week 2 — Architecture Decisions](#week-2--architecture-decisions)
  - [1. Tech Stack Overview](#1-tech-stack-overview)
  - [2. Backend Architecture](#2-backend-architecture)
    - [Layer Pattern](#layer-pattern)
    - [Key Infrastructure (do not reinvent)](#key-infrastructure-do-not-reinvent)
    - [Feature Registration Pattern](#feature-registration-pattern)
  - [3. Orval — Automated API Contract Sync](#3-orval--automated-api-contract-sync)
    - [The Problem it Solves](#the-problem-it-solves)
    - [How it Works](#how-it-works)
    - [Orval Configuration (`orval.config.ts`)](#orval-configuration-orvalconfigts)
    - [The Result](#the-result)
    - [The Rule](#the-rule)
  - [4. Frontend Architecture](#4-frontend-architecture)
    - [`apiFetch` — The Single HTTP Gateway](#apifetch--the-single-http-gateway)
  - [5. State Management Strategy](#5-state-management-strategy)
    - [Why not just Redux for everything?](#why-not-just-redux-for-everything)
    - [Why not just React Query for everything?](#why-not-just-react-query-for-everything)
    - [Redux Slice Pattern](#redux-slice-pattern)
  - [6. Routing — TanStack Router](#6-routing--tanstack-router)
    - [File-Based Route Pattern](#file-based-route-pattern)
  - [7. Forms \& Validation](#7-forms--validation)
    - [Why React Hook Form?](#why-react-hook-form)
    - [Why Zod?](#why-zod)
  - [8. Internationalisation](#8-internationalisation)
    - [Structure](#structure-1)
    - [The Rule](#the-rule-1)
    - [Why enforce this strictly?](#why-enforce-this-strictly)
  - [9. Styling System](#9-styling-system)
    - [Tailwind v4](#tailwind-v4)
    - [shadcn/ui](#shadcnui)
    - [`cn()` utility](#cn-utility)
  - [10. Folder Structure](#10-folder-structure)
    - [Backend (`backend/src/Backend.Api/`)](#backend-backendsrcbackendapi)
    - [Frontend (`frontend/src/`)](#frontend-frontendsrc)
  - [11. Best Practices Enforced](#11-best-practices-enforced)
    - [Security](#security)
    - [Performance](#performance)
    - [Consistency](#consistency)
    - [Maintainability](#maintainability)
  - [Summary](#summary)

---

---

# Week 1 — AI in This Template

---

## 1. Philosophy: AI as a Structured Collaborator

Most AI coding tools are ad-hoc: you paste code, the AI edits it, things break. This template takes a different approach.

**Core idea:** The AI is a team member that follows the same rules as a human developer — architecture patterns, naming conventions, security practices, and feature spec discipline.

Three principles drive this:

| Principle                         | Meaning                                                                                |
| --------------------------------- | -------------------------------------------------------------------------------------- |
| **Clarify before code**           | AI asks questions upfront before writing anything                                      |
| **Spec is the source of truth**   | Every feature has a living spec that both humans and AI read first                     |
| **Rules are explicit and scoped** | Backend rules only apply to `.cs` files; frontend rules only apply to `.ts/.tsx` files |

---

## 2. CLAUDE.md — The AI's Instruction Manual

**Location:** `.claude/CLAUDE.md`

This is the master document loaded into every AI session. It tells the AI:

- **What to ask before coding** — 6 mandatory clarification questions for new features (entity fields, relationships, business rules, endpoints, UI, authorization)
- **Where conventions live** — points to `docs/coding-style.md`, `docs/feature-generation/`
- **The feature spec workflow** — step-by-step what to do before and after every code change
- **Which MCP tools to use** — Context7 for library docs, shadcn for UI components

### Clarify Before You Code — The 6 Questions

Before writing a single line of code for a new feature, the AI must ask:

1. What fields does the entity need? Types, constraints, required/optional?
2. Does this entity relate to any other entity?
3. Any special business logic? (status transitions, computed fields, restrictions)
4. Standard CRUD only, or custom actions? (toggle, bulk delete, etc.)
5. Standard table + form dialog, or something different?
6. Public, authenticated only, or role-restricted?

> This eliminates the most common cause of rework: building the wrong thing.

---

## 3. Rules — Scoped Conventions

**Location:** `.claude/rules/`

Rules files are automatically loaded when the AI edits files matching their glob pattern. They contain non-negotiable conventions with reasons why.

### `backend.md` — applies to `backend/**/*.cs`

| Rule                                                         | Why                                                                                     |
| ------------------------------------------------------------ | --------------------------------------------------------------------------------------- |
| Use `BCrypt` for passwords, never SHA256                     | SHA256 is fast — attackers try billions/second offline. BCrypt is tunable-slow.         |
| All read-only EF queries use `.AsNoTracking()`               | EF tracks every loaded entity; tracking wastes memory and risks ghost writes            |
| Paginated queries need `.OrderBy()` before `.Skip()/.Take()` | SQL has no guaranteed order — without it you get duplicate or missing rows across pages |
| Never put secrets in `appsettings.json`                      | Git history is permanent — once committed, a secret is always accessible                |
| Always return `ApiResponse<T>` from controllers              | Consistent response shape across the entire API                                         |
| Always pass `CancellationToken` through every async method   | Allows request cancellation to propagate correctly through the stack                    |

### `frontend.md` — applies to `frontend/src/**/*.{ts,tsx}`

| Rule                                                        | Why                                                                              |
| ----------------------------------------------------------- | -------------------------------------------------------------------------------- |
| Never edit `api/generated/`                                 | Orval overwrites this folder completely on every `api:sync` run                  |
| Run `npm run api:sync` after any backend DTO change         | Frontend hooks become stale and type-unsafe otherwise                            |
| Server state in React Query only, never duplicated in Redux | Two sources of truth drift apart — stale Redux overrides fresh API data silently |
| All visible text via `t()`                                  | Hardcoded strings break internationalisation and are hard to audit               |
| Update both `en.json` AND `fi.json` always                  | Missing translation keys cause runtime fallbacks or blank UI                     |

---

## 4. Skills — Micro-Task Agents

**Location:** `.claude/skills/` and `.claude/agents/`

Skills are named commands (`/skill-name`) that trigger focused, well-defined AI tasks. Each skill does exactly one thing.

| Skill                                  | What it does                                                                                                  |
| -------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| `/create-spec [feature]`               | Asks targeted questions (max 3 per batch), creates a 12-section spec from template                            |
| `/new-feature [feature]`               | Full end-to-end: spec → backend scaffold → migration → api:sync → frontend scaffold → quality check           |
| `/scaffold-feature [feature]`          | Generates backend from spec: entity, DTOs, validator, repository, service, controller, tests                  |
| `/scaffold-feature-frontend [feature]` | Generates frontend from spec: components, hooks, Redux slice, route, nav link, translations                   |
| `/api-sync`                            | Runs Swagger export + Orval regeneration, shows changed files                                                 |
| `/add-migration [Name]`                | Creates EF Core migration, shows file for review, applies to database                                         |
| `/clarify-spec [feature]`              | Resolves all TODO/NEEDS CLARIFICATION markers interactively                                                   |
| `/quality-check`                       | Runs 7 gates: backend build, backend tests, frontend lint, TypeScript, frontend tests, translations, api sync |
| `/spec-sync`                           | Updates feature spec when code changes affect entity fields, validation, endpoints, or UI                     |
| `/pr-reviewer`                         | Senior code review: SOLID principles, project conventions; outputs critical/major/minor issue report          |

### Why Micro-Tasks?

Large "do everything" prompts produce inconsistent results. Breaking the workflow into focused skills means:

- Each step is predictable and repeatable
- Failures are isolated — a bad scaffold doesn't break the spec
- Humans can review output at each gate before proceeding

---

## 5. Feature Spec System

**Location:** `feature_docs/`

Every feature has a spec file before any code is written. The spec is the single source of truth — both humans and AI read it before making changes.

### Structure

```
feature_docs/
  FEATURES.md                          # Index of all features + status
  _templates/
    feature-spec-template.md           # 12-section canonical template
    feature-data-model-template.md     # Data model reference
  [feature-name]/
    feature-spec-[feature-name].md     # One spec per feature
```

### The 12 Mandatory Spec Sections

| #   | Section                               | Contains                                                 |
| --- | ------------------------------------- | -------------------------------------------------------- |
| 1   | Entity                                | Fields table: name, type, constraints, required/optional |
| 2   | Core Values & Principles              | What this feature optimizes for                          |
| 3   | Architecture Decisions                | Min 2 decisions with rationale                           |
| 4   | Data Flow                             | Mermaid diagram + prose walkthrough                      |
| 5   | API Endpoints                         | Table: method, route, auth requirement                   |
| 6   | Validation Rules                      | Per-field rules enforced in backend                      |
| 7   | Business Rules + Acceptance Scenarios | Edge cases and expected outcomes                         |
| 8   | Authorization                         | Who can do what                                          |
| 9   | Frontend UI Description               | Component layout and interactions                        |
| 10  | Redux UI State                        | What slice holds, initial state shape                    |
| 11  | File Locations                        | Exact paths for all backend + frontend files             |
| 12  | Tests                                 | What's written, what's pending                           |

### Spec Sync Rule

After every code change that touches entity fields, validation, endpoints, or UI behavior — the spec must be updated before the task is considered complete. The `/spec-sync` skill handles this automatically.

---

## 6. AI Workflows End-to-End

**Location:** `docs/ai-workflow.md`, `docs/WORKFLOW_VISUAL.md`

Five documented workflows with Mermaid diagrams:

### Workflow 1: New Feature

```
/create-spec → /scaffold-feature → /add-migration → /api-sync → /scaffold-feature-frontend → /quality-check
```

1. AI asks clarification questions (max 3 per batch)
2. Creates spec from template with all 12 sections
3. Scaffolds complete backend (entity → DTO → validator → repo → service → controller → tests)
4. Creates and applies EF Core migration
5. Runs `npm run api:sync` to regenerate typed frontend hooks
6. Scaffolds complete frontend (components → hooks → Redux slice → route → translations)
7. Runs 7-gate quality check

### Workflow 2: Add Field to Existing Entity

```
Update spec → Update backend → /add-migration → /api-sync → Update frontend
```

Spec comes first, always. Code follows spec, never the reverse.

### Workflow 3: Fix a Bug

```
/fix-issue → AI reads spec + code → implements fix → runs tests → updates spec if behavior changed
```

### Workflow 4: Change a Coding Convention

```
Edit docs/coding-style.md → Commit → AI follows new rule from next session
```

Conventions are code — they live in version control.

### Workflow 5: Code Review

```
/pr-reviewer → Reviews SOLID + all project conventions → Outputs critical/major/minor report
```

---

## 7. MCP Servers

**Location:** `.mcp.json`

MCP (Model Context Protocol) servers give the AI access to external tools without leaving the session.

| Server     | Purpose                                                 | When used                                                                   |
| ---------- | ------------------------------------------------------- | --------------------------------------------------------------------------- |
| `context7` | Live library documentation and code examples            | Anytime the AI needs to know how a library API works — no hallucinated docs |
| `shadcn`   | shadcn/ui component CLI — add, search, audit components | Frontend scaffolding, adding new UI primitives                              |

### Why Context7?

Standard AI models have a knowledge cutoff and can hallucinate library APIs. With Context7, the AI fetches current documentation at query time — the code it generates matches the actual installed version.

### shadcn MCP

Instead of the AI guessing shadcn component names or install commands, the MCP can:

- Search available components
- Return the exact `npx shadcn add [component]` command
- View component source and examples from registries

---

## 8. Settings & Permissions

**Locations:** `.claude/settings.json` (shared), `.claude/settings.local.json` (developer-local)

### What settings.json controls

- **Permission whitelist** — specific bash commands the AI can run without prompting (api:sync, build, lint, test, migrations, git status/diff/log)
- **Block list** — AI cannot read `.env*` files
- **Pre-commit hooks** — blocks AI edits to `api/generated/`; reminds about spec updates after feature code changes
- **MCP servers** — which servers are enabled per project

### What settings.local.json controls

- Extended permissions for the current developer's machine
- Additional tools: WebSearch, WebFetch, mkdir, npm install
- Useful for setup and experimentation without affecting the shared team config

### Design Intent

The shared settings define the safe minimum for all developers. Local settings allow individual flexibility without committing risky permissions to the repo.

---

## 9. Memory System

**Location:** `.claude/projects/[project-id]/memory/MEMORY.md`

The AI maintains a persistent memory file across sessions. It stores:

- Stable patterns confirmed across multiple interactions (e.g., `cn()` is in `lib/utils.ts`, not `lib/cn.ts`)
- Key architectural decisions and important file paths
- Workflow rules (e.g., always `TaskStop` background servers after api:sync)
- Solutions to recurring problems

This means the AI doesn't re-discover the same things every session — it builds context over time like a developer who has worked on the project before.

---

---

# Week 2 — Architecture Decisions

---

## 1. Tech Stack Overview

| Layer           | Technology                     | Version     | Why chosen                                                        |
| --------------- | ------------------------------ | ----------- | ----------------------------------------------------------------- |
| Backend runtime | .NET / ASP.NET Core            | 10          | LTS, minimal APIs, excellent EF Core integration                  |
| Database        | PostgreSQL + EF Core           | 10          | Reliable RDBMS; EF handles migrations and query generation        |
| ORM             | Entity Framework Core          | 10          | Code-first migrations, LINQ queries, no raw SQL needed for CRUD   |
| Auth            | JWT Bearer                     | 10          | Stateless, scalable, standard for SPAs                            |
| API docs        | Swashbuckle (Swagger)          | 10.1        | OpenAPI spec output used by Orval for frontend code gen           |
| Validation      | FluentValidation               | 11.11       | Declarative, testable, separates validation from controllers      |
| Logging         | Serilog                        | 10          | Structured logging with sink flexibility                          |
| Frontend        | React + TypeScript             | 19 / 5.9    | Component model, type safety, ecosystem                           |
| Build tool      | Vite                           | 7.3         | Fast HMR, modern ESM bundling                                     |
| Routing         | TanStack Router                | 1.159       | Type-safe, file-based, code-splitting built in                    |
| Server state    | TanStack Query (React Query)   | 5.90        | Cache management, background refetch, stale-while-revalidate      |
| UI state        | Redux Toolkit                  | 2.11        | Predictable, DevTools support, minimal boilerplate via RTK        |
| Forms           | React Hook Form + Zod          | 7.71 / 4.3  | Uncontrolled inputs = performance; Zod = shared schema validation |
| API codegen     | Orval                          | 8.3         | Generates typed React Query hooks from OpenAPI spec automatically |
| UI components   | shadcn/ui + Radix UI           | 3.8 / 1.4   | Accessible primitives, fully owned source, no runtime overhead    |
| Styling         | Tailwind CSS v4                | 4.1         | Utility-first, no CSS files to maintain, tree-shaken output       |
| i18n            | i18next + react-i18next        | 25.8 / 16.5 | Industry standard, JSON-based, hook-first API                     |
| Icons           | Lucide React                   | 0.564       | Consistent, tree-shakeable SVG icons                              |
| Notifications   | Sonner                         | 2.0         | Minimal toast library, Tailwind-compatible                        |
| Testing (BE)    | xUnit + Moq + FluentAssertions | —           | Standard .NET testing trio                                        |
| Testing (FE)    | Vitest + Testing Library       | 4.0 / 16.3  | Vite-native test runner, same config as app                       |

---

## 2. Backend Architecture

### Layer Pattern

```
HTTP Request
    ↓
Controller      (thin HTTP handler — validate auth, call service, return ApiResponse<T>)
    ↓
Service         (business logic, orchestration, calls repository)
    ↓
Repository      (EF Core queries — AsNoTracking for reads, tracked for writes)
    ↓
DbContext       (EF Core — PostgreSQL)
```

**Why this separation?**

- Controllers stay thin and testable — no business logic bleeds in
- Services own business rules — swappable without touching HTTP layer
- Repositories own data access — swap EF for Dapper without touching business logic

### Key Infrastructure (do not reinvent)

| Type                  | Location                                   | Purpose                                                                                 |
| --------------------- | ------------------------------------------ | --------------------------------------------------------------------------------------- |
| `BaseEntity`          | `Common/Models/BaseEntity.cs`              | All entities inherit this — provides `Id` (int), `CreatedAt`, `UpdatedAt` automatically |
| `ApiResponse<T>`      | `Common/Models/ApiResponse.cs`             | Every controller response wrapped here — `.Ok(data)` or `.Fail(message)`                |
| `PagedResult<T>`      | `Common/Models/PagedResult.cs`             | Paginated list: `Items`, `TotalCount`, `Page`, `PageSize`, computed `TotalPages`        |
| `NotFoundException`   | `Common/Exceptions/NotFoundException.cs`   | `throw new NotFoundException("Product", id)` → auto-mapped to 404                       |
| `ValidationException` | `Common/Exceptions/ValidationException.cs` | `throw new ValidationException(errors)` → auto-mapped to 400 with error list            |

### Feature Registration Pattern

Every feature requires two DI registrations in `Program.cs`:

```csharp
builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
builder.Services.AddScoped<IProductsService, ProductsService>();
```

This is enforced by the backend rule and checked during `/quality-check`.

---

## 3. Orval — Automated API Contract Sync

This is the most important automation in the frontend workflow.

### The Problem it Solves

Without Orval:

- Backend changes a DTO → frontend developer manually updates API call types
- Easy to miss fields, get types wrong, forget to update all call sites
- Frontend and backend drift apart silently

### How it Works

```
Backend changes endpoint/DTO
        ↓
npm run api:sync
        ↓
dotnet swagger tofile → generates backend/swagger.json (OpenAPI spec)
        ↓
Orval reads swagger.json
        ↓
Generates frontend/src/api/generated/**
  ├── [feature]/getFeatures.ts      (typed useQuery hook)
  ├── [feature]/postFeatures.ts     (typed useMutation hook)
  └── [feature]/schemas.ts          (Zod schemas matching backend DTOs)
```

### Orval Configuration (`orval.config.ts`)

```typescript
input: "../backend/swagger.json"
output: {
  mode: "tags-split",        // one folder per API tag (feature)
  client: "react-query",     // generates TanStack Query hooks
  mutator: "./src/api/mutator/apiFetch.ts",  // custom fetch wrapper
  override: {
    query: { useQuery: true, useMutation: true }
  }
}
```

### The Result

Frontend developers call a generated hook that is:

- **Fully typed** — request params and response match the backend DTO exactly
- **Auto-cached** — React Query handles stale-while-revalidate
- **Auto-invalidated** — mutations trigger refetch of related queries

```typescript
// This entire hook was generated — no manual code
const { data, isLoading } = useGetProducts({ page: 1, pageSize: 20 });
```

### The Rule

`api/generated/` is **never manually edited**. The pre-commit hook blocks it. Every change to that folder must come from `npm run api:sync`.

---

## 4. Frontend Architecture

### `apiFetch` — The Single HTTP Gateway

**Location:** `frontend/src/api/mutator/apiFetch.ts`

All HTTP calls go through one function. Orval uses it as the mutator for every generated hook.

```typescript
const BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5054";

export class ApiError extends Error {
  status: number;
  errors: string[];
}

export const apiFetch = async <T>(url: string, options?: RequestInit) => {
  // Handles: auth headers, response.ok check, JSON parsing, ApiError mapping
  // Returns: { data: T, status: number, headers: Headers }
};
```

**Why centralize this?**

- Auth token injection happens in one place — add a header here, every request gets it
- Error handling is consistent — `ApiError` has the same shape everywhere
- Easy to add request interceptors (e.g., refresh token) without touching generated code

---

## 5. State Management Strategy

A deliberate separation of concerns between two state systems:

| Concern                     | Technology                    | Examples                                            |
| --------------------------- | ----------------------------- | --------------------------------------------------- |
| **Server state** (API data) | React Query (via Orval hooks) | product list, user profile, booking data            |
| **UI state** (interaction)  | Redux Toolkit slices          | search query, selected IDs, active tab, open panels |
| **Local ephemeral state**   | React `useState`              | dialog open/closed, hover state                     |

### Why not just Redux for everything?

If you put API data in Redux:

- Redux holds stale data while the server has updated data
- You need to manually invalidate, refetch, and sync
- Components read cached Redux instead of fresh API data — silent bugs

React Query handles all of this automatically: cache TTL, background refetch, stale-while-revalidate, optimistic updates.

### Why not just React Query for everything?

UI state (e.g., "which rows are selected in the table") is not server data — it lives only in the browser. Putting it in React Query would mean fake "queries" with no real endpoint. Redux is the right tool for this.

### Redux Slice Pattern

```typescript
// features/products/store/productsSlice.ts
const productsSlice = createSlice({
  name: "products",
  initialState: {
    searchQuery: "",
    selectedIds: [] as number[],
    activeTab: "all",
  },
  reducers: {
    setSearchQuery: (state, action) => {
      state.searchQuery = action.payload;
    },
    toggleSelected: (state, action) => {
      /* ... */
    },
  },
});
```

---

## 6. Routing — TanStack Router

**Why TanStack Router over React Router?**

| Feature            | TanStack Router                                        | React Router v6     |
| ------------------ | ------------------------------------------------------ | ------------------- |
| Type safety        | Full end-to-end type safety on params, search, loaders | Partial             |
| File-based routing | Built-in                                               | Needs manual config |
| Search params      | First-class, typed, validated                          | Manual parsing      |
| Code splitting     | Automatic per-route                                    | Manual `lazy()`     |
| Loaders            | Integrated with React Query                            | Separate            |

### File-Based Route Pattern

```
routes/
  __root.tsx          → root layout (nav, providers)
  index.tsx           → home page (/)
  products/
    index.tsx         → /products
    $productId.tsx    → /products/123
```

Each route file is a thin wrapper — it imports the feature page component and renders it:

```typescript
// routes/products/index.tsx
export const Route = createFileRoute("/products")({
  component: ProductsPage,
});
```

---

## 7. Forms & Validation

**Stack:** React Hook Form + Zod + `@hookform/resolvers`

### Why React Hook Form?

- Uncontrolled inputs — no re-render on every keystroke (performance)
- Built-in dirty/touched tracking
- Integrates with any UI library (shadcn inputs, Radix, etc.)

### Why Zod?

- TypeScript-first — schema infers the type, no duplication
- Same validation library used on both frontend (form validation) and can be referenced for backend shape expectations
- Composable: `.optional()`, `.refine()`, `.transform()`

```typescript
const productSchema = z.object({
  name: z.string().min(1, "Name is required").max(100),
  price: z.number().positive("Price must be positive"),
  categoryId: z.number().int(),
});

type ProductFormValues = z.infer<typeof productSchema>;

const form = useForm<ProductFormValues>({
  resolver: zodResolver(productSchema),
});
```

---

## 8. Internationalisation

**Stack:** i18next + react-i18next

### Structure

```
locales/
  en.json    → English (primary)
  fi.json    → Finnish (mirrored structure)
```

### The Rule

Every visible string goes through `t()`. No hardcoded text anywhere in components. Both locale files must be updated together.

```typescript
const { t } = useTranslation();
return <h1>{t("products.title")}</h1>;
```

### Why enforce this strictly?

Missing translation keys cause either runtime errors or blank UI depending on i18next config. Enforcing it from the start (via AI rules + quality-check) means the codebase is always i18n-ready.

---

## 9. Styling System

**Stack:** Tailwind CSS v4 + shadcn/ui + Radix UI + `cn()` utility

### Tailwind v4

New in v4: CSS-first configuration, no `tailwind.config.js` required. Works via Vite plugin:

```typescript
// vite.config.ts
import tailwindcss from "@tailwindcss/vite";
plugins: [tailwindcss()];
```

### shadcn/ui

Not a component library — it's a code generator. Components are copied into `components/ui/` and fully owned by the project. No runtime dependency, no version lock-in.

```bash
npx shadcn add button dialog table  # components become your source code
```

shadcn is also integrated as an MCP server — the AI can add components without leaving the session.

### `cn()` utility

```typescript
// lib/utils.ts
import { clsx } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}
```

Used for conditional Tailwind classes without conflicts:

```typescript
<div className={cn("base-class", isActive && "active-class", className)} />
```

---

## 10. Folder Structure

### Backend (`backend/src/Backend.Api/`)

```
Features/
  Users/                     → auth + user management
  _FeatureTemplate/          → scaffold example (copy this for new features)
  [FeatureName]/             → flat structure, no Dtos/ or Validators/ subfolders
    [Feature]Entity.cs
    [Feature]Dtos.cs
    [Feature]Validator.cs
    I[Feature]Repository.cs
    [Feature]Repository.cs
    I[Feature]Service.cs
    [Feature]Service.cs
    [Feature]Controller.cs
Common/
  Models/                    → BaseEntity, ApiResponse, PagedResult
  Exceptions/                → NotFoundException, ValidationException
  Middleware/
  Extensions/
  Swagger/
Data/
  ApplicationDbContext.cs
  Migrations/
  DataSeeder.cs
Identity/
  JwtService.cs
```

### Frontend (`frontend/src/`)

```
api/
  generated/                 → Orval output (never edit manually)
  mutator/
    apiFetch.ts              → single HTTP gateway
components/
  ui/                        → shadcn primitives (Button, Dialog, Table, etc.)
  layout/
    app-layout.tsx
features/
  _template-feature/         → scaffold example (copy this for new features)
  [feature-name]/
    components/              → page + form dialog + delete dialog
    hooks/                   → usePagination, useFilters, etc.
    store/                   → Redux slice (UI state only)
    index.ts                 → barrel export
hooks/
  use-debounce.ts
  use-media-query.ts
lib/
  utils.ts                   → cn() utility
locales/
  en.json
  fi.json
providers/                   → QueryClientProvider, ReduxProvider, i18n setup
routes/
  __root.tsx
  [feature-name]/
    index.tsx                → thin route wrapper
store/
  store.ts                   → Redux configureStore
  hooks.ts                   → useAppDispatch, useAppSelector
```

---

## 11. Best Practices Enforced

### Security

- Passwords hashed with **BCrypt** (never SHA256) — tunable cost factor makes brute-force infeasible
- Secrets in **environment variables** only — never in `appsettings.json`
- JWT Bearer auth on all protected endpoints
- CORS policy configured per environment

### Performance

- `.AsNoTracking()` on all read-only EF queries — skips change tracking overhead
- React Query cache prevents redundant API calls
- Uncontrolled form inputs (React Hook Form) — no re-render per keystroke
- Orval code-splits API hooks by feature tag automatically
- TanStack Router auto-code-splits by route

### Consistency

- `ApiResponse<T>` wrapper on every endpoint — predictable shape across the entire API
- `CancellationToken` threaded through every async method — request cancellation propagates correctly
- `.OrderBy()` before `.Skip()/.Take()` on every paginated query — deterministic pagination
- `t()` for every visible string — always i18n-ready
- Typed Redux hooks (`useAppDispatch`, `useAppSelector`) — no accidental `any` in state access

### Maintainability

- Feature specs as single source of truth — new developers read the spec, not the code
- Generated API hooks — backend and frontend stay in sync automatically
- Flat feature file structure — no deep nesting, easy to navigate
- Coding style in version control — conventions change via PR, not verbal agreement

---

## Summary

This template is two things at once:

**1. An AI Workflow System**

The AI is not a code autocomplete — it's a structured workflow participant. It follows explicit rules, reads specs before coding, asks questions before building, and keeps documentation in sync. The workflow is designed to minimize hallucination and maximize human control at every step.

**2. A Modern Fullstack Foundation**

Every architectural decision has a documented reason. The stack choices (Orval, TanStack Router/Query, shadcn, Redux Toolkit, FluentValidation) are not trend-following — each solves a specific problem that simpler alternatives leave open.

The two goals reinforce each other: a consistent, well-documented codebase is also easier for AI to understand and extend correctly.
