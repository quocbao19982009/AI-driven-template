---
name: add-migration
description: Create and apply an EF Core migration. Shows the migration file for review before applying.
argument-hint: "[MigrationName]"
disable-model-invocation: true
allowed-tools: "Bash, Read"
---

# Add EF Core Migration

Create a new migration named `$ARGUMENTS` and optionally apply it.

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

### 3. Ask Before Applying

Ask the user: "The migration looks correct. Apply it to the database with `dotnet ef database update`?"

Only proceed if the user confirms.

### 4. Apply the Migration

```bash
cd backend && dotnet ef database update
```

### 5. Remind About API Sync

After applying:

> **Reminder:** If this migration changed entity fields that affect the API, run `npm run api:sync` (or `/api-sync`) from the repo root before working on the frontend.
