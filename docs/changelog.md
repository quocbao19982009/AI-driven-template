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
