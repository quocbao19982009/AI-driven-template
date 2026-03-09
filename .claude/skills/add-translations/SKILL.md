---
name: add-translations
description: Add translation keys to both en.json and fi.json locale files for a feature. Ensures both locales stay in sync.
argument-hint: "[feature-name]"
allowed-tools: "Read, Edit"
---

# Add Translations

Add or update translation keys for the `$ARGUMENTS` feature in both locale files.

## Steps

1. **Read both locale files:**
   - `frontend/src/locales/en.json`
   - `frontend/src/locales/fi.json`

2. **Read the feature spec** (if it exists) for context on what keys are needed:
   - `feature_docs/$ARGUMENTS/feature-spec-$ARGUMENTS.md`

3. **Read the `features` namespace** in both locale files as the reference pattern for key structure.

4. **Determine what keys to add:**
   - If the user provided specific keys, add those
   - If not, examine the feature's components (read files in `frontend/src/features/$ARGUMENTS/`) and identify all `t()` calls that reference keys not yet in the locale files

5. **Add keys to BOTH files:**

   **`en.json`** — Add under a namespace matching the feature name:

   ```json
   "[feature-name]": {
     "title": "...",
     "description": "...",
     "new[Entity]": "...",
     "table": { "id": "ID", "name": "Name", "createdAt": "Created At", "empty": "No ... found." },
     "form": { "createTitle": "...", "editTitle": "...", "nameLabel": "...", "namePlaceholder": "..." },
     "delete": { "title": "...", "description": "..." },
     "toast": { "created": "...", "updated": "...", "deleted": "...", "createError": "...", "updateError": "...", "deleteError": "..." }
   }
   ```

   **`fi.json`** — Add the same keys with **actual Finnish translations**, not English placeholders. Use the existing Finnish translations in the `"features"` namespace as a style reference.

6. **Also add the nav key** if not present:
   - `"nav.[feature-name]"` in both files

7. **Validate:** Ensure the JSON remains valid after edits. Count that both files have the same number of keys in the feature namespace.

## Important

- Finnish translations must be real Finnish — not English copies
- Mirror the exact key structure from the `features` namespace
- Never remove existing keys — only add new ones
