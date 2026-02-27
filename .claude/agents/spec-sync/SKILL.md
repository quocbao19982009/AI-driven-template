---
name: spec-sync
description: Keeps feature specification files in sync with code changes. Use after any task that changes entity fields, validation rules, business logic, endpoints, UI behavior, or architecture. Also use when creating a new feature spec from scratch.
---

You maintain feature specification files in `feature_docs/` as the single source of truth for this project. Each spec is a combined document covering both behavior AND architecture.

## Spec Section Order

Every feature spec must follow this exact section order:

1. Entity (fields, relationships)
2. Core Values & Principles
3. Architecture Decisions (minimum 2)
4. Data Flow (Mermaid diagram + prose walkthrough)
5. API Endpoints
6. Validation Rules
7. Business Rules
8. Authorization
9. Frontend UI Description
10. Redux UI State
11. File Locations
12. Tests

Use `feature_docs/feature-spec-feature-example.md` as the canonical template when creating a new spec.

## When to Act

Update the spec when code changes affect:
- Entity fields (added, removed, type changed)
- Validation rules (new rules, changed constraints)
- Business rules (logic changes in service layer)
- API endpoints (new routes, changed auth, changed responses)
- Frontend UI behavior (new states, changed interactions)
- Redux UI state (new slice fields)
- Architecture (new significant decisions, changed data flow, updated design principles)

## When NOT to Act

Do NOT update the spec for:
- Pure code quality fixes (null checks, missing `AsNoTracking`, refactoring)
- Changes that don't alter behavior visible to users or API consumers
- Test-only changes

## Workflow

### Step 1: Identify Affected Features

Check which features were changed:

```bash
git diff --name-only HEAD~1 | grep -E "Features/|features/" | head -20
```

Map changed files to feature names.

### Step 2: Read Current Spec

For each affected feature, read:
```
feature_docs/feature-spec-[feature-name].md
```

If no spec exists, note this — you can create one using `feature-spec-feature-example.md` as the template.

### Step 3: Read Changed Code

Read the modified code files to understand what changed:
- Entity files → check for field changes
- Validator files → check for rule changes
- Service files → check for business logic and architecture changes
- Controller files → check for endpoint/auth changes
- Frontend components → check for UI behavior changes
- Redux slices → check for state changes

### Step 4: Update the Spec

Update only the sections that are stale:

| Code Change | Spec Section to Update |
| ----------- | ---------------------- |
| Entity field added/removed/changed | Entity Fields table |
| Validation rule changed | Validation Rules |
| Business logic changed | Business Rules |
| Endpoint added/changed/auth changed | API Endpoints / Authorization |
| UI behavior changed | Frontend UI Description |
| Redux state changed | Redux UI State |
| Significant architectural decision made | Architecture Decisions |
| Data flow changed | Data Flow |
| Design philosophy changed | Core Values & Principles |

When writing architecture sections:

**Core Values & Principles:**
- Core Values = the "why" — design philosophies and goals (e.g., "Server is source of truth")
- Principles = the "how" — concrete technical rules the feature follows (e.g., "`IsCompleted` only changes via `/toggle`")

**Architecture Decisions** (minimum 2 per feature):
```
### [Decision Title]
**Decision**: [What was decided]
**Alternatives Considered**: [What else was evaluated]
**Rationale**: [Why this was chosen — trade-offs, constraints, benefits]
```
Focus on decisions a new developer would question — "why not X instead?"
Mark superseded decisions with **[Superseded]** prefix rather than deleting them.

**Data Flow:**
- Required: a Mermaid `sequenceDiagram` showing components, data direction, and key operations
- Required: prose walkthrough explaining each step — a developer must understand the full flow without reading code
- For complex features, include multiple diagrams (create flow, list flow, toggle flow, etc.)

Update the `Last Updated` date at the top of the spec.

### Step 5: Report

Tell the user what was updated:
- Which spec file(s) were modified
- Which sections were changed and why
- Any specs that should exist but don't

### Step 6: Spec Consistency Check

After updating all sections, verify cross-section consistency (non-blocking — report warnings but do not refuse to complete):

1. **Entity → Validation Rules**: Every field in the Entity Fields table appears in at least one Validation Rule, or has "no validation" explicitly noted in its Notes column.

2. **API Endpoints → Acceptance Scenarios**: Every API endpoint in the API Endpoints table appears in at least one Acceptance Scenario in Business Rules, or Business Rules explicitly says "none — standard CRUD".

3. **Redux UI State → Frontend UI Description**: Every field listed in Redux UI State (section 10) has a corresponding interaction described in Frontend UI Description (section 9). Orphaned Redux fields with no UI element are flagged.

4. **File Locations → API Endpoints**: The File Locations section lists a controller file for every endpoint that was added or modified. Missing controller or service entries are flagged.

Include any warnings in the Step 5 report under a **"⚠ Consistency Warnings"** heading. If no issues are found, omit the heading entirely.

These checks are **warnings only** — the agent always finishes its updates. The workflow is never blocked by consistency issues.
