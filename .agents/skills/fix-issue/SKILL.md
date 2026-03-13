---
name: fix-issue
description: Fix a bug or issue from a description. Identifies affected code, implements the fix, runs tests, and updates the feature spec.
argument-hint: "[issue description]"
allowed-tools: "Read, Write, Edit, Bash, Glob, Grep, mcp__context7__resolve-library-id, mcp__context7__query-docs"
context: fork
---

# Fix Issue

Fix the issue: `$ARGUMENTS`

## Step 1: Understand the Issue

Parse the user's description to identify:

- Which feature area is affected
- Specific files mentioned (if any)
- Whether this is a bug fix, enhancement, or new functionality
- Error messages or stack traces provided

If the description is too vague, ask the user for:

- What is the expected behavior?
- What is the actual behavior?
- Steps to reproduce (if a bug)

## Step 2: Read Context

- Read the relevant feature spec from `feature_docs/[feature]/feature-spec-[feature].md` if one exists
- Read the affected source files to understand current behavior
- Read AGENTS.md conventions that apply to the affected area

## Step 3: Implement the Fix

Follow all AGENTS.md conventions:

- Backend: AsNoTracking, CancellationToken, ApiResponse<T>, proper exceptions
- Frontend: t() for all strings, React Query for server state, Redux for UI state only
- Never edit files under `api/generated/`

## Step 4: Run Relevant Tests

- If backend files changed: `dotnet test backend/Backend.sln`
- If frontend files changed: `cd frontend && npm run test:run`
- If the fix warrants a new test, write one following the test patterns in the template

## Step 5: Update Feature Spec

If the fix changed any behavior documented in the feature spec, update the spec:

- Changed validation rules → update Validation Rules section
- Changed business logic → update Business Rules section
- Changed UI behavior → update Frontend UI Description
- Pure code fixes (null checks, refactoring) → no spec update needed

## Step 6: Summary

Provide a summary of:

- What was wrong (root cause)
- What was changed (files modified)
- How it was verified (test results)
- Whether the feature spec was updated
