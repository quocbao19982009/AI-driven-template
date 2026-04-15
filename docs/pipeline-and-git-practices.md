# Pipeline & Git Practices

This document covers the CI/CD pipeline, local git hooks, and the conventions that connect them.

---

## Git Hooks (Local)

Hooks live in `.githooks/` and are shared with the team via source control. They run automatically if you have configured git to use them:

```bash
git config core.hooksPath .githooks
```

Run this once after cloning. Without it, hooks are silently skipped.

### pre-push

Runs every time you do `git push`. Two checks:

| Check | What it does | On failure |
|---|---|---|
| Format check | Runs `npm run format:check` | Blocks the push — run `npm run format` then re-push |
| api:sync reminder | Detects if `.cs`/`.csproj` files changed but `frontend/src/api/generated/` was not updated | Prints a warning — push still proceeds |

The api:sync check is a **warning, not a block** — not every backend change touches an endpoint. Use your judgement: if you added or modified a DTO or controller endpoint, run `npm run api:sync` and commit the result before pushing.

To bypass hooks in an emergency:

```bash
git push --no-verify
```

Do not make this a habit — CI will catch what the hook missed.

---

## CI Pipeline (GitHub Actions)

Defined in `.github/workflows/ci.yml`. Runs on every push to `main` and every pull request targeting `main`.

### Jobs

```
push / PR
├── backend          (always)
├── frontend         (always)
└── dependency-review  (PRs only)
```

#### Backend job

| Step | Purpose |
|---|---|
| Restore | `dotnet restore` — downloads NuGet packages |
| Vulnerability scan | `dotnet list package --vulnerable --include-transitive` — fails if any known CVE found in direct or transitive NuGet packages |
| Build | `dotnet build --configuration Release` |
| Test | `dotnet test` |

#### Frontend job

| Step | Purpose |
|---|---|
| Install | `npm ci` — clean install from lockfile |
| Vulnerability scan | `npm audit --audit-level=high --omit=dev` — fails if any high/critical CVE found in production dependencies |
| Lint | `npm run lint` |
| Test | `npm run test:run` |
| Build | `npm run build` |

#### Dependency Review job (PRs only)

Runs on every pull request. Uses `actions/dependency-review-action` to scan the dependency delta introduced by the PR against the GitHub Advisory Database.

- Fails the PR if any added package has a **high or critical** CVE
- Fails the PR if any added package uses a **copyleft license** (GPL-2.0, GPL-3.0, AGPL-3.0)
- Posts an inline summary comment on the PR

This is complementary to the audit steps above — the audit steps scan the full existing tree, the dependency review scans only what the PR adds.

---

## Dependabot (Automated Dependency Updates)

Configured in `.github/dependabot.yml`. Opens PRs automatically every Monday for outdated packages.

| Ecosystem | Directory | Groups |
|---|---|---|
| npm | `/frontend` | tanstack, radix, testing, vite-toolchain, i18n |
| nuget | `/backend` | microsoft-aspnetcore, serilog, swashbuckle, test-packages |
| github-actions | `/` | (ungrouped, max 5 PRs) |

Groups consolidate related packages into a single PR (e.g., all `@tanstack/*` packages update together). React major version bumps are ignored — these require coordinated manual updates.

Dependabot PRs trigger the full CI pipeline, including vulnerability scans.

---

## Supply Chain Risk Summary

| Threat | Mitigation |
|---|---|
| Known CVE in a NuGet package | `dotnet list package --vulnerable` in backend CI job |
| Known CVE in a production npm package | `npm audit --audit-level=high --omit=dev` in frontend CI job |
| PR introduces a new vulnerable package | Dependency Review Action blocks the PR |
| PR introduces a copyleft-licensed package | Dependency Review Action blocks the PR |
| Outdated packages accumulating over time | Dependabot weekly PRs |
| CI action pins going stale | Dependabot `github-actions` ecosystem |

---

## Branch & PR Conventions

- `main` is the only long-lived branch — it is always deployable
- All work is done on short-lived feature branches and merged via PR
- PRs require CI to be green before merging
- Squash merge is preferred to keep `main` history linear and readable
- Commit messages should describe *why*, not just *what*

---

## api:sync Discipline

`npm run api:sync` regenerates `frontend/src/api/generated/` from the backend's OpenAPI spec. The generated files must be committed alongside any backend change that affects endpoints or DTOs.

**Rule:** If your PR touches `.cs` files and changes an endpoint or DTO shape, it must also include updated `api/generated/` files.

The pre-push hook will remind you if it detects backend changes without a corresponding generated file update. CI does not enforce this automatically — it is a developer responsibility caught at code review.
