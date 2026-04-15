# Project Conventions

> Backend and frontend conventions are in `.claude/rules/` and load automatically when editing relevant files.

---

## Clarify Before You Code (ALWAYS DO THIS)

**For new features, always ask:**

1. **Entity fields** — What fields does the entity need? What are their types, constraints, and whether they're required or optional?
2. **Relationships** — Does this entity relate to any other entity (e.g., belongs to a User)?
3. **Business rules** — Any special logic? (e.g., status transitions, computed fields, restrictions)
4. **Endpoints** — Standard CRUD only, or are there custom actions (e.g., toggle, bulk delete)?
5. **Frontend UI** — Table + form dialog (standard)? Or something different?
6. **Authorization** — Public, authenticated only, or role-restricted?

**For changes to existing features, ask if any of these are unclear:**

- What is the exact expected behavior after the change?
- Does this affect any other feature or shared component?
- Should old data be migrated or is this forward-only?

Ask all questions in a single message. Do not start coding until requirements are clear.

---

## Feature Generation

- **Backend steps + exception handling + layer pattern** → `docs/feature-generation/backend.md`
- **Frontend steps + file tree + file roles** → `docs/feature-generation/frontend.md`
- **Implementation checklist** → `docs/feature-generation/implementation-checklist.md`

---

## Feature Spec Sync (ALWAYS DO THIS)

Specs live in `feature_docs/` — one subfolder per feature (e.g. `feature_docs/factories/feature-spec-factories.md`). Templates live in `feature_docs/_templates/`. Each spec is the single source of truth combining behavior and architecture.

**Spec section order:**

1. Entity · 2. Core Values & Principles · 3. Architecture Decisions · 4. Data Flow · 5. API Endpoints · 6. Validation Rules · 7. Business Rules · 8. Authorization · 9. Frontend UI Description · 10. Redux UI State · 11. File Locations · 12. Tests

**Workflow for every request (DO NOT SKIP):**

1. Read `feature_docs/FEATURES.md` — identify the feature and its spec path
2. **If the feature is NEW (no spec file exists yet):**
   - Use the `/create-spec` skill to create the spec file first
   - **Do NOT write any code until the spec file exists and is complete**
   - Register the feature in `feature_docs/FEATURES.md`
3. **NEVER make any code change before reading that feature's spec file.** Read it in full.
4. Make changes
5. **ALWAYS update the spec before responding to the user** if the task changed any of: entity fields, validation or business rules, endpoints or auth, UI behavior or Redux state, architecture. Use the `spec-sync` agent to do this. **Do NOT consider the task complete until the spec is up to date.**
6. **ALWAYS update `feature_docs/FEATURES.md`** as part of spec-sync — keep the row's Data Model, Summary, and Status columns in sync with the spec after every change.

**No update needed for:** code quality fixes with no behavior impact.

---

## Convention Changelog

`docs/changelog.md` tracks project-wide convention and architectural changes.

**At the end of any session that changed a convention** (CLAUDE.md, `.claude/rules/`, `docs/coding-style.md`, or a skill), append one row per distinct change to `docs/changelog.md`:

| Date       | Change                       | Reason             |
| ---------- | ---------------------------- | ------------------ |
| YYYY-MM-DD | What rule/convention changed | Why it was changed |

**Skip it for:** feature additions, bug fixes, or code changes with no convention impact.

---

## Coding Style

→ See `docs/coding-style.md` for all style rules (naming, formatting, Tailwind, testing).

**To change a style rule:** edit `docs/coding-style.md`, commit it, and the AI will follow the updated rules from the next session onward.

---

## Database Safety (HARD RULE — NEVER VIOLATE)

→ Full details in `.claude/rules/backend.md` (auto-loaded for `.cs` files). Summary for non-backend contexts:

- The AI may ONLY run `dotnet ef migrations add <Name>` to create migration files
- The AI must NEVER run `dotnet ef database update`, `dotnet ef database drop`, `dotnet ef migrations remove`, or any destructive SQL
- After creating a migration, always tell the user to run `dotnet ef database update` themselves

---

## Tooling

- Always use Context7 MCP when library/API documentation, code generation, or configuration steps are needed — do not ask, just use it if available
- Convention history and change rationale: see `docs/changelog.md`

## Code Intelligence

Prefer LSP over Grep/Read for code navigation - it's faster, precise and avoids reading entire files:

- `workspaceSymbol` to find where something is defined
- `findReferences` to see all usages across the codebase
- `goToDefinition`/`goToImplementation` to jump to source
- `hover` for type info without reading the file
  Use Grep only when LSP isn't available or text/pattern searches (comments, string, config)

after writing or editing code, check LSP diagnostics and fix errors before proceeding
