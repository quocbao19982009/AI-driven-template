# Spec-Kit vs. AI-Driven Template — Comparison

## TL;DR

| Dimension | Spec-Kit | AI-Driven Template |
|---|---|---|
| **Primary goal** | Universal SDD framework for any project | Opinionated fullstack starter for .NET + React |
| **Scope** | Tool/methodology (bring your own stack) | Complete application template (specific stack) |
| **Setup** | CLI bootstrap (`specify init`) | Clone-and-go |
| **Agent support** | 20+ agents (Claude, Copilot, Cursor, Gemini…) | Claude Code only |
| **Tech stack** | Agnostic | .NET 10 + React 19 + PostgreSQL (fixed) |
| **Spec ownership** | `.specify/specs/NNN-feature/` (multi-file) | `feature_docs/feature-spec-[name].md` (single file) |

---

## 1. Core Philosophy

**Spec-Kit** inverts the traditional SDLC — specifications *are* the deliverable, code is regenerated output. It enforces a constitution of nine immutable principles (Library-First, Test-First, Anti-Abstraction, etc.) that govern every spec and plan.

**AI-Driven Template** takes a pragmatic, convention-over-configuration approach — specs exist to guide AI scaffolding, but the spec is kept in sync *with* the code rather than being strictly prior to it. `CLAUDE.md` is the "constitution", but focused on concrete technical rules rather than philosophical principles.

---

## 2. Workflow Comparison

| Step | Spec-Kit | AI-Driven Template |
|---|---|---|
| **1. Project init** | `specify init` CLI — generates directory structure | Clone repo — structure already in place |
| **2. Principles** | `/speckit.constitution` — generates a living project constitution | `CLAUDE.md` — pre-written, edit to customize |
| **3. Spec** | `/speckit.specify` — open-ended, AI-shaped spec | `/create-spec` — template with 12 fixed sections |
| **4. Clarify** | `/speckit.clarify` — structured ambiguity resolution | CLAUDE.md mandates asking questions before coding |
| **5. Plan** | `/speckit.plan` — generates `plan.md`, `data-model.md`, `contracts/`, `research.md` | Spec file IS the plan (all sections combined) |
| **6. Tasks** | `/speckit.tasks` — breaks plan into ordered, parallelizable tasks | No task-breakdown step — AI scaffolds directly |
| **7. Implement** | `/speckit.implement` — executes all tasks | `/scaffold-feature` + `/scaffold-feature-frontend` |
| **8. Sync** | `/speckit.analyze` — cross-artifact consistency validation | `spec-sync` agent — keeps spec in sync after code changes |
| **9. DB** | N/A (stack-agnostic) | `dotnet ef migrations add` (developer runs manually) |
| **10. API contract** | N/A | `npm run api:sync` (Orval regenerates TypeScript hooks) |

**Key difference:** Spec-Kit has a richer pre-implementation phase (specify → clarify → plan → tasks → implement). AI-Driven Template is more compressed — spec → scaffold → sync.

---

## 3. Spec Structure

### Spec-Kit — Multi-file per feature (`specs/001-feature-name/`)

| File | Contents |
|---|---|
| `spec.md` | User stories (P1/P2/P3), functional requirements, edge cases, acceptance scenarios (Given/When/Then) |
| `plan.md` | Technical architecture, constitution compliance gates |
| `data-model.md` | Entity definitions and schemas |
| `research.md` | Technology decisions with rationale |
| `contracts/` | API contracts, WebSocket specs, data schemas |
| `tasks.md` | Parallelizable task list with dependencies |
| `quickstart.md` | Key validation scenarios |

### AI-Driven Template — Single file per feature (`feature_docs/feature-spec-[name].md`)

12 ordered sections in one file:

1. Entity
2. Core Values & Principles
3. Architecture Decisions
4. Data Flow
5. API Endpoints
6. Validation Rules
7. Business Rules
8. Authorization
9. Frontend UI Description
10. Redux UI State
11. File Locations
12. Tests

**Key difference:** Spec-Kit separates concerns into multiple files. AI-Driven Template collapses everything into one file — simpler, but less granular.

---

## 4. AI Agent Strategy

### Spec-Kit
- Stack-agnostic — works with 20+ agents (Claude Code, GitHub Copilot, Cursor, Gemini CLI, Amazon Q, etc.)
- Slash commands are plain markdown files, adapted per agent format
- Agent context file (equivalent to `CLAUDE.md`) is auto-updated by `/speckit.plan`

### AI-Driven Template
- Claude Code only (intentional — leverages sub-agents and MCP)
- **Skills** = developer-invoked tools (`/create-spec`, `/scaffold-feature`, `/quality-check`, etc.)
- **Agents** = autonomous specialists (`pr-reviewer`, `spec-sync`, `unit-tester`)
- **MCP servers**: `context7` (live docs lookup), `shadcn` (component install)

**Key difference:** Spec-Kit is portable across agents. AI-Driven Template is Claude-Code-specific and exploits its sub-agent system for autonomous review and sync.

---

## 5. Enforcement Mechanisms

| Mechanism | Spec-Kit | AI-Driven Template |
|---|---|---|
| **Constitutional rules** | 9-article constitution (philosophical principles) | `CLAUDE.md` critical conventions (concrete technical rules) |
| **Completeness gates** | Template checklists, `[NEEDS CLARIFICATION]` markers | "Clarify before you code" mandate in `CLAUDE.md` |
| **Consistency check** | `/speckit.analyze` — cross-artifact validation | `spec-sync` agent runs after every change |
| **Code quality** | `/speckit.checklist` | `/quality-check` skill (build + tests + lint + type-check) |
| **Code review** | Not built-in | `pr-reviewer` agent (SOLID, conventions, `coding-style.md`) |
| **Testing mandate** | Integration-first, strict test-first (Article III) | Equivalence Partitioning via `unit-tester` agent |

---

## 6. What Each Does Better

### Spec-Kit is better for:
- Teams who want a **methodology**, not a specific stack
- Projects that need **multi-agent flexibility** (Copilot, Cursor, Gemini, etc.)
- Deep spec rigor — separate `plan.md`, `data-model.md`, `contracts/` per feature
- **Greenfield exploration** — parallel implementations, creative discovery phases
- Organizations that want an **extension ecosystem** for customization

### AI-Driven Template is better for:
- Teams who want a **production-ready .NET + React app** immediately
- Projects where the stack is decided — no need for agnosticism
- **Faster time-to-feature** — one spec file, direct scaffolding, API bridge auto-generated
- **Claude-specific power** — sub-agents, MCP servers, Orval/OpenAPI integration
- Brownfield additions to a well-defined, layered architecture

---

## 7. Gap Analysis — What Each Is Missing

### Spec-Kit lacks:
- A concrete stack — you still need to set up tech, DI, testing infrastructure, etc.
- API contract automation (no Orval/OpenAPI bridge)
- Automated spec sync after code changes (no `spec-sync` agent equivalent)
- Autonomous code review agent
- Stack-specific scaffolding commands

### AI-Driven Template lacks:
- The rigorous pre-implementation phase (clarify → plan → tasks)
- Multi-agent portability — tightly coupled to Claude Code
- Data model / research / contracts as separate artifacts
- An extension system for customization
- Explicit parallelization markers in task execution (`[P]`)
- Auto-numbered feature branching workflow

---

## 8. Complementarity — Adopting Spec-Kit Ideas

These aren't competitors — they solve adjacent problems. Ideas worth adopting from Spec-Kit:

| Spec-Kit idea | How to adopt in AI-Driven Template |
|---|---|
| `/speckit.clarify` command | Add a `/clarify-spec` skill that iterates on an existing spec before scaffolding |
| Separate `data-model.md` | Add a dedicated "Data Model" section or file alongside the feature spec |
| Parallelizable task list (`tasks.md`) | Add a `/plan-tasks` skill that breaks a spec into an ordered, dependency-aware task list |
| `[NEEDS CLARIFICATION]` markers | Mandate this in the `feature-spec-template.md` for unresolved fields |
| Constitution amendment workflow | Version `CLAUDE.md` changes with a changelog entry |
| Auto-numbered feature branches | Add a `create-new-feature.sh` script that auto-increments the feature number and creates a branch |

---

## 9. Summary

| | Spec-Kit | AI-Driven Template |
|---|---|---|
| Best for | Methodology-first, stack-agnostic teams | Stack-decided, Claude-native teams |
| Spec depth | High (7 files per feature) | Medium (1 file, 12 sections) |
| Pre-implementation rigor | High (clarify → plan → tasks → implement) | Medium (spec → scaffold) |
| Post-implementation sync | Manual (`/speckit.analyze`) | Automatic (`spec-sync` agent) |
| Agent portability | High (20+ agents) | Low (Claude Code only) |
| Time-to-running-app | High (methodology first) | Low (clone and run) |
| Extensibility | Extension catalog system | Edit `CLAUDE.md` and docs |
| API bridge | Not included | Orval + OpenAPI auto-sync |
| Code review | Not included | `pr-reviewer` agent |
| Testing | Strict TDD mandate | `unit-tester` agent, Equivalence Partitioning |
