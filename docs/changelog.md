# Convention Changelog

> Add an entry here whenever a project-wide convention, skill, or architectural rule changes.
> This file is NOT auto-loaded — read it when you need to understand why a rule exists in its current form.

| Date | Change | Reason |
|---|---|---|
| 2026-02-27 | Added `[NEEDS CLARIFICATION: ...]` inline marker convention alongside `<!-- TODO: ... -->` | Distinguishes "AI can infer a default" from "human must answer before proceeding"; visible in rendered Markdown unlike HTML comments |
| 2026-02-27 | Added completeness gates to `scaffold-feature` and `scaffold-feature-frontend` | Prevents scaffolding from an incomplete spec; markers must be resolved first |
| 2026-02-27 | Added `/clarify-spec` skill | Interactive resolution of unresolved markers before scaffolding; equivalent to Spec-Kit's `/speckit.clarify` |
| 2026-02-27 | Added opt-in `feature-data-model-template.md` | Resource for complex many-to-many or multi-entity features; not mandatory for simple CRUD |
| 2026-02-27 | Documentation audit: fixed Id type (Guid→int), utility paths (cn→lib/utils, useDebounce→hooks/use-debounce), component file naming (PascalCase→kebab-case), spec template added 5 missing sections (Core Values, Architecture Decisions, Data Flow, File Locations, Tests), ai-workflow copy path fixed, feature spec File Locations rewritten to match actual codebase | 18 issues found where docs contradicted actual codebase patterns |
| 2026-03-13 | Added "Convention Changelog" rule to CLAUDE.md | Ensures `docs/changelog.md` is updated at the end of any session that changes a convention, rule, or skill — making changelog maintenance a first-class workflow obligation |
| 2026-03-14 | Added `UserRole` enum to backend key infrastructure table in `backend.md` | Code review found roles were magic strings; enum was added to code but not documented as a convention |
| 2026-03-14 | Added Authorization Pattern section to `backend.md` | Code review found 3 critical auth bugs (privilege escalation, IDOR, self-promotion); documenting the pattern prevents recurrence in future features |
| 2026-03-14 | Added `IsActive` deactivation check convention to `backend.md` | Deactivated users could still log in; convention ensures all auth flows check `IsActive` |
| 2026-03-14 | Fixed spec template section 10 numbering (H3→H2) | "Redux UI State" was nested inside section 9 as an H3 instead of being a standalone H2 section |
| 2026-03-14 | Updated `var` usage rule in `coding-style.md` — relaxed to match actual codebase | Previous rule ("avoid var unless self-evident") contradicted widespread `var` usage throughout the codebase |
| 2026-03-14 | Fixed null-coalescing example in `coding-style.md` from `"User"` string to `UserRole.User` | Stale example referenced magic string after `UserRole` enum was introduced |
| 2026-03-14 | Fixed `store/index.ts` → `store/store.ts` in spec template and feature specs | Path referenced a file that doesn't exist; actual store file is `store/store.ts` |
| 2026-03-14 | Fixed repository return type in `backend.md` feature-generation guide | Guide showed `PagedResult<Feature>` but actual pattern returns `(List<Feature>, int)` tuple; service wraps into `PagedResult<TDto>` |
| 2026-03-14 | Added status legend to `FEATURES.md` | Status values were undefined; AI was guessing inconsistently |
| 2026-03-14 | Deduplicated DB safety rule — CLAUDE.md now references `backend.md` instead of repeating it | Two copies of the same rule would diverge if only one was updated |
| 2026-03-15 | Added PR template; branch protection for main enforced at repo level | Enforce PR-only workflow for all changes |
