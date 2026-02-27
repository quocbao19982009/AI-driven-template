---
name: clarify-spec
description: Iteratively resolve incomplete sections in an existing feature spec. Finds all TODO and NEEDS CLARIFICATION markers, asks targeted questions (max 3 at a time), and updates the spec until it is fully complete.
argument-hint: "[feature-name]"
disable-model-invocation: true
allowed-tools: "Read, Write, Edit, Glob"
---

# Clarify Feature Specification

Resolve all unresolved markers in `feature_docs/feature-spec-$ARGUMENTS.md`.

## Steps
1. Read the spec and collect all markers
2. Batch markers into question groups (max 3 per message)
3. Ask the first batch — wait for answers
4. Update the spec with answers, removing the markers
5. Re-run self-check — repeat from step 2 if markers remain
6. Confirm completion when clean

---

## Phase 1 — Read and Collect Markers

Read `feature_docs/feature-spec-$ARGUMENTS.md` in full.

If it does not exist, stop:
> "No spec found. Create one with `/create-spec $ARGUMENTS`."

Scan the full text for:
- `<!-- TODO:`
- `[NEEDS CLARIFICATION:`

For each marker, record: marker text (verbatim), nearest preceding `##`/`###` heading, marker type.

If NO markers found, stop:
> "Spec is clean — no unresolved markers. Run `/scaffold-feature $ARGUMENTS` to proceed."

---

## Phase 2 — Batch and Ask (max 3 questions per message)

Group markers by section. Select up to 3 from the top (prioritize Entity → Endpoints → Business Rules → Authorization → UI).

Question templates by location:

| Marker location | Question |
|---|---|
| Entity / Fields | "What fields does [FeatureName] need? For each: C# type, required/optional, constraints." |
| Relationships | "Does [FeatureName] relate to another entity? Which one, and which side holds the FK?" |
| API Endpoints auth | "Should any endpoints require authentication or a role? Or are all public?" |
| Business Rules / Scenarios | "What are the edge cases or failure scenarios? (duplicates, missing required fields, etc.)" |
| Frontend UI | "What should the UI look like? Standard table + form dialog, or something different?" |
| Authorization | "Is the feature public, auth-required, or role-restricted?" |
| Other | Quote the marker verbatim and ask for the answer. |

Ask all batch questions in **one single message**.

---

## Phase 3 — Update the Spec

For each answered question:
1. Find the marker in the spec
2. Replace it (and its placeholder line if present) with the actual answer, formatted for the section:
   - Entity fields → row in the Fields table
   - UI description → prose in Description subsection
   - Endpoint auth → update Auth Required column
   - Acceptance scenarios → new `**Scenario: ...**` block
   - Authorization → role/auth requirement or "none"
3. Leave no fragment of the original marker in the file

Update `Last Updated` date at the top to today's date.

---

## Phase 4 — Self-Check After Each Batch

Re-run the full self-check:
```
- [ ] Every field in Entity has C# type, required/optional, and at least one constraint
- [ ] Every endpoint has an auth status (yes/no, not blank)
- [ ] Business Rules has at least one Acceptance Scenario (or explicitly "none — standard CRUD")
- [ ] Frontend UI description is specific enough two developers would build the same UI
- [ ] Authorization section is not blank — write "none" or state the requirement
```

For any failed check, append the appropriate marker to that section.

Scan full file again for remaining markers (pre-existing + newly added).

- If markers remain → back to Phase 2 with remaining collection
- If no markers remain → proceed to Phase 5

---

## Phase 5 — Confirm Completion

> **Spec is complete.**
>
> `feature_docs/feature-spec-$ARGUMENTS.md` has no unresolved markers.
>
> Next step: run `/scaffold-feature $ARGUMENTS` to generate the backend.
