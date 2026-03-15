# Contributing to AI-Driven Template

Thank you for your interest in contributing! This guide will help you get started.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) with npm
- [SQL Server](https://www.microsoft.com/en-us/sql-server/) (or SQL Server Express / LocalDB)

### Dev Environment Setup

1. Clone the repo:
   ```bash
   git clone https://github.com/quocbao19982009/AI-drivent-template.git
   cd AI-drivent-template
   ```
2. Follow the setup instructions in [`backend/README.md`](backend/README.md) and [`frontend/README.md`](frontend/README.md).
3. Run both projects together from the root:
   ```bash
   npm run dev
   ```

## Branch Naming

Use descriptive branch names with a prefix:

- `feature/short-description` — new features
- `fix/short-description` — bug fixes
- `docs/short-description` — documentation changes
- `refactor/short-description` — code refactoring

## Pull Request Workflow

1. Fork the repo and create your branch from `main`.
2. Make your changes following the coding standards below.
3. Run quality checks before pushing:
   ```bash
   # Backend
   dotnet build backend/Backend.sln
   dotnet format backend/Backend.sln --verify-no-changes

   # Frontend
   cd frontend
   npm run lint
   npx tsc --noEmit
   npm run test:run
   ```
4. If you changed backend DTOs or endpoints, regenerate the frontend API client:
   ```bash
   npm run api:sync
   ```
5. Open a PR against `main` using the [PR template](.github/pull_request_template.md).
6. Fill in the summary and test plan sections.

## Coding Standards

This project enforces consistent coding style across both backend and frontend. See [`docs/coding-style.md`](docs/coding-style.md) for the full rules.

Key points:
- **Backend**: C# conventions, `ApiResponse<T>` wrapper on all endpoints, `.AsNoTracking()` for read queries
- **Frontend**: No barrel files, all strings through `t()` for i18n, Redux for UI-only state

## Adding a New Feature

This project has a structured feature generation workflow:

1. **Create a feature spec** in `feature_docs/` using the templates in `feature_docs/_templates/`
2. **Backend implementation** — follow [`docs/feature-generation/backend.md`](docs/feature-generation/backend.md)
3. **Frontend implementation** — follow [`docs/feature-generation/frontend.md`](docs/feature-generation/frontend.md)
4. **Checklist** — use [`docs/feature-generation/implementation-checklist.md`](docs/feature-generation/implementation-checklist.md) to verify completeness

## Running Tests

```bash
# Frontend unit tests
cd frontend
npm run test:run

# Frontend tests in watch mode
cd frontend
npm run test
```

## Commit Messages

Write clear, concise commit messages:

- Use the imperative mood: "Add feature" not "Added feature"
- Keep the first line under 72 characters
- Reference issues when applicable: "Fix login redirect (#42)"

Examples:
```
Add user search endpoint with pagination
Fix token refresh race condition
Docs: update backend auth patterns
```

## Translations

When adding or changing visible UI text:

1. Add the key to **both** `frontend/src/locales/en.json` and `frontend/src/locales/fi.json`
2. Never hardcode user-facing strings — always use `t()` from `useTranslation()`

## Questions?

Open an issue for questions or discussion. We're happy to help!
