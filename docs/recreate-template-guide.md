# How to Recreate This AI-Driven Development Template

A step-by-step guide for building a similar AI-driven development template for **any tech stack** — web, mobile (iOS/Android/React Native/Flutter), desktop, or any other platform.

This guide is written for developers who want to replicate the same AI-assisted workflow: spec-first development, automated scaffolding, code generation, and quality gates — adapted to their own technology choices.

---

## What You Are Recreating

This template is not just boilerplate code. It is a **system of conventions, AI instructions, and automation** that allows an AI coding assistant to:

1. Understand your project's architecture and rules
2. Ask the right clarifying questions before writing code
3. Generate full-stack features from a specification file
4. Keep documentation in sync with code
5. Review code against your conventions
6. Run quality checks automatically

The code (ASP.NET Core + React) is interchangeable — the AI workflow on top is what you are recreating.

---

## Prerequisites

- An AI coding assistant that supports instruction files (e.g., GitHub Copilot with `.github/copilot-instructions.md`, Claude with `.claude/CLAUDE.md`, Cursor with `.cursorrules`)
- A code editor that supports skills/agents (VS Code with Copilot, Claude Code, Cursor, Windsurf)
- Basic understanding of your chosen tech stack

### Recommended Resources

- **[spec-kit](https://github.com/github/spec-kit)** (76.4k stars) — GitHub's official toolkit for writing AI instruction files. Great starting point for structuring your `.github/copilot-instructions.md` and understanding what makes good AI instructions.
- **[GitHub Copilot Best Practices](https://docs.github.com/en/copilot/get-started/best-practices)** — GitHub's official guide for getting the most out of Copilot: writing effective prompts, using context well, and building good AI-assisted workflows.
- **[GitHub Copilot CLI](https://docs.github.com/copilot/how-tos/copilot-cli)** — Use Copilot directly in your terminal to explain commands, suggest shell commands, and run AI-assisted workflows without leaving the CLI.
- **[Claude Code Best Practices](https://docs.anthropic.com/en/docs/claude-code/best-practices)** — Anthropic's official guide for writing effective `CLAUDE.md` files, structuring rules, and getting the most out of Claude Code.
- **[skills.sh](https://skills.sh)** — A community registry of reusable AI skills you can install directly into your project. Browse existing skills before writing your own from scratch.

### Recommended VS Code Extensions

| Extension                        | ID                                      | Purpose                                          |
| -------------------------------- | --------------------------------------- | ------------------------------------------------ |
| GitHub Copilot                   | `github.copilot-chat`                   | AI code completion + chat                        |
| Claude Code                      | `anthropic.claude-code`                 | Alternative AI coding assistant                  |
| Markdown Preview Mermaid Support | `bierner.markdown-mermaid`              | Render Mermaid diagrams in spec files            |
| ESLint                           | `dbaeumer.vscode-eslint`                | Enforce frontend code quality                    |
| Prettier                         | `esbenp.prettier-vscode`                | Auto-format code on save                         |
| EditorConfig                     | `editorconfig.editorconfig`             | Consistent formatting across editors             |
| Code Spell Checker               | `streetsidesoftware.code-spell-checker` | Catch typos in code and comments                 |
| DotENV                           | `mikestead.dotenv`                      | Syntax highlighting for `.env` files             |
| i18n Ally                        | `lokalise.i18n-ally`                    | Manage and preview translation keys inline       |
| Color Highlight                  | `naumovs.color-highlight`               | Highlight color values (hex, rgb, etc.) in files |

---

## Step 1: Choose Your Tech Stack

Before anything else, decide on your stack. Every later step depends on these choices.

### 1.1 — Pick a Backend

| Option                | Language    | Best for                                  |
| --------------------- | ----------- | ----------------------------------------- |
| ASP.NET Core Web API  | C#          | Enterprise, strongly typed, OpenAPI-first |
| Express / Fastify     | TypeScript  | JS-native teams, fast setup               |
| Django REST Framework | Python      | Data-heavy apps, admin panel included     |
| Spring Boot           | Java/Kotlin | Enterprise Java ecosystems                |
| Go (Gin / Echo)       | Go          | High-performance microservices            |
| Firebase / Supabase   | N/A (BaaS)  | Rapid prototyping, serverless             |
| Skip backend entirely | —           | If using BaaS or offline-first mobile app |

### 1.2 — Pick a Frontend / Client

| Option                         | Language        | Best for                           |
| ------------------------------ | --------------- | ---------------------------------- |
| React (Vite + TanStack Router) | TypeScript      | Web SPA (this template's choice)   |
| Next.js                        | TypeScript      | Web with SSR/SSG                   |
| React Native                   | TypeScript      | Cross-platform iOS + Android       |
| Flutter                        | Dart            | Cross-platform iOS + Android + Web |
| Swift (SwiftUI)                | Swift           | iOS-only native                    |
| Kotlin (Jetpack Compose)       | Kotlin          | Android-only native                |
| .NET MAUI                      | C#              | Cross-platform from .NET ecosystem |
| Electron / Tauri               | TypeScript/Rust | Desktop apps                       |

### 1.3 — Pick Your API Contract Strategy

This is critical — it determines how AI generates client code from the backend.

| Strategy                | Tools                           | How it works                                                    |
| ----------------------- | ------------------------------- | --------------------------------------------------------------- |
| OpenAPI / Swagger       | Orval, openapi-generator, NSwag | Backend exports JSON spec → generator creates typed client code |
| GraphQL                 | graphql-codegen, Relay          | Schema → typed queries, mutations, fragments                    |
| tRPC                    | tRPC                            | Shared TypeScript types — no code generation needed             |
| Protocol Buffers / gRPC | protoc, Buf                     | `.proto` files → typed client stubs                             |
| Manual                  | —                               | No generation; you write API calls by hand (not recommended)    |

> **Why this matters:** In the original template, `npm run api:sync` exports a Swagger spec and runs Orval to generate TypeScript hooks. Your equivalent command depends on this choice.

### 1.4 — Document Your Choices

Create a file (e.g., `docs/stack-decisions.md`) recording:

```markdown
# Stack Decisions

- **Backend:** [your choice] — [reason]
- **Client:** [your choice] — [reason]
- **API contract:** [your choice] — [reason]
- **State management:** [your choice] — [reason]
- **Database:** [your choice] — [reason]
- **Auth strategy:** [your choice] — [reason]
```

> **Important:** Always document your _reason_ for each choice — not just what you picked. A reason forces you to think critically rather than just accepting a default.
>
> More importantly: **choose a stack you actually understand.** AI will confidently generate code, explain errors, and suggest patterns for any technology — but it is not infallible. If you are unfamiliar with the stack, you will have no way to tell when AI is wrong, outdated, or subtly misguiding you. Your own understanding is the final check. Pick a stack where you can read the generated code, spot mistakes, and verify the AI's reasoning yourself.

---

## Step 2: Set Up the Repository Structure

Create a monorepo (or multi-repo) with clear separation.

### 2.1 — Root Structure

```
your-project/
├── .github/
│   └── copilot-instructions.md        ← AI instructions (GitHub Copilot)
├── .claude/
│   ├── CLAUDE.md                       ← AI instructions (Claude)
│   ├── rules/                          ← Stack-specific rules (auto-loaded by glob)
│   ├── agents/                         ← Specialized AI agents
│   └── skills/                         ← Reusable AI skills (commands)
├── .agents/
│   └── skills/                         ← VS Code Copilot skills
├── backend/                            ← Your backend project
├── mobile/  (or frontend/)             ← Your client project
├── docs/                               ← Human-readable documentation
│   ├── ai-workflow.md                  ← How to use AI with this project
│   ├── coding-style.md                 ← Single source of truth for style rules
│   └── feature-generation/             ← Layer patterns and checklists
├── feature_docs/                       ← Feature specifications
│   ├── FEATURES.md                     ← Feature index table
│   └── _templates/
│       └── feature-spec-template.md    ← Spec template
├── templates/                          ← Internal templates and commands
└── package.json                        ← Root scripts (api:sync, quality checks)
```

### 2.2 — Adapt for Mobile

If building a mobile app, the `frontend/` folder becomes `mobile/` or `ios/` + `android/`:

```
# React Native
mobile/
├── src/
│   ├── api/generated/          ← Same concept — generated API client
│   ├── features/               ← Feature-based folder structure
│   ├── navigation/             ← React Navigation setup
│   ├── store/                  ← Redux / Zustand for UI state
│   └── locales/                ← i18n translations

# Flutter
mobile/
├── lib/
│   ├── api/generated/          ← Generated API client (openapi-generator)
│   ├── features/               ← Feature-based folder structure
│   ├── navigation/             ← GoRouter or Navigator 2.0
│   ├── state/                  ← Riverpod / BLoC
│   └── l10n/                   ← Localization

# Swift (iOS native)
ios/
├── Sources/
│   ├── API/Generated/          ← Generated from OpenAPI spec
│   ├── Features/               ← Feature-based modules
│   ├── Navigation/             ← Coordinator pattern
│   └── Resources/Localizable/  ← String catalogs
```

---

## Step 3: Write the AI Instructions File

This is the most important file in the entire template. It tells the AI how your project works.

### 3.1 — Create the Main Instructions File

For GitHub Copilot: `.github/copilot-instructions.md`
For Claude: `.claude/CLAUDE.md`
For Cursor: `.cursorrules`

> **This is the single most important step in the entire guide.** The instruction file is what makes the AI understand your project — without it, every response is generic and inconsistent. A well-written instruction file is the difference between an AI that follows your conventions and one that ignores them.
>
> **Follow official best practices when writing it:**
>
> - GitHub Copilot → [GitHub Copilot Best Practices](https://docs.github.com/en/copilot/get-started/best-practices) + [spec-kit](https://github.com/github/spec-kit)
> - Claude → [Claude Code Best Practices](https://docs.anthropic.com/en/docs/claude-code/best-practices)
>
> **You do not have to write it from scratch.** Once you have made your stack decisions (Step 1) and set up your repository structure (Step 2), you can ask AI to generate a first draft of the instruction file for you — describe your stack, your layer pattern, and your key rules, and let it produce the skeleton. Then review it carefully, correct anything wrong, and add your project-specific hard rules on top.
>
> Treat this file as living documentation: update it whenever a convention changes, a new rule is added, or you discover the AI is consistently getting something wrong.

### 3.2 — Sections to Include

Use this structure — adapt the content to your stack:

```markdown
# Project Conventions

## Clarify Before You Code (ALWAYS DO THIS)

**For new features, always ask:**

1. Entity fields — types, constraints, required/optional?
2. Relationships — FKs, one-to-many, many-to-many?
3. Business rules — special logic, state machines?
4. Endpoints — standard CRUD or custom actions?
5. Client UI — what screens/components?
6. Authorization — public, auth-required, role-based?

Ask all questions in a single message. Do not start coding until requirements are clear.

## Feature Spec Sync (ALWAYS DO THIS)

Specs live in `feature_docs/`. Each spec is the single source of truth.

**Workflow for every request:**

1. Read `feature_docs/FEATURES.md` — find the feature's spec path
2. If new feature — create spec first, do NOT write code until spec is complete
3. NEVER make code changes before reading the spec
4. Make changes
5. ALWAYS update the spec if behavior changed

## Database Safety (HARD RULE)

[Your database safety rules — e.g., never run destructive migrations automatically]

## Secrets & API Keys (HARD RULE)

- NEVER store API keys, tokens, or secrets in frontend code — they are visible to anyone who opens browser DevTools
- NEVER commit secrets to source control — use environment variables, a secrets manager, or your platform's secret store (e.g., `.env` files that are git-ignored, Doppler, AWS Secrets Manager, Azure Key Vault)
- All external API calls that require a secret key must go through the backend — the frontend calls your own API, your backend calls the third-party service

## Backend Rules

[Your stack-specific backend rules]

## Client Rules

[Your stack-specific client/mobile rules]
```

> **Protect your secrets from AI too.** Your AI coding assistant reads the files you open and your workspace context. To prevent secrets from leaking into AI chat history or completions:
>
> - Add `.env` files to `.gitignore` and never open them in the editor when chatting with AI
> - In VS Code, use `files.exclude` in `.vscode/settings.json` to hide secret files from the workspace
> - In Copilot, use `.github/copilot-instructions.md` to explicitly state: "Never read, suggest, or output the contents of `.env` files"
> - In Claude / Claude Code, similar rules can be added to `CLAUDE.md`
>
> The best defense is never having plaintext secrets in files the AI can reach. Use secret management tools (e.g., `dotnet user-secrets`, OS keychain, a vault service) so secrets are never written to disk in your project folder at all.

### 3.3 — Key Principles to Encode

Regardless of tech stack, always include rules for:

| Category                | Example rules                                                               |
| ----------------------- | --------------------------------------------------------------------------- |
| **Security**            | Password hashing algorithm, no secrets in config files, auth token handling |
| **Data access**         | Read-only query optimizations, pagination requirements                      |
| **API response format** | Standard response wrapper (equivalent of `ApiResponse<T>`)                  |
| **Error handling**      | Custom exception types that map to HTTP status codes                        |
| **State management**    | What goes in local state vs. server cache vs. global state                  |
| **Localization**        | All visible text through translation function, update all locale files      |
| **Code generation**     | Never edit generated files, always sync after backend changes               |

---

## Step 4: Create Stack-Specific Rule Files

Rule files are loaded automatically when editing matching files — so the AI gets the right rules for the right file.

### 4.1 — Backend Rules

Create a file like `.claude/rules/backend.md` (or equivalent for your AI tool):

```markdown
---
globs: backend/**/*.cs   ← change to your file pattern (*.kt, *.py, *.go, etc.)
---

# Backend Critical Conventions

- [Your security rules]
- [Your data access patterns]
- [Your API response conventions]
- [Your error handling patterns]
- Register every new feature's services in the DI container

## Key Infrastructure — use these, do not reinvent

| Type                | Location | Purpose                   |
| ------------------- | -------- | ------------------------- |
| BaseEntity          | ...      | Base for all entities     |
| ApiResponse         | ...      | Standard response wrapper |
| PagedResult         | ...      | Paginated list wrapper    |
| NotFoundException   | ...      | Auto-mapped to 404        |
| ValidationException | ...      | Auto-mapped to 400        |
```

### 4.2 — Client Rules

Create `.claude/rules/client.md` (adapt globs to your file types):

```markdown
---
globs: mobile/**/*.{ts,tsx}   ← or *.dart, *.swift, *.kt
---

# Client Critical Conventions

- Never edit generated API files — they are overwritten on sync
- Import types from generated code — never redefine locally
- Server state in [React Query / Riverpod / Combine] — never duplicate into local state
- All visible text through translation function
- [Your navigation conventions]
- [Your component naming conventions]
```

---

## Step 5: Create the Feature Spec Template

The spec template is the contract between the developer and the AI. It defines what AI needs to know before generating code.

### 5.1 — Create the Template

Create `feature_docs/_templates/feature-spec-template.md`:

```markdown
# Feature Specification: [FeatureName]

**Last Updated:** [YYYY-MM-DD]

## 1. Entity

**Name:** [EntityName]

### Fields

| Property | Type   | Required | Constraints | Notes |
| -------- | ------ | -------- | ----------- | ----- |
| Name     | string | yes      | max 200     |       |

> ID and timestamps are inherited from base entity — do not list them.

## 2. Relationships

- none

## 3. Architecture Decisions

- Standard layered pattern

## 4. Data Flow

[Describe how data moves through layers for key operations]

## 5. API Endpoints

| Method | Route                | Description      | Auth |
| ------ | -------------------- | ---------------- | ---- |
| GET    | /api/[features]      | List (paginated) | no   |
| GET    | /api/[features]/{id} | Get single       | no   |
| POST   | /api/[features]      | Create           | yes  |
| PUT    | /api/[features]/{id} | Update           | yes  |
| DELETE | /api/[features]/{id} | Delete           | yes  |

## 6. Validation Rules

- Name: required, not empty, max 200 characters

## 7. Business Rules

[Special logic, state machines, ownership checks]

### Acceptance Scenarios

**Scenario: Create with valid data**

- Given: valid request
- When: processed
- Then: returns success with created entity

**Scenario: Create with invalid data**

- Given: empty required field
- When: processed
- Then: returns validation error

## 8. Authorization

- none (or describe role requirements)

## 9. Client UI

### Screens

[Describe each screen: list view, detail view, form, etc.]

### Navigation

[How users navigate to/from this feature]

## 10. Local State

[UI-only state — search filters, selected items, modals open/closed]

## 11. File Locations

### Backend

- Entity: `backend/src/.../Features/[Feature]/[Feature].cs`
- [... list all expected files]

### Client

- Screen: `mobile/src/features/[feature]/screens/[feature]-list.tsx`
- [... list all expected files]

## 12. Tests

- [ ] Backend: happy path, not-found, validation failure
- [ ] Client: render, form submit, error state
```

### 5.2 — Create the Feature Index

Create `feature_docs/FEATURES.md`:

```markdown
# Feature Index

> Read this file first. Identify the relevant feature, then read only its spec.

| Feature   | Spec File               | Data Model | Summary          | Status       |
| --------- | ----------------------- | ---------- | ---------------- | ------------ |
| [Example] | feature-spec-example.md | —          | Template example | Example only |
```

---

## Step 6: Create Code Templates (The "\_Template" Pattern)

This is what makes AI scaffolding reliable — real, working code that AI copies and adapts.

### 6.1 — Backend Template

Create a `_FeatureTemplate` folder inside your backend features directory containing every file a feature needs. This is real, compilable code — not pseudocode.

For example, with ASP.NET Core:

```
backend/src/.../Features/_FeatureTemplate/
├── Feature.cs              ← Entity
├── FeatureDtos.cs          ← Request/response DTOs
├── FeatureValidator.cs     ← Validation rules
├── IFeatureRepository.cs   ← Repository interface
├── FeatureRepository.cs    ← Data access implementation
├── IFeatureService.cs      ← Service interface
├── FeatureService.cs       ← Business logic
└── FeatureController.cs    ← HTTP controller
```

For **other stacks**, adapt the layers:

| Stack                | Template files                                                                             |
| -------------------- | ------------------------------------------------------------------------------------------ |
| Express + TypeScript | `model.ts`, `schema.ts` (Zod), `repository.ts`, `service.ts`, `controller.ts`, `routes.ts` |
| Django               | `models.py`, `serializers.py`, `views.py`, `urls.py`, `tests.py`                           |
| Spring Boot          | `Entity.kt`, `Dto.kt`, `Repository.kt`, `Service.kt`, `Controller.kt`                      |
| Go (Gin)             | `model.go`, `handler.go`, `repository.go`, `service.go`, `routes.go`                       |

### 6.2 — Client Template

Create a `_template-feature` folder in your client features directory:

For React / React Native:

```
mobile/src/features/_template-feature/
├── components/
│   ├── feature-list-screen.tsx
│   ├── feature-form-screen.tsx
│   └── feature-delete-dialog.tsx
├── hooks/
│   ├── use-feature-pagination.ts
│   └── use-feature-form.ts
└── store/
    └── feature-slice.ts
```

For Flutter:

```
mobile/lib/features/_template_feature/
├── presentation/
│   ├── feature_list_page.dart
│   ├── feature_form_page.dart
│   └── widgets/feature_card.dart
├── domain/
│   └── feature_model.dart
└── data/
    └── feature_repository.dart
```

For Swift (iOS):

```
ios/Sources/Features/_TemplateFeature/
├── Views/
│   ├── FeatureListView.swift
│   └── FeatureFormView.swift
├── ViewModels/
│   └── FeatureViewModel.swift
└── Models/
    └── Feature.swift
```

### 6.3 — Test Templates

Create test templates alongside code templates:

```
backend/tests/Features/_FeatureTemplate/
├── FeatureServiceTests.cs
├── FeatureControllerTests.cs
└── FeatureValidatorTests.cs
```

These give AI the exact test patterns to follow — the right mocking library, assertion style, and naming convention.

---

## Step 7: Create the API Sync Pipeline

This is the automation that bridges backend and client.

### 7.1 — Define the Sync Command

In your root `package.json` (or `Makefile`, `Taskfile`, etc.):

```json
{
  "scripts": {
    "api:export": "[build backend] && [export API spec to a file]",
    "api:generate": "[run code generator against the spec]",
    "api:sync": "npm run api:export && npm run api:generate"
  }
}
```

### 7.2 — Examples by Stack

| Backend                  | Export command                                    | Client generator                              |
| ------------------------ | ------------------------------------------------- | --------------------------------------------- |
| ASP.NET Core + Swagger   | `dotnet swagger tofile --output swagger.json ...` | Orval, NSwag, openapi-generator               |
| Express + swagger-jsdoc  | Build step exports `swagger.json`                 | Orval, openapi-generator                      |
| Django + drf-spectacular | `python manage.py spectacular --file schema.yaml` | openapi-generator                             |
| Spring Boot + springdoc  | Build exports `/v3/api-docs`                      | openapi-generator                             |
| GraphQL (any)            | `graphql-codegen`                                 | graphql-codegen (does both export + generate) |
| Go + swag                | `swag init`                                       | openapi-generator, oapi-codegen               |

### 7.3 — Mobile-Specific Considerations

For mobile apps, the generated client typically goes in:

```
mobile/src/api/generated/     ← React Native (Orval / openapi-generator)
mobile/lib/api/generated/     ← Flutter (openapi-generator)
ios/Sources/API/Generated/    ← Swift (openapi-generator / CreateAPI)
android/app/src/.../api/      ← Kotlin (openapi-generator)
```

Add a `.gitignore` entry or a comment header in generated files so developers know not to edit them.

---

## Step 8: Create AI Skills (Automated Commands)

Skills are reusable AI commands that follow a specific workflow. Each skill is a markdown file with instructions.

> **Tip:** Before writing skills from scratch, check [skills.sh](https://skills.sh) — a community registry of open-source Claude Code skills. Many common workflows (spec generation, API sync, scaffolding, testing, etc.) already have published skills you can install directly instead of authoring them yourself.

### 8.1 — Essential Skills to Create

| Skill                     | Purpose                                    | When to use               |
| ------------------------- | ------------------------------------------ | ------------------------- |
| `create-spec`             | Create a new feature spec from template    | Starting a new feature    |
| `clarify-spec`            | Resolve TODO markers in an incomplete spec | Spec has gaps             |
| `scaffold-feature`        | Generate backend code from spec            | After spec is complete    |
| `scaffold-feature-client` | Generate client code from spec             | After API sync            |
| `api-sync`                | Export spec + regenerate client code       | After backend changes     |
| `quality-check`           | Run all build/lint/test checks             | After any code change     |
| `add-translations`        | Add i18n keys to all locale files          | After adding UI strings   |
| `new-feature`             | Orchestrate the full end-to-end flow       | Combines all above skills |

### 8.2 — Skill File Structure

Each skill lives in `.agents/skills/[skill-name]/SKILL.md` (for VS Code Copilot) or `.claude/skills/[skill-name]/` (for Claude):

```markdown
---
name: scaffold-feature
description: Scaffold the backend for a new feature from a spec file.
argument-hint: "[feature-name]"
allowed-tools: "Read, Write, Edit, Bash, Glob, Grep"
---

# Scaffold Backend Feature

Scaffold the complete backend for the `$ARGUMENTS` feature.

## Prerequisites

### Step P1: Verify the spec file exists

Read `feature_docs/$ARGUMENTS/feature-spec-$ARGUMENTS.md`.
If missing → stop and tell the user to run `/create-spec $ARGUMENTS`.

### Step P2: Check for unresolved markers

Scan for `<!-- TODO:` or `[NEEDS CLARIFICATION:`.
If found → stop and list them.

## Step 1: Read the Templates

Read all files in `Features/_FeatureTemplate/` to learn the patterns.

## Step 2: Generate the Feature

Create all files following the template patterns, adapted to the spec.

## Step 3: Register Services

Add DI registration lines to the app's startup file.

## Step 4: Create Migration (if applicable)

Run the migration command and tell the user to apply it.

## Step 5: Verify

Run the build command to confirm zero errors.
```

### 8.3 — The Quality Check Skill

This is the simplest but most important skill:

```markdown
---
name: quality-check
description: Run all quality checks.
---

# Quality Check

Run all checks sequentially. Continue through all even if one fails.

## 1. Backend Build

[your build command]

## 2. Backend Tests

[your test command]

## 3. Client Lint

[your lint command]

## 4. Client Type Check

[your type check command]

## 5. Client Tests

[your test command]

## 6. Translation Validation

[check all locale files have matching keys]

## Report

List all failures with file paths and error messages.
```

---

## Step 9: Create AI Agents (Specialized Roles)

Agents are specialized AI personas with specific expertise and restricted tool access.

### 9.1 — Essential Agents

Create these in `.claude/agents/` or equivalent:

**PR Reviewer** (`pr-reviewer.md`):

```markdown
---
name: pr-reviewer
description: Senior code reviewer. Use when reviewing code on the current branch.
tools: Read, Glob, Grep
---

You are a senior engineer reviewing code against project conventions.

## Checklist

- [ ] [Your backend conventions]
- [ ] [Your client conventions]
- [ ] [Your security rules]
- [ ] Feature spec is up to date
```

**Spec Sync** (`spec-sync.md`):

```markdown
---
name: spec-sync
description: Keeps feature specs in sync with code changes.
tools: Read, Write, Edit, Glob, Grep
---

Update the feature spec when code changes affect entity fields,
validation rules, endpoints, UI behavior, or architecture.
```

**Unit Tester** (`unit-tester.md`):

```markdown
---
name: unit-tester
description: Testing specialist using Equivalence Partitioning methodology.
tools: Read, Write, Edit, Glob, Grep
---

Write minimal, focused tests using the Equivalence Partitioning Method.
Follow the project's test framework and patterns.
```

---

## Step 10: Configure MCP Servers

MCP (Model Context Protocol) servers extend the AI with live, tool-level access to external systems — databases, docs, APIs, design systems — without copy-pasting context into the chat.

### 10.1 — What MCP Gives You

| Without MCP | With MCP |
| --- | --- |
| Paste docs manually into chat | AI queries live docs on demand |
| Describe your component library | AI reads your registry directly |
| Copy-paste API schemas | AI fetches the schema itself |

### 10.2 — Project-Level Config (`.mcp.json`)

Create `.mcp.json` at the repo root. Claude Code picks this up automatically for all contributors.

```json
{
  "mcpServers": {
    "context7": {
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "@upstash/context7-mcp", "--api-key", "YOUR_API_KEY_HERE"]
    },
    "shadcn": {
      "command": "npx",
      "args": ["shadcn@latest", "mcp"],
      "cwd": "frontend"
    }
  }
}
```

> On Windows wrap the command in `cmd /c`: `"command": "cmd", "args": ["/c", "npx", ...]`

### 10.3 — Recommended MCP Servers

| Server | Package | What it provides |
| --- | --- | --- |
| **Context7** | `@upstash/context7-mcp` | Up-to-date library docs & code examples (React, .NET, etc.) |
| **shadcn** | `shadcn@latest mcp` | Live shadcn component registry, examples, and `add` commands |
| **Postgres / SQLite** | `@modelcontextprotocol/server-postgres` | AI can query your DB schema and data directly |
| **Filesystem** | `@modelcontextprotocol/server-filesystem` | Scoped file access outside the working directory |
| **GitHub** | `@modelcontextprotocol/server-github` | Read issues, PRs, and code from GitHub repos |

Browse the full community catalog at [mcp.so](https://mcp.so) or [glama.ai/mcp/servers](https://glama.ai/mcp/servers).

### 10.4 — Wiring MCP Into CLAUDE.md

Tell the AI when to use each server so it doesn't have to guess:

```markdown
## Tooling

- Always use Context7 MCP when library/API documentation or configuration steps are needed — do not ask, just use it
- Use the shadcn MCP when adding or configuring UI components
```

---

## Step 11: Create Documentation

### 11.1 — AI Workflow Guide (`docs/ai-workflow.md`)

Document the workflows developers will use daily:

```markdown
# AI Workflow Guide

## The Golden Rule

Backend first → sync → client.

## Workflow 1: Add a New Feature

Step 1 — Fill in the feature spec
Step 2 — Scaffold the backend
Step 3 — Create & apply migration
Step 4 — Sync the API contract
Step 5 — Scaffold the client
Step 6 — Add translations
Step 7 — Quality check

## Workflow 2: Modify an Existing Feature

Step 1 — Update the spec first
Step 2 — Change the backend
Step 3 — Sync + update the client

## Workflow 3: Fix a Bug

Step 1 — Provide full context (error, stack trace, file)
Step 2 — AI diagnoses and fixes
Step 3 — Verify the fix
Step 4 — Update spec if behavior changed
```

### 11.2 — Coding Style Guide (`docs/coding-style.md`)

This is the single source of truth for all style rules. AI reads this file and follows it.

Include sections for:

- Naming conventions (files, classes, variables, functions)
- Formatting (indentation, braces, line length)
- Language-specific idioms
- Component structure
- Test naming and organization

### 11.3 — Feature Generation Guides

Create `docs/feature-generation/backend.md` and `docs/feature-generation/client.md` describing:

- The layer pattern (which files, in what order)
- Exception/error handling patterns
- The implementation checklist

### 11.4 — Implementation Checklist

Create `docs/feature-generation/implementation-checklist.md` — a checkbox list for verifying a feature is complete:

```markdown
# Implementation Checklist

## Backend

- [ ] Entity extends base class
- [ ] DTOs defined
- [ ] Validation rules defined
- [ ] Repository + Service + Controller created
- [ ] Services registered in DI
- [ ] Migration created
- [ ] Build passes

## API Sync

- [ ] Sync command run
- [ ] Generated client code visible

## Client

- [ ] Screens/components created
- [ ] Navigation wired up
- [ ] Translation keys in all locale files
- [ ] No hardcoded strings
- [ ] Lint passes
- [ ] Type check passes

## Spec

- [ ] Spec exists and reflects final implementation
- [ ] Feature registered in FEATURES.md
```

---

## Step 12: Set Up Root Automation Scripts

### 12.1 — Root `package.json` (or Makefile)

```json
{
  "scripts": {
    "dev": "concurrently \"[start backend]\" \"[start client]\"",
    "api:export": "[build + export API spec]",
    "api:generate": "cd mobile && [run generator]",
    "api:sync": "npm run api:export && npm run api:generate",
    "format": "[format all code]",
    "format:check": "[check formatting without fixing]"
  }
}
```

### 12.2 — Enforce Strict Linting and Formatting (CRITICAL)

Strict linting and formatting rules are not optional — they are the foundation of AI-generated code quality.

When AI generates code, it will follow your linter's rules **only if the linter is configured and enforced**. Without them, every AI-generated file will have slightly different formatting, inconsistent naming, and subtle style drift that accumulates over time.

**Set up and enforce:**

| Tool                           | Stack                   | Purpose                                                |
| ------------------------------ | ----------------------- | ------------------------------------------------------ |
| ESLint                         | TypeScript / JavaScript | Code quality rules, import order, unused vars          |
| Prettier                       | Any frontend            | Consistent formatting (semicolons, quotes, line width) |
| `dotnet format`                | C#                      | Code style enforcement                                 |
| SwiftLint                      | Swift                   | Style and convention enforcement                       |
| ktlint / detekt                | Kotlin                  | Code style + static analysis                           |
| `dart analyze` + `dart format` | Flutter                 | Lint rules + formatting                                |

**Make rules strict from day one.** It's much harder to tighten rules later when you already have hundreds of files with violations. Start strict, loosen only with good reason.

Add format-on-save to your editor config (`.vscode/settings.json`):

```json
{
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.fixAll.eslint": "explicit"
  }
}
```

### 12.3 — Git Hooks (Optional)

Set up pre-commit hooks to enforce quality:

- Format check
- Lint check
- Translation key sync check

---

## Step 13: Validate the Template

Before considering the template done, verify these scenarios:

### 13.1 — Smoke Test: Create a Sample Feature

1. Run `/create-spec sample-items` (or manually create the spec)
2. Fill in the spec with a simple entity (Name, Description)
3. Run `/scaffold-feature sample-items`
4. Verify all backend files are created and build passes
5. Run `api:sync`
6. Run `/scaffold-feature-client sample-items`
7. Verify client code builds/compiles
8. Run `/quality-check`
9. Delete the sample feature

### 13.2 — Verify AI Understands the Rules

Ask the AI:

- "What is the project structure?"
- "What conventions does this project follow?"
- "How do I add a new feature?"

The AI should answer accurately based on your instruction files.

### 13.3 — Verify Spec Sync

1. Make a change to an existing feature (add a field)
2. Verify the AI updates the spec file automatically
3. Verify `FEATURES.md` index is updated

---

## Quick Reference: File Purposes

| File/Folder                       | Purpose                           |
| --------------------------------- | --------------------------------- |
| `.github/copilot-instructions.md` | Main AI instructions (Copilot)    |
| `.claude/CLAUDE.md`               | Main AI instructions (Claude)     |
| `.claude/rules/*.md`              | Auto-loaded rules by file pattern |
| `.claude/agents/*.md`             | Specialized AI agents             |
| `.agents/skills/*/SKILL.md`       | Reusable AI commands              |
| `docs/ai-workflow.md`             | Human guide for AI workflows      |
| `docs/coding-style.md`            | Style rules (AI reads this)       |
| `docs/feature-generation/*.md`    | Layer patterns and checklists     |
| `feature_docs/FEATURES.md`        | Feature index                     |
| `feature_docs/_templates/`        | Spec templates                    |
| `feature_docs/[feature]/`         | One spec per feature              |
| `Features/_FeatureTemplate/`      | Code templates AI copies from     |
| `package.json` (root)             | `api:sync` and automation scripts |

---

## Step 14: Maintain the Template Over Time

AI tools evolve rapidly. Instruction formats change, new capabilities appear, and models behave differently across versions. Your template is a living system that needs periodic maintenance.

### 14.1 — Review Template Code Regularly

The `_FeatureTemplate` folder is the **single most important thing in your template.** Every feature AI generates is a copy-and-adapt of this code. If the template code has bugs, bad patterns, or outdated practices, every generated feature inherits those problems.

**Treat template code like production code:**

- Review it in code reviews
- Run linters and tests against it
- Update it when you discover better patterns
- After updating, re-scaffold a test feature to verify the output is still correct

### 14.2 — Track Convention Changes

Keep a changelog (`docs/changelog.md`) of convention changes:

```markdown
| Date       | Change                                                | Reason                                 |
| ---------- | ----------------------------------------------------- | -------------------------------------- |
| 2026-03-01 | Added strict ESLint rule for import order             | AI was generating inconsistent imports |
| 2026-02-15 | Updated spec template to require acceptance scenarios | Improved test coverage                 |
```

This helps new team members understand why rules exist and prevents them from being accidentally removed.

### 14.3 — Keep Up with AI Tool Changes

- **Instruction file formats** change — check your AI tool's docs quarterly for new features (e.g., new glob patterns, new skill capabilities, new agent syntax)
- **Model upgrades** may change behavior — when a new model version is released, test your key workflows (scaffold, quality-check) to make sure they still work
- **New community skills** appear on [skills.sh](https://skills.sh) — check periodically for skills that could replace your custom ones
- **spec-kit** updates — [github/spec-kit](https://github.com/github/spec-kit) evolves alongside Copilot; update your instruction files when new best practices emerge

### 14.4 — Different AI Models Behave Differently

This is the most important thing to understand about AI-driven development:

**Do not expect identical output across different AI models, or even across different sessions with the same model.**

- GPT-4o, Claude Sonnet, Gemini Pro — each interprets instructions slightly differently
- The same model can produce different code on different runs
- Newer model versions may ignore rules that older versions followed
- Some models are better at following complex multi-step skills; others get lost

**Strategies to handle this:**

1. **Make rules explicit, not implicit.** Don't assume the AI will "know" your convention. Write it in the rule file.
2. **Use strict linting as a safety net.** If the AI ignores a naming convention, the linter catches it immediately.
3. **Keep template code simple.** Complex templates are harder for AI to adapt correctly. Simpler templates = more consistent output.
4. **Run quality checks after every generation.** The `/quality-check` skill exists specifically because AI output is non-deterministic.
5. **Don't chase perfection.** Accept that you'll need to make small manual fixes after generation. The goal is 90% automated, not 100%.

---

## Common Mistakes to Avoid

1. **Skipping the spec** — AI generates better code when it has a complete spec. Don't rush to code.
2. **Editing generated files** — They get overwritten. Customize through wrappers or configuration.
3. **Putting rules in the wrong file** — Stack-specific rules go in rule files (auto-loaded by glob), not the main instructions file.
4. **No code templates** — Without `_FeatureTemplate`, AI guesses at your patterns and gets them wrong. Real, working templates are essential.
5. **Neglecting template code quality** — Bugs in `_FeatureTemplate` propagate to every generated feature. Review template code as carefully as production code.
6. **No API sync step** — Manual client-side type definitions drift from the backend. Automate the sync.
7. **Too many rules at once** — Start with the critical rules (security, error handling, response format) and add more as patterns emerge.
8. **Not testing the template** — Always scaffold a sample feature end-to-end before using the template for real work.
9. **No strict linting** — Without ESLint/Prettier/formatters enforced, AI output drifts in style across features. Linting is your safety net.
10. **Assuming all AI models are the same** — Test your template with the specific model you'll be using. Don't assume rules written for one model will work identically on another.
11. **Not maintaining the template** — AI tools change fast. Instruction formats, model behavior, and best practices shift. Schedule quarterly reviews of your template.

---
