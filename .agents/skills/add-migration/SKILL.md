---
name: add-migration
description: Create an EF Core migration and show the file for review. Never applies the migration — database updates are done manually by the user only.
argument-hint: "[MigrationName]"
disable-model-invocation: true
allowed-tools: "Bash, Read"
---

# Add EF Core Migration

Create a new migration named `$ARGUMENTS` and show it for review.

## Steps

### 1. Create the Migration

```bash
cd backend && dotnet ef migrations add $ARGUMENTS
```

If this fails, report the error. Common causes:
- Backend doesn't build — fix build errors first
- DbContext has no changes — the entity wasn't added or modified correctly

### 2. Show the Migration File

Read the newly created migration file for the user to review:
- Find it in `backend/src/Backend.Api/Migrations/`
- It will be the most recent file matching `*_$ARGUMENTS.cs`
- Show the `Up()` and `Down()` methods

### 3. Remind the User to Apply Manually

> **Action required (manual):** Run the following command yourself to apply the migration:
> ```bash
> cd backend && dotnet ef database update
> ```
> The AI will never run this command — database updates are the user's responsibility.

### 4. Remind About API Sync

> **Reminder:** If this migration changed entity fields that affect the API, run `npm run api:sync` (or `/api-sync`) from the repo root before working on the frontend.
