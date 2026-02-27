# Coding Style Guide

This is the single source of truth for coding style across the project. All rules here are inferred from the existing codebase and enforced by the AI on every task.

**To change a rule:** Edit this file, commit it, and the AI will follow it from the next session onward.

---

## 1. General

- **Indentation:** 4 spaces for C# and TypeScript; 2 spaces for JSON, YAML, and XML config files
- **Line length:** No hard limit, but keep lines under 120 characters where practical
- **End of file:** Always include a trailing newline
- **Trailing whitespace:** Always trimmed
- **One primary class/interface per file** — related DTOs and small helpers may coexist in the same file

---

## 2. C# / Backend

### Namespaces
- Use **file-scoped namespaces** (semicolon-terminated, no braces)
  ```csharp
  namespace Backend.Features.Todos;
  ```
- Organize by feature: `Backend.Features.[FeatureName]`
- Common utilities: `Backend.Common.*`

### Naming
| Symbol | Convention | Example |
|---|---|---|
| Classes, Records, Enums | `PascalCase` | `TodoItem`, `UsersController` |
| Interfaces | `I` + `PascalCase` | `ITodosService`, `IUsersRepository` |
| Methods | `PascalCase` | `GetAllAsync`, `CreateAsync` |
| Properties | `PascalCase` | `FirstName`, `IsCompleted` |
| Private/internal fields | `_camelCase` | `_service`, `_logger` |
| Private static fields | `s_camelCase` | `s_defaultTimeout` |
| Constants | `PascalCase` | `DefaultPageSize` |
| Local variables & parameters | `camelCase` | `user`, `cancellationToken` |

- Do **not** use `this.` qualification unless there is an ambiguity
- Do **not** abbreviate — use meaningful full names

### `var` Usage
- Avoid `var` when the type is not immediately obvious from the right-hand side
- Use explicit types: `List<User> users = ...`, `string email = ...`
- `var` is acceptable only when the type is self-evident: `var dto = new UserDto(...)`

### Expression-Bodied Members
- Use for simple, single-line implementations
  ```csharp
  public string FullName => $"{FirstName} {LastName}";
  private static UserDto MapToDto(User user) => new(...);
  ```
- Use block bodies for methods with multiple statements or logic

### Access Modifiers
- Always declare access modifiers explicitly — no implicit `private`
- Order: `public` / `private` / `protected` / `internal` → `static` → `virtual` / `abstract` / `sealed` / `override` → `readonly` → `async`

### Brace Style
- Allman style — opening brace on a new line
  ```csharp
  if (condition)
  {
      // code
  }
  ```

### Async/Await
- Every async method must accept `CancellationToken cancellationToken = default`
- Pass `cancellationToken` through all layers: controller → service → repository
- Use `async Task` / `async Task<T>` — never `async void` (except event handlers)

### Null Checks & Patterns
- Prefer `is null` / `is not null` over `== null` / `!= null`
- Use null-coalescing: `request.Role ?? "User"`
- Use pattern matching and switch expressions over `as` + null checks

### DTOs & Records
- Use **records** for immutable response DTOs: `public record UserDto(int Id, string Email);`
- Use **classes** for mutable request objects with validation attributes
- Annotate entity properties with `[Required]`, `[MaxLength]` etc.

### Method Grouping
- Group related methods with a comment header:
  ```csharp
  // --- GetAll ---
  // --- Create ---
  ```

---

## 3. TypeScript / Frontend

### Naming
| Symbol | Convention | Example |
|---|---|---|
| Components | `kebab-case`, `.tsx` | `todos-page.tsx`, `todo-form-dialog.tsx` |
| Hooks | `camelCase` starting with `use` | `useTodoPagination`, `useDebounce` |
| Types & Interfaces | `PascalCase` | `TodoFormDialogProps`, `PaginationState` |
| Variables & functions | `camelCase` | `searchQuery`, `onSubmit` |
| Module-level constants | `UPPER_SNAKE_CASE` | `DEFAULT_PAGE_SIZE` |

### `type` vs `interface`
- Prefer **`type`** for prop shapes and simple aliases
- Use **`interface`** only when merging declarations or extending multiple types
- Use `type` with `z.infer<>` for Zod schema inference

### Functions
- Use **named function declarations** for exported components (better stack traces)
  ```typescript
  export function TodosPage() { ... }
  ```
- Use **arrow functions** for internal helpers and callbacks
  ```typescript
  const mapToLabel = (priority: string) => ...;
  ```

### Import Ordering
1. React and external libraries
2. Internal path aliases (`@/`)
3. Relative imports (`./`)

Blank line between each group:
```typescript
import { useState } from "react";
import { useTranslation } from "react-i18next";

import type { TodoDto } from "@/api/generated/models";
import { useAppDispatch } from "@/store/hooks";

import { TodosTable } from "./todos-table";
```

### Exports
- Use **named exports** for components and hooks
- Default exports only for modules with a single, obvious primary export

### Props
- Define props type above the component function
- Destructure props in the function signature
  ```typescript
  type TodoFormDialogProps = {
    todo?: TodoDto | null;
    open: boolean;
    onOpenChange: (open: boolean) => void;
  };

  export function TodoFormDialog({ todo, open, onOpenChange }: TodoFormDialogProps) { ... }
  ```

### State Rules
- `useState` → local, temporary UI state (dialog open/close)
- `useQuery` / `useMutation` via Orval-generated hooks → server/API state
- `useAppDispatch` / `useAppSelector` → UI-only Redux state (search, filters, selected IDs)
- Never duplicate API data into Redux

### Hooks
- Always include dependency arrays in `useEffect`
- Always use `useAppDispatch` / `useAppSelector` — never raw `useDispatch` / `useSelector`
- Always use `const { t } = useTranslation()` — never hardcode visible text

---

## 4. CSS / Tailwind

### Class Ordering
Order Tailwind classes as follows:
1. **Layout** — `flex`, `grid`, `hidden`, `block`, positioning
2. **Sizing** — `w-*`, `h-*`, `max-w-*`, `p-*`, `m-*`, `gap-*`
3. **Typography** — `text-*`, `font-*`
4. **Visual** — `bg-*`, `border`, `rounded-*`, `shadow-*`
5. **Responsive** — `md:`, `lg:` variants
6. **State** — `hover:`, `focus:`, `disabled:`, `aria-*:` variants

### Dynamic Classes
- Always use `cn()` for any dynamic or conditional class merging
  ```typescript
  className={cn("base-class", isActive && "active-class", className)}
  ```
- Use `cva` for components with multiple variants (e.g., Button, Badge)

### Colors
- Use semantic tokens: `text-primary`, `bg-accent`, `text-muted-foreground`
- Use opacity modifiers: `bg-destructive/20`
- Map status/priority colors through a config object rather than inline ternaries

### Responsive Design
- Mobile-first: base styles target mobile, use `sm:` / `md:` / `lg:` for larger screens

---

## 5. Testing

### Backend (xUnit + Moq + FluentAssertions)
- Test class: `[Feature]Tests` — e.g., `TodosServiceTests`
- Test method: `MethodName_Condition_ExpectedResult` — e.g., `GetAll_ReturnsOkWithPagedResult`
- SUT field named `_sut`; mocks suffixed with `Mock` — e.g., `_serviceMock`
- Group tests with comment headers matching the method under test
- Use `It.IsAny<CancellationToken>()` in mock setups
- Assert with FluentAssertions: `.Should().BeOfType<OkObjectResult>()`

### Frontend (Vitest + React Testing Library)
- Test file: `[Component].test.tsx`
- Test naming: `it("should ...", () => { ... })`
- Group with `describe()` blocks
- Render via `render()` from `@/test/test-utils`
- Mock API calls with `vi.mock()`
- Query with `screen.getByText()`, `screen.getByRole()`, etc.
