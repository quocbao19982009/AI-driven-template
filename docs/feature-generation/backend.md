# Backend Feature Generation

## Steps

1. Copy `Features/_FeatureTemplate/` folder
2. Rename "Feature" → your entity name in all files
3. Update entity properties in `<Entity>.cs`
4. Adjust DTOs in `<Entity>Dtos.cs`
5. Add `public DbSet<Entity> Entities => Set<Entity>();` to `ApplicationDbContext.cs`
6. Register in `Program.cs`:
   ```csharp
   builder.Services.AddScoped<IEntitiesRepository, EntitiesRepository>();
   builder.Services.AddScoped<IEntitiesService, EntitiesService>();
   ```
7. Create and apply the migration:
   ```sh
   cd backend && dotnet ef migrations add AddEntityName
   ```

---

## Exception Handling

- **Not found**: `throw new NotFoundException("Entity", id)` → auto-mapped to 404
- **Validation**: `throw new ValidationException(errors)` → auto-mapped to 400 with error list
- **All other errors**: let them bubble up to `ErrorHandlingMiddleware` — do not catch and swallow

Never return `null` from a service when an entity is not found — always throw `NotFoundException`.

---

## Feature Layer Pattern

| Layer | Role |
|-------|------|
| Controller | Thin — handles HTTP, calls Service |
| Service | Business logic, validation, calls Repository |
| Repository | Data access, EF Core queries |
| Entity | Database model, inherits `BaseEntity` |
| DTOs | Request/response models (immutable records) |
| Validator | FluentValidation rules |
