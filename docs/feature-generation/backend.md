# Backend Feature Generation Guide

> Referenced by CLAUDE.md. Describes the steps, layer pattern, and exception handling for scaffolding a new backend feature.

---

## Layer Pattern

Each feature lives in `backend/src/Backend.Api/Features/<FeatureName>/` as a flat folder (no subfolders):

```
Feature.cs                  → EF Core entity (extends BaseEntity)
FeatureDtos.cs              → Request/response records
FeatureValidator.cs         → FluentValidation rules
IFeatureRepository.cs       → Repository interface
FeatureRepository.cs        → EF Core implementation
IFeatureService.cs          → Service interface
FeatureService.cs           → Business logic + DTO mapping
FeatureController.cs        → HTTP layer
```

---

## Steps

### 1. Entity (`Feature.cs`)

- Extend `BaseEntity` — do NOT redeclare `Id`, `CreatedAt`, `UpdatedAt`
- Nullable reference types on: fields that are optional or support future OAuth-style null states
- Add `DbSet<Feature> Features` to `ApplicationDbContext`

### 2. DTOs (`FeatureDtos.cs`)

- `CreateFeatureRequest` / `UpdateFeatureRequest` — inbound records (no `Id`)
- `FeatureDto` — outbound record (includes `Id`, `CreatedAt`, `UpdatedAt`)
- Never expose `PasswordHash` or internal fields in DTOs

### 3. Validator (`FeatureValidator.cs`)

- Inherit `AbstractValidator<CreateFeatureRequest>` and `AbstractValidator<UpdateFeatureRequest>`
- Mirror backend validation constraints (max length, required, format)

### 4. Repository

`IFeatureRepository.cs`:
```csharp
Task<PagedResult<Feature>> GetAllAsync(int page, int pageSize, CancellationToken ct);
Task<Feature?> GetByIdAsync(int id, CancellationToken ct);
Task<Feature> CreateAsync(Feature entity, CancellationToken ct);
Task<Feature> UpdateAsync(Feature entity, CancellationToken ct);
Task DeleteAsync(Feature entity, CancellationToken ct);
```

`FeatureRepository.cs`:
- All read-only queries **must** use `.AsNoTracking()`
- All paginated queries **must** use `.OrderBy()` before `.Skip()/.Take()`

### 5. Service

`FeatureService.cs`:
- Internal static `MapToDto(Feature entity)` method — never inline mapping in multiple places
- Throw `NotFoundException("Feature", id)` for missing entities — never return null from public methods
- Throw `ValidationException(errors)` for domain rule violations
- Pass `CancellationToken` through every async call

### 6. Controller

- Return `ApiResponse<T>` from every action — never return a raw DTO
- Use `[HttpPost]` → 201 Created for create, `[HttpPut]` → 200, `[HttpDelete]` → 204 NoContent
- Pass `CancellationToken cancellationToken = default` to every action

### 7. Register in `Program.cs`

```csharp
builder.Services.AddScoped<IFeatureRepository, FeatureRepository>();
builder.Services.AddScoped<IFeatureService, FeatureService>();
```

### 8. Migration

```bash
dotnet ef migrations add Add<FeatureName>Entity
```

Then tell the user to run `dotnet ef database update` — the AI must never run this command.

---

## Exception Handling

| Situation | What to throw | Maps to |
|---|---|---|
| Entity not found by ID | `throw new NotFoundException("Feature", id)` | 404 |
| Domain rule violated | `throw new ValidationException(errors)` | 400 |
| Unauthorized access | `throw new UnauthorizedAccessException(message)` | 401 (via `ErrorHandlingMiddleware`) |

`ErrorHandlingMiddleware` catches all exceptions and maps them to `ApiResponse<T>.Fail(message)` with the correct status code. Controllers do NOT need try/catch for these types.

---

## Key Infrastructure (do not reinvent)

| Type | Location | Usage |
|---|---|---|
| `BaseEntity` | `Common/Models/BaseEntity.cs` | `class Feature : BaseEntity` |
| `ApiResponse<T>` | `Common/Models/ApiResponse.cs` | `ApiResponse<FeatureDto>.Ok(dto)` |
| `PagedResult<T>` | `Common/Models/PagedResult.cs` | Return from list endpoints |
| `NotFoundException` | `Common/Exceptions/NotFoundException.cs` | `throw new NotFoundException("Feature", id)` |
| `ValidationException` | `Common/Exceptions/ValidationException.cs` | `throw new ValidationException(errors)` |

---

## Reference Implementation

See `Features/_FeatureTemplate/` for the canonical example of all 8 files.
