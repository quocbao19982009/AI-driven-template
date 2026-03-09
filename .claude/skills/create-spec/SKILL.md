---
name: create-spec
description: Create a new feature specification file from the template. Classifies what is known, asks targeted questions for missing details, then creates the spec with acceptance scenarios and a self-check.
argument-hint: "[feature-name]"
allowed-tools: "Read, Write, Edit, Glob"
---

# Create Feature Specification

Create a new feature specification for `$ARGUMENTS`.

## Argument Normalization (DO THIS FIRST)

Before any other step, normalize `$ARGUMENTS` to **lowercase-kebab-case** (e.g., `Todo` → `todos`, `MeetingRoom` → `meeting-rooms`, `todo` → `todos`). Use this normalized value everywhere `$ARGUMENTS` appears below. If the argument is already singular, pluralize it (add `s`, or apply standard English pluralization). Store the result as `{feature}`.

> Example: `/create-spec Todo` → `{feature}` = `todos`
> Example: `/create-spec meeting-room` → `{feature}` = `meeting-rooms`

## Steps

1. **Classify what is known vs. unknown** (5 categories)
2. **Ask targeted questions if needed** (max 3, single message — skip if all categories are clear)
3. **Read template + example**
4. **Create spec file** with acceptance scenarios pre-populated in Business Rules
5. **Run self-check** and annotate any incomplete sections
6. **Remind user** to fill TODOs before running `/scaffold-feature $ARGUMENTS`

---

## Phase 1 — Classify Known vs. Unknown

Before creating any file, scan the user's request against these 5 categories:

| Category                          | Unclear if…                                                 | Maps to spec section     |
| --------------------------------- | ----------------------------------------------------------- | ------------------------ |
| Entity shape                      | No fields mentioned, or fields lack types/constraints       | Section 1: Entity        |
| Relationships                     | Another entity is mentioned but FK direction is unspecified | Section 1: Relationships |
| Business rules & custom endpoints | Non-CRUD behavior exists but is unspecified                 | Sections 5, 7            |
| Authorization                     | Not stated whether public or auth-required                  | Section 8                |
| Frontend UI                       | Not described, or too vague to build from                   | Section 9                |

Mark each category as **known** or **unclear**.

---

## Phase 2 — Ask Targeted Questions (only if any category is unclear)

If one or more categories are unclear, ask all questions in **one single message** — maximum 3 questions total. Do not ask one question at a time.

If all 5 categories are clear, skip this phase and proceed directly to Phase 3.

**Question templates by category:**

- **Entity shape unclear:** "What fields does [FeatureName] need? For each field, what is its C# type (string, int, bool, DateTime?), is it required or optional, and are there any constraints (max length, allowed values, must be in the future, etc.)?"
- **Relationships unclear:** "Does [FeatureName] belong to or relate to any other entity (e.g., belongs to a User)? If so, which entity, and in which direction does the foreign key go?"
- **Business rules / custom endpoints unclear:** "Are there any non-CRUD actions (e.g., toggle, approve, bulk delete, status transitions)? Any special logic in the service layer beyond saving data?"
- **Authorization unclear:** "Should any endpoints require authentication or specific roles? Or are all endpoints public?"
- **Frontend UI unclear:** "What should the UI look like? Is it a standard table + form dialog, or something different? Describe the key interactions (search, filters, dialogs, etc.)."

Merge overlapping questions. Do not exceed 3.

---

## Phase 3 — Create Spec + Self-Check

### 3a. Read the template and example

- Read `feature_docs/_templates/feature-spec-template.md` for structure
- Read `feature_docs/feature-spec-feature-example.md` for a filled-in reference

### 3b. Create the spec file

- Create `feature_docs/$ARGUMENTS/feature-spec-$ARGUMENTS.md` based on the template (create the subfolder if it doesn't exist)
- Replace all `[FeatureName]` placeholders with the proper PascalCase entity name
- Replace all `[features]` route placeholders with the kebab-case plural
- Set `Last Updated` to today's date
- Pre-fill all sections using information from the user's request (or from Phase 2 answers)
- In the **Business Rules** section, pre-populate `### Acceptance Scenarios` with at minimum:
  - One happy-path scenario per endpoint
  - One failure scenario per validation rule or business rule
  - Leave `<!-- TODO: fill in concrete values -->` on any scenario that needs user-specific detail

### 3c. Self-check before finishing

After writing the file, verify each item:

```
Self-check:
- [ ] Every field in Entity has a C# type, required/optional status, and at least one constraint
- [ ] Every endpoint in API Endpoints has an auth status (yes/no, not blank)
- [ ] Business Rules has at least one Acceptance Scenario (or explicitly says "none — standard CRUD")
- [ ] Frontend UI description is specific enough that two developers would build the same UI
- [ ] Authorization section is not blank — write "none" explicitly or state the auth requirement
```

For any item that fails, use the appropriate marker type:

- Use `<!-- TODO: ... -->` when info is missing but a default can be inferred from context
- Use `[NEEDS CLARIFICATION: ...]` when the info cannot be inferred and requires a human answer

If Authorization is blank, append:
`[NEEDS CLARIFICATION: is any endpoint restricted to authenticated users or specific roles? Write "none" if all endpoints are public.]`

The skill does not block on incomplete sections — it finishes and flags. The spec file is always created.

---

## Phase 4 — Remind User

After the spec file is created and self-check is complete, tell the user:

> Spec created at `feature_docs/$ARGUMENTS/feature-spec-$ARGUMENTS.md`.
>
> Resolve all `<!-- TODO: -->` and `[NEEDS CLARIFICATION: ]` markers before running `/scaffold-feature $ARGUMENTS` — the scaffold skill will refuse while any markers remain. Run `/clarify-spec $ARGUMENTS` to resolve them interactively.
