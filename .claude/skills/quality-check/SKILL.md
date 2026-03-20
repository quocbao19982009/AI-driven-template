---
name: quality-check
description: Run full quality checks — backend build, frontend lint, TypeScript check, tests, and translation validation. Use after completing code changes.
allowed-tools: "Bash, Read, Grep, Glob"
context: fork
---

# Quality Check

Run all quality checks sequentially. **Continue through all checks even if one fails** — report everything at the end.

## Checks

### 1. Backend Build

```bash
dotnet build backend/Backend.sln
```

### 2. Backend Tests

```bash
dotnet test backend/Backend.sln
```

### 3. Frontend Lint

```bash
cd frontend && npm run lint
```

### 4. TypeScript Type Check

```bash
cd frontend && npx tsc --noEmit -p tsconfig.app.json
```

### 5. Frontend Tests

```bash
cd frontend && npm run test:run
```

### 6. Translation Validation

Check that every top-level key in `en.json` also exists in `fi.json` and vice versa:

- Read `frontend/src/locales/en.json` and `frontend/src/locales/fi.json`
- Compare top-level keys — report any that exist in one but not the other
- For each shared top-level key, do a deep comparison of nested keys
- Report any translation keys missing from either file

### 7. API Sync Check

```bash
npm run api:check
```

This verifies that Orval-generated files match the current Swagger spec. If this fails, the developer forgot to run `npm run api:sync` after a backend change.

## Output Format

After running all checks, output a summary:

```
## Quality Check Summary

| Check            | Status | Details          |
| ---------------- | ------ | ---------------- |
| Backend Build    | PASS   |                  |
| Backend Tests    | PASS   | 24 passed        |
| Frontend Lint    | FAIL   | 3 errors         |
| TypeScript       | PASS   |                  |
| Frontend Tests   | PASS   | 18 passed        |
| Translations     | FAIL   | 2 keys missing   |
| API Sync         | PASS   |                  |
```

For any FAIL, include the specific errors below the table with file paths and line numbers where applicable.

Categorize failures as:

- **Blocking** — must fix before merging (build failures, test failures, type errors)
- **Warning** — should fix (lint issues, missing translations)
