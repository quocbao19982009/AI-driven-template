# Feature Spec: Authentication & Authorization

**Last Updated:** `2026-02-27`
**Status:** Plan only — not yet implemented

> This feature is low priority per the assignment brief. The plan below describes how authentication and authorization would be designed and integrated into the existing system. No code changes have been made.

---

## 1. Entity

### User
Extends `BaseEntity` (`Id: int`, `CreatedAt: DateTime`, `UpdatedAt: DateTime`)

| Field          | C# Type   | Required | Constraints                                     | Notes                             |
| -------------- | --------- | -------- | ----------------------------------------------- | --------------------------------- |
| `Email`        | `string`  | yes      | Unique, max 200 chars, valid email format        | Used as login identifier          |
| `PasswordHash` | `string`  | yes      | BCrypt hash (never store plain text)             | Hashed via `BCrypt.Net-Next`      |
| `FullName`     | `string`  | yes      | Not empty, max 200 chars                         |                                   |
| `Role`         | `UserRole` | yes     | Enum: `Admin`, `Manager`, `Employee`             | Single role per user              |

**Database:** Table name: `Users`

### UserRole (Enum)
```csharp
public enum UserRole
{
    Admin,    // Full system access including user management
    Manager,  // Can manage factory and personnel information (all CRUD)
    Employee  // Can only view factories/personnel and create/read reservations
}
```

---

## 2. Core Values & Principles

- Authentication proves identity (who you are) — Authorization proves permissions (what you may do).
- Passwords are always hashed with BCrypt (`BCrypt.Net-Next`) before storage — never SHA256 or plain text.
- Tokens are short-lived JWTs signed with a secret key stored in environment variables / user-secrets — never in `appsettings.json`.
- Each endpoint is protected at the role it requires — controllers use `[Authorize(Roles = "...")]` attributes.
- Role escalation is blocked by service-layer checks: Employees cannot create Manager or Admin users; only Admins can promote roles.
- The frontend stores the JWT in memory (React state / Redux) — NOT in `localStorage` to prevent XSS token theft. `HttpOnly` refresh token cookies are used if a session-persistence flow is needed.
- All existing endpoints are currently public. Adding auth is a breaking change — every endpoint must be gated behind at minimum `[Authorize]` before the system goes to production.

---

## 3. Architecture Decisions

### JWT over session cookies for the API

**Decision:** Use JWT (JSON Web Tokens) with a short expiry (e.g., 15 minutes) + a `HttpOnly` refresh token cookie with a longer TTL.
**Alternatives Considered:** ASP.NET Core Identity with cookie sessions; OAuth2 with an external provider (Azure AD, Auth0).
**Rationale:** The system is a REST API consumed by a SPA. Stateless JWTs eliminate the need for a session store on the backend and scale horizontally without sticky sessions. External OAuth is the right long-term choice for an enterprise cloud system (the assignment mentions Azure tenancy), but adds external dependencies not needed for a PoC. ASP.NET Identity is heavyweight — we already have a custom `User` entity and would need to align schemas.

### BCrypt for password hashing (already a project convention)

**Decision:** `BCrypt.Net-Next` (`BCrypt.HashPassword` / `BCrypt.Verify`) with the default cost factor (12).
**Alternatives Considered:** SHA256, Argon2, PBKDF2.
**Rationale:** SHA256 is cryptographically fast — attackers can try billions of guesses per second offline. BCrypt is deliberately slow (cost factor tunes the iteration count), making brute-force infeasible. Argon2 is the modern gold standard but `BCrypt.Net-Next` is already a project convention. This decision is pre-established in CLAUDE.md and must not be changed.

### Claim-based roles (enum stored in DB, claim in JWT)

**Decision:** Store `UserRole` as an enum column in the `Users` table; embed the role as a claim in the JWT on login.
**Alternatives Considered:** Separate `Roles` table with many-to-many (ASP.NET Identity approach); policy-based auth with resource-level checks.
**Rationale:** The system only needs three coarse roles (Admin, Manager, Employee). A flat enum is simpler and sufficient. Role claims in the JWT allow `[Authorize(Roles = "Manager,Admin")]` without any database round-trip per request. Policy-based auth would be the right evolution if permissions become granular (e.g., "can edit own reservations only").

### Separate `AuthController` for login/refresh/logout

**Decision:** Add a dedicated `/api/auth` controller with `POST /login`, `POST /refresh`, and `POST /logout`.
**Alternatives Considered:** Embedding auth in `UsersController`; using a third-party identity server.
**Rationale:** Auth flows have different concerns (token generation, cookie management) from user CRUD. Keeping them separate obeys single-responsibility and makes it easy to swap the auth mechanism later without touching user management.

---

## 4. Data Flow

### Login Flow (planned)
1. Client `POST /api/auth/login` with `{ email, password }`.
2. `AuthService.LoginAsync` loads `User` by email (throws `UnauthorizedException` if not found — do NOT distinguish "email not found" from "wrong password" to prevent user enumeration).
3. `BCrypt.Verify(password, user.PasswordHash)` — throws `UnauthorizedException` if false.
4. Service generates a signed JWT containing claims: `sub` (userId), `email`, `role`, `exp` (15 min).
5. Service generates a refresh token (random opaque string, stored in a `RefreshTokens` table with expiry + userId).
6. Controller sets refresh token as `HttpOnly; Secure; SameSite=Strict` cookie, returns JWT in response body.
7. Frontend stores JWT in Redux / React state (NOT localStorage).

### Authenticated Request Flow (planned)
1. Client attaches `Authorization: Bearer <jwt>` header on every API request.
2. ASP.NET Core JWT middleware validates signature, expiry, and issuer on every request automatically.
3. Controller `[Authorize(Roles = "Manager,Admin")]` attribute checks the `role` claim.
4. If unauthorized: 401 (not authenticated) or 403 (authenticated but wrong role).

### Token Refresh Flow (planned)
1. Client intercepts 401 response in `apiFetch.ts` mutator.
2. Client `POST /api/auth/refresh` — server reads `HttpOnly` cookie, validates refresh token in DB.
3. Server issues a new JWT and rotates the refresh token.
4. Client retries the original request with the new JWT.

### Logout Flow (planned)
1. Client `POST /api/auth/logout`.
2. Server deletes refresh token from DB, clears the `HttpOnly` cookie.
3. Frontend clears JWT from state.

---

## 5. API Endpoints

| Method | Route                    | Request Body                 | Response                    | Required Role | Description                   |
| ------ | ------------------------ | ---------------------------- | --------------------------- | ------------- | ----------------------------- |
| POST   | `/api/auth/login`        | `LoginRequest`               | `ApiResponse<LoginResponse>`| None (public) | Authenticate and receive JWT  |
| POST   | `/api/auth/refresh`      | —                            | `ApiResponse<LoginResponse>`| None (cookie) | Rotate refresh token, new JWT |
| POST   | `/api/auth/logout`       | —                            | `ApiResponse<bool>`         | Authenticated | Invalidate refresh token      |
| GET    | `/api/users`             | —                            | `ApiResponse<PagedResult<UserDto>>` | Admin  | List all users          |
| POST   | `/api/users`             | `CreateUserRequest`          | `ApiResponse<UserDto>`      | Admin         | Create a new user             |
| PUT    | `/api/users/{id}`        | `UpdateUserRequest`          | `ApiResponse<UserDto>`      | Admin         | Update user role/name         |
| DELETE | `/api/users/{id}`        | —                            | `ApiResponse<bool>`         | Admin         | Delete user                   |

### Impact on Existing Endpoints

The table below shows the authorization requirements that would be applied to each existing endpoint once auth is enabled:

| Endpoint Group           | Allowed Roles              | Notes                                       |
| ------------------------ | -------------------------- | ------------------------------------------- |
| `GET /api/factories`     | All authenticated          | All roles can view factories                |
| `POST /api/factories`    | Manager, Admin             | Only Managers and Admins can create         |
| `PUT /api/factories/{id}`| Manager, Admin             |                                             |
| `DELETE /api/factories/{id}` | Admin only             | Destructive action gated to Admin           |
| `GET /api/personnel`     | All authenticated          |                                             |
| `POST /api/personnel`    | Manager, Admin             |                                             |
| `PUT /api/personnel/{id}`| Manager, Admin             |                                             |
| `DELETE /api/personnel/{id}` | Admin only             |                                             |
| `GET /api/reservations`  | All authenticated          | Employees can view all reservations         |
| `POST /api/reservations` | All authenticated          | Employees can create reservations           |
| `PUT /api/reservations/{id}` | Manager, Admin         | Editing reservations is manager-level       |
| `DELETE /api/reservations/{id}` | Manager, Admin      |                                             |
| `GET /api/scheduling/**` | All authenticated          | Read-only summary visible to all            |

### DTOs (planned)

```csharp
public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, DateTime ExpiresAt, UserDto User);

public record UserDto(int Id, string Email, string FullName, string Role, DateTime CreatedAt);

public record CreateUserRequest(string Email, string Password, string FullName, UserRole Role);

public record UpdateUserRequest(string FullName, UserRole Role);
```

---

## 6. Validation Rules

| Field      | Rules                                                                        |
| ---------- | ---------------------------------------------------------------------------- |
| `Email`    | Required, valid email format, unique in `Users` table, max 200 chars         |
| `Password` | Required on create, min 8 characters, at least one digit or special char     |
| `FullName` | Required, not empty, max 200 chars                                           |
| `Role`     | Required, must be one of `Admin`, `Manager`, `Employee`                      |

- `LoginRequest` validation: both fields required; no hint given if email-not-found vs wrong password (prevents user enumeration).
- Password confirmation field is a frontend-only concern; backend validates only the `Password` field.

---

## 7. Business Rules

1. **No user enumeration:** Login returns generic `401 Unauthorized` whether email is not found or password is wrong.
2. **BCrypt only:** Passwords are always hashed with BCrypt before storage. Plaintext comparison is never performed.
3. **Short-lived JWTs:** Access tokens expire in 15 minutes. The refresh token has a configurable TTL (e.g., 7 days).
4. **Refresh token rotation:** Each use of a refresh token issues a new one and invalidates the old one. Reuse of a spent refresh token triggers invalidation of the entire refresh chain for that user (token theft detection).
5. **Single role per user:** A user has exactly one role. Admins can elevate or demote roles.
6. **Admin cannot delete themselves:** Service throws `ValidationException` if an Admin attempts to delete their own account.
7. **First user bootstrapping:** The first user created in an empty system is automatically assigned `Admin` role (seed data or a one-time bootstrap endpoint behind a setup flag).

---

## 8. Authorization

| Resource                   | Employee | Manager | Admin |
| -------------------------- | -------- | ------- | ----- |
| View factories / personnel | ✓        | ✓       | ✓     |
| Create / edit factories    | ✗        | ✓       | ✓     |
| Delete factories           | ✗        | ✗       | ✓     |
| Create / edit personnel    | ✗        | ✓       | ✓     |
| Delete personnel           | ✗        | ✗       | ✓     |
| Create reservations        | ✓        | ✓       | ✓     |
| Edit / delete reservations | ✗        | ✓       | ✓     |
| View scheduling overview   | ✓        | ✓       | ✓     |
| User management            | ✗        | ✗       | ✓     |

Backend enforcement: `[Authorize(Roles = "Manager,Admin")]` on controller actions.
Frontend enforcement: role-aware UI (hide/disable write actions for Employee role) — but this is cosmetic only; the backend is the authoritative gate.

---

## 9. Frontend UI Description

### Login Page
- Route: `/login`
- Unauthenticated users are redirected here by a TanStack Router `beforeLoad` guard.
- Form fields: Email (text input), Password (password input).
- Submit button: "Sign in".
- On success: redirect to `/` (dashboard / factories page).
- On failure: inline error "Invalid email or password." (no field-level specificity).

### User Management Page (Admin only)
- Route: `/users`
- Nav link hidden for Manager and Employee roles.
- Paginated table: columns `Email`, `Full Name`, `Role`, `Created At`, Actions.
- "New User" button (opens Create dialog). Admin role only.
- Edit and Delete actions per row. Cannot delete self.

### Role-aware UI Guards
- Hide "New Factory", "Edit", "Delete" buttons from Employees.
- Hide "Edit Reservation", "Delete Reservation" from Employees.
- Show a disabled tooltip "You don't have permission to perform this action" when an Employee hovers disabled controls.
- Navigation guards: redirect to `/unauthorized` (403 page) if route requires a role the user does not have.

### Auth State in Redux
- `authSlice` holds: `token: string | null`, `user: UserDto | null`, `isAuthenticated: boolean`.
- On app startup, `apiFetch.ts` reads the token from Redux and attaches it as a Bearer header.
- On 401 response, `apiFetch.ts` triggers the refresh flow; if refresh fails, dispatches `logout` action and redirects to `/login`.

---

## 10. Redux UI State

**Slice:** `auth` (in `store/slices/authSlice.ts`)

| State Field       | Type           | Purpose                                           |
| ----------------- | -------------- | ------------------------------------------------- |
| `token`           | `string \| null` | Current JWT access token                        |
| `user`            | `UserDto \| null` | Logged-in user info (id, email, role, fullName) |
| `isAuthenticated` | `boolean`      | Derived from `token !== null`                     |

**Slice:** `users` (in `store/slices/usersSlice.ts`) — Admin page only

| State Field   | Type       | Purpose                          |
| ------------- | ---------- | -------------------------------- |
| `searchQuery` | `string`   | Search/filter on user list page  |
| `selectedIds` | `number[]` | For future bulk-action support   |

---

## 11. File Locations

### Backend (planned)

| File | Path |
|------|------|
| Auth Controller | `backend/src/Backend.Api/Features/Auth/AuthController.cs` |
| Auth Service interface | `backend/src/Backend.Api/Features/Auth/IAuthService.cs` |
| Auth Service | `backend/src/Backend.Api/Features/Auth/AuthService.cs` |
| Auth Repository interface | `backend/src/Backend.Api/Features/Auth/IAuthRepository.cs` |
| Auth Repository | `backend/src/Backend.Api/Features/Auth/AuthRepository.cs` |
| Auth DTOs | `backend/src/Backend.Api/Features/Auth/AuthDtos.cs` |
| Auth Validator | `backend/src/Backend.Api/Features/Auth/AuthValidator.cs` |
| Users Controller | `backend/src/Backend.Api/Features/Users/UsersController.cs` |
| Users Service interface | `backend/src/Backend.Api/Features/Users/IUsersService.cs` |
| Users Service | `backend/src/Backend.Api/Features/Users/UsersService.cs` |
| Users Repository interface | `backend/src/Backend.Api/Features/Users/IUsersRepository.cs` |
| Users Repository | `backend/src/Backend.Api/Features/Users/UsersRepository.cs` |
| Users DTOs | `backend/src/Backend.Api/Features/Users/UserDtos.cs` |
| Users Validator | `backend/src/Backend.Api/Features/Users/UsersValidator.cs` |
| User entity | `backend/src/Backend.Api/Common/Models/User.cs` |
| UserRole enum | `backend/src/Backend.Api/Common/Models/UserRole.cs` |
| RefreshToken entity | `backend/src/Backend.Api/Common/Models/RefreshToken.cs` |

### Frontend (planned)

| File | Path |
|------|------|
| Login page | `frontend/src/features/auth/components/login-page.tsx` |
| Login hooks | `frontend/src/features/auth/hooks/use-login-mutation.ts` |
| Logout hooks | `frontend/src/features/auth/hooks/use-logout-mutation.ts` |
| Users page | `frontend/src/features/users/components/users-page.tsx` |
| Users table | `frontend/src/features/users/components/users-table.tsx` |
| User form dialog | `frontend/src/features/users/components/user-form-dialog.tsx` |
| User delete dialog | `frontend/src/features/users/components/user-delete-dialog.tsx` |
| Auth Redux slice | `frontend/src/features/auth/store/auth-slice.ts` |
| Users Redux slice | `frontend/src/features/users/store/users-slice.ts` |
| Auth route guard | `frontend/src/router/guards/require-auth.tsx` |
| Role route guard | `frontend/src/router/guards/require-role.tsx` |
| Auth route | `frontend/src/routes/auth/index.tsx` |
| Users route | `frontend/src/routes/users/index.tsx` |
| Generated API (auth) | `frontend/src/api/generated/auth/` |
| Generated API (users) | `frontend/src/api/generated/users/` |

### New Backend Registrations in `Program.cs` (planned)

```csharp
// JWT middleware
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* validate issuer, audience, key from env vars */ });
builder.Services.AddAuthorization();

// Feature registrations
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IUsersService, UsersService>();
```

---

## 12. Migration Name

`AddUserAndRefreshTokenEntities`

Adds: `Users` table (email, passwordHash, fullName, role), `RefreshTokens` table (token, userId FK, expiresAt, usedAt, isRevoked).

---

## 13. Tests

### Backend Unit Tests (planned)

| Test | Description |
| ---- | ----------- |
| `LoginAsync_WithValidCredentials_ReturnsJwt` | Happy path: valid email + password returns token |
| `LoginAsync_WithInvalidEmail_ThrowsUnauthorizedException` | Email not found returns generic 401 |
| `LoginAsync_WithWrongPassword_ThrowsUnauthorizedException` | Wrong password returns generic 401 |
| `LoginAsync_NeverRevealsWhichFieldIsWrong` | Both failure paths return identical error message |
| `CreateUserAsync_HashesPasswordWithBCrypt` | PasswordHash is BCrypt, not plaintext or SHA256 |
| `CreateUserAsync_WithDuplicateEmail_ThrowsValidationException` | Unique email enforced |
| `RefreshAsync_WithValidToken_RotatesToken` | Old token invalidated, new one issued |
| `RefreshAsync_WithSpentToken_RevokesChain` | Token reuse triggers full chain revocation |
| `DeleteUserAsync_CannotDeleteSelf_ThrowsValidationException` | Self-delete blocked |

### Frontend Tests (planned)

| Test | Description |
| ---- | ----------- |
| `LoginPage submits email and password` | Form submission dispatches login mutation |
| `LoginPage shows generic error on 401` | No field-level error specificity |
| `RequireAuth redirects unauthenticated users to /login` | Router guard works |
| `RequireRole redirects wrong-role users to /unauthorized` | Role guard works |
| `Employee does not see New Factory button` | Role-aware UI hides write actions |
| `authSlice stores token on login success` | Redux slice unit test |
| `authSlice clears token on logout` | Redux slice unit test |

---

## Checklist

- [ ] Backend: `User` and `RefreshToken` entities created
- [ ] Backend: EF migration `AddUserAndRefreshTokenEntities` applied
- [ ] Backend: JWT middleware configured in `Program.cs`
- [ ] Backend: `AuthController`, `AuthService`, `AuthRepository` implemented
- [ ] Backend: `UsersController`, `UsersService`, `UsersRepository` implemented
- [ ] Backend: `[Authorize]` attributes added to all existing controllers
- [ ] Frontend: `api:sync` run after backend endpoints added
- [ ] Frontend: `authSlice` created, token attached in `apiFetch.ts`
- [ ] Frontend: `RequireAuth` and `RequireRole` router guards added
- [ ] Frontend: Login page implemented
- [ ] Frontend: Role-aware UI guards applied to all write actions
- [ ] Frontend: User management page implemented (Admin only)
- [ ] Translations: `auth.*` and `users.*` keys added to `en.json` and `fi.json`
