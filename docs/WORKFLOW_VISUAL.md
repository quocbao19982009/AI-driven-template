# AI Workflow Visual Guide

> Open VSCode Markdown preview (`Ctrl+Shift+V`) to see rendered diagrams.
> Requires [Markdown Preview Mermaid Support](https://marketplace.visualstudio.com/items?itemName=bierner.markdown-mermaid) extension.

**Color key:** 🔵 Blue = Developer action · 🟢 Green = AI action · 🟣 Purple = Files changed · 🟠 Orange = Automated pipeline · 🟡 Yellow = Result

---

## Overview — Which Workflow?

```mermaid
flowchart TD
    classDef dev fill:#dbeafe,stroke:#3b82f6,color:#1e3a8a
    classDef ai fill:#dcfce7,stroke:#22c55e,color:#14532d

    Dev(["👨‍💻 Developer"]) --> A{"What do I need?"}

    A --> B["Build a brand new feature e.g. Products, Orders, Invoices"]:::dev
    A --> C["Add a field to existing feature e.g. add Description to Products"]:::dev
    A --> D["Something broke — fix a bug"]:::dev
    A --> E["Change a coding style rule"]:::dev
    A --> F["Branch is ready — review before merge"]:::dev

    B --> WF1(["→ Workflow 1: Feature Scaffold"]):::ai
    C --> WF2(["→ Workflow 2: Field Addition"]):::ai
    D --> WF3(["→ Workflow 3: Bug Fix"]):::ai
    E --> WF4(["→ Workflow 4: Change Coding Style"]):::dev
    F --> WF5(["→ Workflow 5: PR Review"]):::ai
```

---

## Workflow 1 — Build a New Feature End-to-End

```mermaid
flowchart TD
    classDef dev fill:#dbeafe,stroke:#3b82f6,color:#1e3a8a
    classDef ai fill:#dcfce7,stroke:#22c55e,color:#14532d
    classDef files fill:#ede9fe,stroke:#7c3aed,color:#3b0764
    classDef pipeline fill:#fed7aa,stroke:#f97316,color:#7c2d12
    classDef result fill:#fef9c3,stroke:#eab308,color:#713f12

    subgraph STEP1["STEP 1 — Create Feature Spec"]

    spacer[" "]:::hidden
        direction LR
        D1["🧑 DEVELOPER Runs: /create-spec products Fills in: entity fields, endpoints, validation rules, UI description, Redux state"]:::dev
        A1["🤖 AI Copies feature-spec-template.md Pre-fills sections from description Asks clarifying questions if vague"]:::ai
        F1["📁 FILES CHANGED NEW: feature_docs/feature-spec-products.md"]:::files
        D1 --> A1 --> F1
    end

    subgraph STEP2["STEP 2 — Scaffold Backend"]
        direction LR
        D2["🧑 DEVELOPER Runs: /scaffold-feature products"]:::dev
        A2["🤖 AI Reads: feature-spec-products.md Reads: Features/_FeatureTemplate/ (8 files) Generates complete backend layer Registers services in Program.cs"]:::ai
        F2["📁 FILES CHANGED NEW: Features/Products/Product.cs NEW: Features/Products/ProductDtos.cs NEW: Features/Products/ProductsValidator.cs NEW: Features/Products/IProductsRepository.cs NEW: Features/Products/ProductsRepository.cs NEW: Features/Products/IProductsService.cs NEW: Features/Products/ProductsService.cs NEW: Features/Products/ProductsController.cs MODIFIED: Program.cs"]:::files
        D2 --> A2 --> F2
    end

    subgraph STEP3["STEP 3 — Create & Apply Migration"]

        direction LR
        N3["⚠️ Developer only — AI does not run this step"]:::result
        D3["🧑 DEVELOPER Runs: dotnet ef migrations add AddProductEntity Reviews migration file to verify it looks correct Runs: dotnet ef database update"]:::dev
        R3["⚠️ AI STOPS HERE Migrations affect the real database Developer must verify before applying"]:::result
        F3["📁 FILES CHANGED NEW: Migrations/[timestamp]_AddProductEntity.cs NEW: Migrations/[timestamp]_AddProductEntity.Designer.cs DB: Products table created in SQL Server"]:::files
        N3 --> D3 --> R3 --> F3
    end

    subgraph STEP4["STEP 4 — API Sync"]


        direction LR
        D4["🧑 DEVELOPER Runs: npm run api:sync (from repo root, not frontend/)"]:::dev
        P4["⚙️ AUTOMATED PIPELINE 1. dotnet build → exports swagger.json 2. Orval reads swagger.json 3. Orval generates TypeScript hooks + Zod schemas"]:::pipeline
        F4["📁 FILES CHANGED NEW: frontend/src/api/generated/products/   getProducts.ts  (list query hook)   getProductsId.ts  (single query hook)   postProducts.ts  (create mutation)   putProductsId.ts  (update mutation)   deleteProductsId.ts  (delete mutation)   schemas.ts  (Zod validation schemas)"]:::files
        D4 --> P4 --> F4
    end

    subgraph STEP5["STEP 5 — Scaffold Frontend"]
        direction LR
        D5["🧑 DEVELOPER Runs: /scaffold-feature-frontend products"]:::dev
        A5["🤖 AI Verifies api/generated/products/ exists Reads: feature-spec-products.md Reads: features/_template-feature/ (10 files) Generates complete frontend layer Registers Redux slice + route + nav link"]:::ai
        F5["📁 FILES CHANGED NEW: features/products/components/products-page.tsx NEW: features/products/components/products-table.tsx NEW: features/products/components/product-form-dialog.tsx NEW: features/products/components/product-delete-dialog.tsx NEW: features/products/hooks/use-product-pagination.ts NEW: features/products/hooks/use-product-form.ts NEW: features/products/store/products-slice.ts NEW: features/products/index.ts NEW: routes/products/index.tsx MODIFIED: store/store.ts MODIFIED: components/layout/app-layout.tsx"]:::files
        D5 --> A5 --> F5
    end

    subgraph STEP6["STEP 6 — Add Translations"]
        direction LR
        D6["🧑 DEVELOPER Runs: /add-translations products Provides real Finnish translation values"]:::dev
        A6["🤖 AI Adds all UI string keys under products namespace Mirrors exact same key structure in both files ⚠️ Reminds developer to supply real Finnish text (AI only adds placeholder Finnish values)"]:::ai
        F6["📁 FILES CHANGED MODIFIED: src/locales/en.json   + products.title, products.description   + products.table.*, products.form.*   + products.delete.*, products.toast.* MODIFIED: src/locales/fi.json   (same keys, Finnish values)"]:::files
        D6 --> A6 --> F6
    end

    subgraph STEP7["STEP 7 — Quality Check"]
        direction LR
        D7["🧑 DEVELOPER Runs: /quality-check"]:::dev
        A7["🤖 AI  runs 7 checks in sequence 1. dotnet build 2. dotnet test 3. cd frontend && npm run lint 4. cd frontend && npx tsc --noEmit 5. cd frontend && npm run test:run 6. Validate en.json keys match fi.json 7. npm run api:check (Orval output committed?)"]:::ai
        R7["📋 RESULT Summary table: PASS / FAIL per check Inline error details for any failures Actionable fix suggestions"]:::result
        D7 --> A7 --> R7
    end

    STEP1 --> STEP2 --> STEP3 --> STEP4 --> STEP5 --> STEP6 --> STEP7
```

---

## Workflow 2 — Add a Field to an Existing Entity

**Example:** Add `Description` (string, optional, max 500 chars) to the Products feature.

```mermaid
flowchart TD
    classDef dev fill:#dbeafe,stroke:#3b82f6,color:#1e3a8a
    classDef ai fill:#dcfce7,stroke:#22c55e,color:#14532d
    classDef files fill:#ede9fe,stroke:#7c3aed,color:#3b0764
    classDef pipeline fill:#fed7aa,stroke:#f97316,color:#7c2d12

    subgraph STEP1["STEP 1 — Update the Feature Spec First"]
        spacer[" "]:::hidden
        direction LR
        D1["🧑 DEVELOPER Opens: feature_docs/feature-spec-products.md Adds Description to: Fields table Adds rule to: Validation Rules Adds field to: Form fields list"]:::dev
        F1["📁 FILES CHANGED MODIFIED: feature_docs/feature-spec-products.md (spec is the source of truth — update first)"]:::files
        D1 --> F1
    end

    subgraph STEP2["STEP 2 — Update the Backend "]



        direction LR
        D2["🧑 DEVELOPER Prompts AI: Add a Description field (string, optional, max 500) to the Products feature. Spec is updated."]:::dev

        A2["🤖 AI  updates in this exact order 1. Product.cs — add Description property 2. ProductDtos.cs — add to CreateProductRequest + ProductResponse 3. ProductsService.cs — update mapping 4. ProductsValidator.cs — add max 500 rule"]:::ai

        F2["📁 FILES CHANGED MODIFIED: Features/Products/Product.cs MODIFIED: Features/Products/ProductDtos.cs MODIFIED: Features/Products/ProductsService.cs MODIFIED: Features/Products/ProductsValidator.cs"]:::files

        D2 --> A2 --> F2
    end

    subgraph STEP3["STEP 3 — Migrate + Sync  (Developer only)"]
        direction LR
        D3["🧑 DEVELOPER dotnet ef migrations add AddProductDescription dotnet ef database update npm run api:sync"]:::dev
        P3["⚙️ PIPELINE Orval adds Description to generated types and regenerates Zod schema"]:::pipeline
        F3["📁 FILES CHANGED NEW: Migrations/[timestamp]_AddProductDescription.cs MODIFIED: api/generated/products/schemas.ts MODIFIED: api/generated/products/postProducts.ts MODIFIED: api/generated/products/putProductsId.ts"]:::files
        D3 --> P3 --> F3
    end

    subgraph STEP4["STEP 4 — Update the Frontend"]
        direction LR
        D4["🧑 DEVELOPER Prompts AI: Description field is added to backend and api:sync ran. Update the Products frontend to include it. See spec for field details."]:::dev
        A4["🤖 AI Adds Description textarea to product-form-dialog.tsx Adds Description column to products-table.tsx Adds translation keys to both locale files"]:::ai
        F4["📁 FILES CHANGED MODIFIED: features/products/components/product-form-dialog.tsx MODIFIED: features/products/components/products-table.tsx MODIFIED: src/locales/en.json   + products.form.descriptionLabel   + products.form.descriptionPlaceholder MODIFIED: src/locales/fi.json   (same keys, Finnish values)"]:::files
        D4 --> A4 --> F4
    end

    STEP1 --> STEP2 --> STEP3 --> STEP4
```

---

## Workflow 3 — Fix a Bug

**Example:** Creating a product returns HTTP 500.

```mermaid
flowchart TD
    classDef dev fill:#dbeafe,stroke:#3b82f6,color:#1e3a8a
    classDef ai fill:#dcfce7,stroke:#22c55e,color:#14532d
    classDef files fill:#ede9fe,stroke:#7c3aed,color:#3b0764
    classDef result fill:#fef9c3,stroke:#eab308,color:#713f12

    subgraph STEP1["STEP 1 — Report the Bug"]
        direction LR
        D1["🧑 DEVELOPER Runs: /fix-issue [description] Example:   Bug: Creating a product returns 500   Error: Object reference not set   Stack: ProductsService.cs line 42   Steps: POST /api/products with valid body"]:::dev
        A1["🤖 AI  parses description Identifies: feature = Products Identifies: layer = Service Identifies: type = NullReferenceException Asks clarifying questions if description is too vague"]:::ai
        D1 --> A1
    end

    subgraph STEP2["STEP 2 — Read Context"]
        direction LR
        A2["🤖 AI  reads (no changes yet) Reads: feature_docs/feature-spec-products.md Reads: Features/Products/ProductsService.cs Reads: Features/Products/ProductsController.cs Reads: CLAUDE.md conventions for backend"]:::ai
        R2["📋 AI understands Current behavior vs intended behavior Which convention may have been violated Root cause hypothesis"]:::result
        A2 --> R2
    end

    subgraph STEP3["STEP 3 — Implement the Fix"]
        direction LR
        A3["🤖 AI  edits the affected file(s) Fix must follow CLAUDE.md: • Add null check before dereferencing • Use NotFoundException not raw exception • Keep CancellationToken in signature • Return ApiResponse, not raw data"]:::ai
        F3["📁 FILES CHANGED MODIFIED: Features/Products/ProductsService.cs   (null check added at line 42) (Only the file causing the bug — nothing else)"]:::files
        A3 --> F3
    end

    subgraph STEP4["STEP 4 — Run Tests"]
        direction LR
        A4["🤖 AI Runs: dotnet test backend/Backend.sln If bug needed a new test case:   Adds test for the null input scenario   to tests/Backend.Tests/Features/Products/"]:::ai
        F4["📁 FILES CHANGED Optional NEW: tests/Backend.Tests/Features/Products/   ProductsServiceTests.cs   (new test: Create_WithNullCategory_ThrowsNotFoundException)"]:::files
        A4 --> F4
    end

    subgraph STEP5["STEP 5 — Update Feature Spec  (only if behavior changed)"]
        direction LR
        A5["🤖 AI  checks: did observable behavior change? If a null category now throws 404 instead of 500 → that IS a behavior change → update spec If just fixed a missing null check → NOT a behavior change → skip spec update"]:::ai
        F5["📁 FILES CHANGED Conditional MODIFIED: feature_docs/feature-spec-products.md   Business Rules: 'Category must exist — returns 404 if not found' (Skipped for pure code-quality fixes)"]:::files
        A5 --> F5
    end

    subgraph STEP6["STEP 6 — Summary"]
        direction LR
        R6["📋 AI REPORTS Root cause: CategoryId was not validated before lookup Files changed: ProductsService.cs (line 42) Tests: dotnet test — 24 passed, 0 failed Spec updated: Yes — Business Rules section  🧑 Developer verifies fix works in browser/Swagger"]:::result
        R6
    end

    STEP1 --> STEP2 --> STEP3 --> STEP4 --> STEP5 --> STEP6
```

---

## Workflow 4 — Change a Coding Style Rule

**Example:** Switch from explicit types to `var` for obvious assignments in C#.

```mermaid
flowchart TD
    classDef dev fill:#dbeafe,stroke:#3b82f6,color:#1e3a8a
    classDef files fill:#ede9fe,stroke:#7c3aed,color:#3b0764
    classDef result fill:#fef9c3,stroke:#eab308,color:#713f12

    subgraph STEP1["STEP 1 — Edit the Style Rule"]
        direction LR
        D1["🧑 DEVELOPER Opens: docs/coding-style.md Changes, adds, or removes the rule Example: change 'Avoid var' → 'Use var when type is obvious'"]:::dev
        F1["📁 FILES CHANGED MODIFIED: docs/coding-style.md"]:::files
        D1 --> F1
    end

    subgraph STEP2["STEP 2 — Commit"]
        direction LR
        D2["🧑 DEVELOPER git add docs/coding-style.md git commit -m 'Update coding style: allow var for obvious types'"]:::dev
        R2["📋 RESULT From the next session onward, AI applies the new rule to all code it writes or reviews"]:::result
        D2 --> R2
    end

    STEP1 --> STEP2
```

---

## Workflow 5 — PR / Code Review

**Example:** Developer finished the Products feature branch and wants a review before merging.

```mermaid
flowchart TD
    classDef dev fill:#dbeafe,stroke:#3b82f6,color:#1e3a8a
    classDef ai fill:#dcfce7,stroke:#22c55e,color:#14532d
    classDef files fill:#ede9fe,stroke:#7c3aed,color:#3b0764
    classDef result fill:#fef9c3,stroke:#eab308,color:#713f12

    subgraph STEP1["STEP 1 — Request Review"]
        direction LR
        D1["🧑 DEVELOPER Prompts: Review my code on the products-feature branch  Optional context to add: • What the branch does • Any areas of concern • Whether to check specific layers only"]:::dev
        A1["🤖 PR REVIEWER AGENT  gathers context git diff main...HEAD --stat git log main...HEAD --oneline  Finds: 12 files changed, +430 -18 Commits: 3 commits on this branch"]:::ai
        D1 --> A1
    end

    subgraph STEP2["STEP 2 — Read All Changed Files"]
        direction LR
        A2["🤖 AI  reads every modified file in full Not just the diff — reads complete file to understand context around each change  Example files read: Features/Products/ProductsService.cs Features/Products/ProductsController.cs features/products/components/products-page.tsx features/products/store/products-slice.ts src/locales/en.json"]:::ai
        R2["📋 AI builds understanding What changed and why Which patterns were used What conventions apply"]:::result
        A2 --> R2
    end

    subgraph STEP3["STEP 3 — Check Project Conventions"]
        direction LR
        A3["🤖 AI  checks each convention from CLAUDE.md"]:::ai
        R3["📋 BACKEND CHECKLIST ✓ Entity inherits BaseEntity ✓ All read queries use AsNoTracking ✓ OrderBy before Skip/Take on paginated queries ✓ All controllers return ApiResponse ✓ CancellationToken passed through full chain ✓ BCrypt used for passwords (not SHA256) ✓ No secrets in appsettings.json ✓ Services registered in Program.cs ✓ Uses NotFoundException / ValidationException  📋 FRONTEND CHECKLIST ✓ All UI text uses t() from useTranslation ✓ Keys exist in BOTH en.json AND fi.json ✓ Server data in React Query, not Redux ✓ useAppDispatch / useAppSelector (not raw hooks) ✓ No manual edits to api/generated/ ✓ cn() used for class merging  📋 CODING STYLE CHECKLIST (docs/coding-style.md) ✓ C# naming: PascalCase methods, _camelCase private fields ✓ File-scoped namespaces in all C# files ✓ No var where type is not obvious ✓ TS naming: PascalCase components, camelCase hooks ✓ Named exports for components and hooks ✓ cn() for Tailwind class merging"]:::result
        A3 --> R3
    end

    subgraph STEP4["STEP 4 — SOLID Principles Review"]
        direction LR
        A4["🤖 AI  evaluates each SOLID principle"]:::ai
        R4["📋 SOLID EVALUATION SRP — ProductsService has one reason to change? OCP — Can add new product types without modifying service? LSP — Repository interface correctly substitutable? ISP — IProductsRepository not bloated with unrelated methods? DIP — Controller depends on IProductsService, not impl?"]:::result
        A4 --> R4
    end

    subgraph STEP5["STEP 5 — Review Report"]
        direction LR
        R5["📋 REVIEW REPORT EXAMPLE  Branch: products-feature  Base: main  Date: 2026-02-20 Summary: Adds full CRUD for Products with pagination and soft delete.  SOLID: SRP PASS · OCP PASS · LSP N/A · ISP PASS · DIP PASS  🔴 Critical:   ProductsController.cs:47 — returns raw ProductDto instead of ApiResponse  🟡 Major:   ProductsRepository.cs:23 — GetAllAsync missing AsNoTracking  🔵 Minor:   products-table.tsx:15 — hardcoded string 'No results' should use t()  Testing: ProductsService has no unit tests — 3 methods uncovered  Overall: NEEDS WORK — fix 1 critical before merging"]:::result
        R5
    end

    subgraph STEP6["STEP 6 — Developer Acts on Findings"]
        direction LR
        D6["🧑 DEVELOPER Fixes critical issue: wraps response in ApiResponse Fixes major issue: adds AsNoTracking Fixes minor: adds t() call with translation key Optionally asks AI to write missing tests"]:::dev
        F6["📁 FILES CHANGED MODIFIED: Features/Products/ProductsController.cs MODIFIED: Features/Products/ProductsRepository.cs MODIFIED: features/products/components/products-table.tsx MODIFIED: src/locales/en.json + fi.json"]:::files
        D6 --> F6
    end

    STEP1 --> STEP2 --> STEP3 --> STEP4 --> STEP5 --> STEP6
```
