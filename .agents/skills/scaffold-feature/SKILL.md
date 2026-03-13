---
name: scaffold-feature
description: Scaffold the backend for a new feature from a feature spec file. Creates entity, DTOs, validator, repository, service, controller, and tests. First half of the full-stack workflow.
argument-hint: "[feature-name]"
allowed-tools: "Read, Write, Edit, Bash, Glob, Grep, mcp__context7__resolve-library-id, mcp__context7__query-docs"
context: fork
---

# Scaffold Backend Feature

Scaffold the complete backend for the `$ARGUMENTS` feature.

## Argument Normalization (DO THIS FIRST)

Before any other step, normalize `$ARGUMENTS` to **lowercase-kebab-case plural** (e.g., `Todo` → `todos`, `MeetingRoom` → `meeting-rooms`). Use this normalized value as `{feature}` everywhere `$ARGUMENTS` appears below.

> Example: `/scaffold-feature Todo` → `{feature}` = `todos`

## Prerequisites

### Step P1: Verify the spec file exists

Read the feature spec:

```
feature_docs/$ARGUMENTS/feature-spec-$ARGUMENTS.md
```

If this file does not exist, **stop immediately** and tell the user:

> "No spec found at `feature_docs/$ARGUMENTS/feature-spec-$ARGUMENTS.md`. Create one first with `/create-spec $ARGUMENTS`."

### Step P2: Completeness gate — scan for unresolved markers

After reading the spec, scan its full text for:

- `<!-- TODO:`
- `[NEEDS CLARIFICATION:`

If ANY markers are found, **stop immediately** with this refusal (do NOT proceed to Step 1):

> **Cannot scaffold: the spec has unresolved markers.**
>
> Resolve each of the following before re-running `/scaffold-feature $ARGUMENTS`:
>
> [List every found marker verbatim, one per line, prefixed with the nearest `##` or `###` heading it appears under]
>
> Run `/clarify-spec $ARGUMENTS` to resolve them interactively, or edit `feature_docs/$ARGUMENTS/feature-spec-$ARGUMENTS.md` directly.

Only proceed to Step 1 when no markers remain.

## Step 1: Read the Templates

Read ALL of these files to understand the patterns:

**Backend feature template (8 files):**

- `backend/src/Backend.Api/Features/_FeatureTemplate/Feature.cs`
- `backend/src/Backend.Api/Features/_FeatureTemplate/FeatureDtos.cs`
- `backend/src/Backend.Api/Features/_FeatureTemplate/FeatureValidator.cs`
- `backend/src/Backend.Api/Features/_FeatureTemplate/IFeatureRepository.cs`
- `backend/src/Backend.Api/Features/_FeatureTemplate/FeatureRepository.cs`
- `backend/src/Backend.Api/Features/_FeatureTemplate/IFeatureService.cs`
- `backend/src/Backend.Api/Features/_FeatureTemplate/FeatureService.cs`
- `backend/src/Backend.Api/Features/_FeatureTemplate/FeatureController.cs`

**Backend test template:**

- `backend/tests/Backend.Tests/Features/_FeatureTemplate/FeatureServiceTests.cs`
- `backend/tests/Backend.Tests/Features/_FeatureTemplate/FeatureControllerTests.cs`
- `backend/tests/Backend.Tests/Features/_FeatureTemplate/FeatureValidatorTests.cs`

**Infrastructure references:**

- `backend/src/Backend.Api/Common/Models/BaseEntity.cs`
- `backend/src/Backend.Api/Common/Models/ApiResponse.cs`
- `backend/src/Backend.Api/Common/Models/PagedResult.cs`
- `backend/src/Backend.Api/Program.cs` (to see where to register services)

## Step 2: Generate the Backend Feature

Create all files under `backend/src/Backend.Api/Features/[PascalPluralName]/`:

**Naming conventions (example: feature-name = "products"):**

- Entity: `Product.cs` (singular PascalCase)
- DTOs: `ProductDtos.cs`
- Validator: `ProductsValidator.cs` (plural)
- Repository: `IProductsRepository.cs` + `ProductsRepository.cs` (plural)
- Service: `IProductsService.cs` + `ProductsService.cs` (plural)
- Controller: `ProductsController.cs` (plural)

**Map fields from the spec's Entity Fields table** into:

- Entity properties (with correct C# types)
- DTO records (Create, Update, Response)
- FluentValidation rules
- Service mapping logic

## Step 3: Critical Conventions (MUST FOLLOW)

These are non-negotiable — violating any of these is a bug:

- **Entity** inherits `BaseEntity` — do NOT add Id, CreatedAt, UpdatedAt
- **All read-only queries** use `.AsNoTracking()`
- **All paginated queries** have `.OrderBy()` BEFORE `.Skip()/.Take()`
- **All controller actions** return `ApiResponse<T>` using `ApiResponse<T>.Ok(data)` or `ApiResponse<T>.Fail(message)`
- **All async methods** pass `CancellationToken` through the full chain (controller → service → repository)
- **Password fields** (if any) use `BCrypt.HashPassword` / `BCrypt.Verify` — NEVER SHA256
- **NotFoundException** for missing entities: `throw new NotFoundException("[Entity]", id)`
- **ValidationException** for business rule violations
- No secrets in `appsettings.json`
- Use `AddSwaggerGen()` only — do NOT add `AddOpenApi()`

## Step 4: Generate Tests

Create test files under `backend/tests/Backend.Tests/Features/[PascalPluralName]/`:

- `[Entity]ServiceTests.cs` — test CRUD operations, validation failures, not-found cases
- `[Entity]ControllerTests.cs` — test HTTP responses and delegation to service
- `[Entity]ValidatorTests.cs` — test each validation rule from the spec

## Step 5: Register in Program.cs

Add these lines to `backend/src/Backend.Api/Program.cs` in the service registration section:

```csharp
builder.Services.AddScoped<I[Plural]Repository, [Plural]Repository>();
builder.Services.AddScoped<I[Plural]Service, [Plural]Service>();
```

## Step 6: Stop and Instruct

After generating all files, tell the user:

> **Backend scaffolding complete.** Next steps:
>
> 1. Review the generated files
> 2. Create and apply the migration:
>    ```bash
>    cd backend
>    dotnet ef migrations add [MigrationName from spec]
>    dotnet ef database update
>    ```
> 3. Sync the API contract:
>    ```bash
>    npm run api:sync
>    ```
> 4. Then scaffold the frontend:
>    ```
>    /scaffold-feature-frontend $ARGUMENTS
>    ```

**Do NOT proceed to frontend scaffolding.** The `api:sync` step must run between backend and frontend.
