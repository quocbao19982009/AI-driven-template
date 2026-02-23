---
name: create-spec
description: Create a new feature specification file from the template. Guides you through filling in entity fields, endpoints, validation, and UI description.
argument-hint: "[feature-name]"
disable-model-invocation: true
allowed-tools: "Read, Write, Edit, Glob"
---

# Create Feature Specification

Create a new feature specification for `$ARGUMENTS`.

## Steps

1. **Read the template and example:**
   - Read `feature_docs/feature-spec-template.md` for the structure
   - Read `feature_docs/feature-spec-feature-example.md` for a filled-in reference

2. **Create the spec file:**
   - Create `feature_docs/feature-spec-$ARGUMENTS.md` based on the template
   - Replace all `[FeatureName]` placeholders with the proper PascalCase entity name
   - Replace all `[features]` route placeholders with the kebab-case plural
   - Set `Last Updated` to today's date

3. **Pre-fill if details were provided:**
   - If the user included field descriptions, endpoints, or business rules in their message, fill those sections in
   - Otherwise, leave the placeholder sections with clear TODO markers for the user to fill

4. **Remind the user:**
   - Tell them to fill in every section before running `/scaffold-feature $ARGUMENTS`
   - The more detail in the spec, the fewer revision cycles needed
   - The spec file is the **source of truth** for the feature — it must stay in sync with code
