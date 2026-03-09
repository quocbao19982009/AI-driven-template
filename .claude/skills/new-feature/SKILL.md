---
name: new-feature
description: Walk through the full end-to-end workflow to implement a new feature — from spec creation through quality check.
argument-hint: "[feature-name]"
allowed-tools: "create-spec, clarify-spec, scaffold-feature, add-migration, api-sync, scaffold-feature-frontend, quality-check"
context: fork
---

# New Feature: $ARGUMENTS

Walk me through the full end-to-end workflow to implement the `$ARGUMENTS` feature. Execute each step sequentially, waiting for confirmation before proceeding to the next.

## Step 1: Create Feature Spec

Run `/create-spec $ARGUMENTS` to create the feature specification file.

## Step 2: Clarify Spec (if needed)

Review the spec for any `<!-- TODO:` or `[NEEDS CLARIFICATION:` markers. If any exist, run `/clarify-spec $ARGUMENTS` to resolve them interactively. Skip this step if the spec is already complete.

## Step 3: Scaffold Backend

Run `/scaffold-feature $ARGUMENTS` to generate the backend entity, DTOs, validator, repository, service, controller, and tests.

## Step 4: Create and Apply Migration

Run `/add-migration $ARGUMENTS` to create the EF Core migration and apply it to the database.

## Step 5: Sync API Contract

Run `/api-sync` to regenerate the frontend Orval hooks from the updated Swagger spec.

## Step 6: Scaffold Frontend

Run `/scaffold-feature-frontend $ARGUMENTS` to generate the frontend page, table, dialogs, hooks, Redux slice, route, navigation, and translations.

## Step 7: Quality Check

Run `/quality-check` to verify everything builds, lints, type-checks, and passes tests.

---

After all steps complete, provide a final summary of all files created and modified.
