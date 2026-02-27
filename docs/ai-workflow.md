# AI Workflow Guide

This guide documents how to use AI effectively with this template.
Follow the order of steps exactly — skipping the API sync step is the most common mistake.

---

## The Golden Rule

**Backend first → sync → frontend.**

The frontend is generated from the backend's Swagger spec. If you ask AI to build the frontend
before syncing, it will either invent the wrong imports or refuse. Always sync in between.

---

## Workflow 1: Add a New Feature End-to-End

This is the primary workflow. Use it whenever you need a new entity with full CRUD.

### Step 1 — Fill in the feature spec

Copy `feature_docs/_templates/feature-spec-template.md` into a new feature subfolder:

```
mkdir feature_docs/products
cp feature_docs/_templates/feature-spec-template.md feature_docs/products/feature-spec-products.md
```

Open `feature_docs/products/feature-spec-products.md` and fill in every section:
entity fields, API endpoints, validation rules, UI description, Redux state.

The more detail you provide, the fewer revision cycles you need.

#### Resolving incomplete specs
If the spec has `<!-- TODO: -->` or `[NEEDS CLARIFICATION: ]` markers, both scaffold skills
will **refuse to proceed**. Run `/clarify-spec [feature-name]` to resolve them interactively,
or edit the spec file directly and remove the markers.

---

### Step 2 — Scaffold the backend

Hand the spec to AI:

```
Scaffold the Products feature backend following CLAUDE.md conventions.
Use feature_docs/products/feature-spec-products.md as the source of truth.
```

**What AI will generate:**

- `Features/Products/Product.cs` — entity inheriting `BaseEntity`
- `Features/Products/ProductDtos.cs` — request/response records
- `Features/Products/ProductsValidator.cs` — FluentValidation rules
- `Features/Products/IProductsRepository.cs` + `ProductsRepository.cs`
- `Features/Products/IProductsService.cs` + `ProductsService.cs`
- `Features/Products/ProductsController.cs`
- Registration lines for `Program.cs`

**After AI finishes — run yourself:**

```bash
cd backend
dotnet ef migrations add AddProductEntity
dotnet ef database update
```

Verify the migration file looks correct before applying.

---

### Step 3 — Sync the API contract

Run this from the **repo root** (not from frontend/ or backend/):

```bash
npm run api:sync
```

This does two things:

1. Builds the backend and exports `backend/swagger.json`
2. Runs Orval to regenerate `frontend/src/api/generated/`

You should see new files appear under `frontend/src/api/generated/products/`.
If the folder doesn't appear, the backend didn't build — fix errors before continuing.

---

### Step 4 — Scaffold the frontend

```
Scaffold the Products frontend feature following CLAUDE.md conventions.
Use feature_docs/products/feature-spec-products.md for the UI description and Redux state.
The Orval hooks are already generated in api/generated/products/.
```

**What AI will generate:**

- `features/products/components/` — page, table/UI components, dialogs
- `features/products/hooks/` — pagination hook, form hook
- `features/products/store/products-slice.ts` — Redux UI state
- `features/products/index.ts` — barrel export
- `routes/products/index.tsx` — thin route file

**After AI finishes — do manually:**

1. Register the Redux slice in `store/index.ts`
2. Add a navigation link in `components/layout/app-layout.tsx` (use `t("nav.products")`)
3. Add translation keys for the new feature in `frontend/src/locales/en.json` and `frontend/src/locales/fi.json`

**i18n requirement for every new feature:**

All hardcoded UI strings in components must use `t()` from `useTranslation()`. Add a top-level namespace for each new feature in both locale files:

```json
// en.json
"products": {
  "title": "Products",
  "description": "Manage your products.",
  "newProduct": "New Product",
  "table": { "id": "ID", "name": "Name", "createdAt": "Created At", "empty": "No products found." },
  "form": { "createTitle": "...", "editTitle": "...", ... },
  "delete": { "title": "...", "description": "..." },
  "toast": { "created": "...", "createError": "...", ... }
}
```

Mirror the same keys with translated values in `fi.json`. Use the `features` namespace in `en.json` / `fi.json` as the reference pattern.

---

## Workflow 2: Add a Field to an Existing Entity

### Step 1 — Update the feature spec first

Open `feature_docs/products/feature-spec-products.md` and add the new field to the **Fields** table,
**Validation Rules**, and **Form fields** sections before touching any code.
The spec is the source of truth — keep it current so future AI prompts stay accurate.

### Step 2 — Scaffold the backend change

```
Add a Description field (string, optional, max 500 chars) to the Product entity.
The spec has been updated in feature_docs/products/feature-spec-products.md.
Follow CLAUDE.md conventions.
```

**Order AI must follow:**

1. Add property to `Product.cs`
2. Add property to DTOs in `ProductDtos.cs`
3. Update mapping in `ProductsService.cs`
4. Add validation rule in `ProductsValidator.cs`

**After AI finishes:**

```bash
cd backend
dotnet ef migrations add AddProductDescription
dotnet ef database update
npm run api:sync          # from repo root — regenerates Orval output
```

### Step 3 — Update the frontend

```
The Description field has been added to the backend and api:sync has been run.
Update the Products frontend form and table to include it.
Refer to feature_docs/products/feature-spec-products.md for the field details.
```

If the new field adds a new label or placeholder, add the corresponding keys to both `en.json` and `fi.json` under the feature's namespace (e.g. `products.form.descriptionLabel`).

---

## Workflow 3: Fix a Bug

### Step 1 — Developer: write the prompt with full context

Do not just say "fix the bug" — AI will guess at the wrong file.
Provide the error, stack trace, and the relevant file upfront:

```
Bug: Creating a product returns 500.
Error from the API: "Object reference not set to an instance of an object"
Stack trace: [paste here]
Relevant file: Features/Products/ProductsService.cs
```

### Step 2 — AI: diagnose and fix the code

AI reads the relevant files, identifies the root cause, and applies the fix.

### Step 3 — Developer: verify the fix

Run the app or tests to confirm the bug is resolved before moving on.

### Step 4 — AI: update the spec if behavior changed

If the fix involved a logic change, a new validation rule, or a UI behavior correction,
ask AI to update the spec in the same session:

```
The bug is fixed. Review what changed and update feature_docs/products/feature-spec-products.md
to reflect any logic, validation, or UI behavior that was corrected.
```

**What warrants a spec update (AI judges this):**

| Change                                     | Update which section                     |
| ------------------------------------------ | ---------------------------------------- |
| A business rule was wrong or missing       | Business Rules                           |
| A validation rule was incorrect            | Validation Rules                         |
| A field behaves differently than described | Entity Fields or Frontend UI Description |
| An endpoint was missing or had wrong auth  | API Endpoints / Authorization            |
| UI state was added to fix a UX issue       | Redux UI state                           |

**What does not need a spec update:**

- Pure code quality fixes (null check, missing `AsNoTracking`, wrong HTTP status code)
- Refactoring that doesn't change behavior

---

## Workflow 4: Write Tests for a Feature

```
Write unit tests for ProductsService following the test patterns in the
Backend test project. Cover: Create (valid), Create (duplicate name),
GetById (found), GetById (not found), Delete (exists).
```

Point AI at specific scenarios. "Write tests for Products" produces shallow output.
"Write tests for these 5 scenarios" produces useful tests.

---

## Workflow 5: Change a Coding Style Rule

Coding style rules live in `docs/coding-style.md`. This is the single source of truth — the AI reads it and follows it on every task.

### Step 1 — Edit the rule

Open `docs/coding-style.md` and change, add, or remove the rule.

### Step 2 — Commit it

```bash
git add docs/coding-style.md
git commit -m "Update coding style: [describe what changed]"
```

### Step 3 — Done

From the next session onward, the AI follows the updated rules. No other files need to change unless the old style violated conventions that are also checked in the PR reviewer — in that case, update `docs/coding-style.md` only (the pr-reviewer agent reads it automatically).

---

## Workflow 6: Clarify an Incomplete Spec

Use when `/scaffold-feature` refused because of unresolved markers.

### Step 1 — Run the clarify skill
```
/clarify-spec [feature-name]
```

### Step 2 — Answer questions in batches
The skill finds all markers, asks up to 3 targeted questions at a time,
updates the spec with your answers, and repeats until no markers remain.

### Step 3 — Proceed to scaffold
When complete, the skill confirms: "Spec is complete — run `/scaffold-feature [feature-name]`."

---

## Prompting Tips

### Be specific about files

Instead of:

> "Fix the validation"

Write:

> "Fix validation in `Features/Products/ProductsValidator.cs` — the Price rule should reject zero, currently it doesn't"

### Reference CLAUDE.md conventions explicitly when needed

> "Add pagination to ProductsRepository. Follow the same pattern as UsersRepository.GetAllAsync — use AsNoTracking and OrderBy before Skip/Take."

### Tell AI what not to change

> "Update the ProductDto to add the Description field. Do not change the controller or any other file."

### Batch related backend changes, then sync once

Instead of syncing after every small change, batch all backend changes in one session,
then run `npm run api:sync` once before switching to the frontend.

---

## Common Mistakes

| Mistake                                                   | Result                                                 | Fix                                                 |
| --------------------------------------------------------- | ------------------------------------------------------ | --------------------------------------------------- |
| Asking AI for frontend code before `api:sync`             | Wrong or invented Orval imports                        | Always sync first                                   |
| Manually editing `api/generated/`                         | Changes overwritten on next sync                       | Never edit generated files                          |
| Skipping `OrderBy` in paginated queries                   | Non-deterministic page results                         | Always `.OrderBy()` before `.Skip()/.Take()`        |
| Putting new state in Redux instead of React Query         | Stale data, double source of truth                     | Server data → React Query, UI state → Redux         |
| Forgetting to register service/repository in `Program.cs` | 500 on all endpoints for that feature                  | AI should do this — verify it did                   |
| Forgetting to register Redux slice in `store/index.ts`    | Redux state silently missing                           | Do this manually after frontend scaffold            |
| Hardcoding UI strings instead of using `t()`              | Strings not translatable, i18n Ally shows missing keys | Always use `useTranslation` + `t()` in components   |
| Adding translation keys to only one locale file           | App crashes or falls back to key name in Finnish       | Always update both `en.json` and `fi.json` together |
| Running `/scaffold-feature` on a spec with TODO markers   | Skill refuses — lists specific unresolved markers      | Resolve with `/clarify-spec` or edit spec directly  |
