# Plan: Task Management System (Full-Stack)

## TL;DR
Build a full-stack Team Task Board — .NET 8 Web API backend (Clean Architecture, TDD, ADO.NET + SQLite with raw parameterized SQL, JWT auth) with a React (Vite + TypeScript) frontend. No Entity Framework, Dapper, or MediatR. Seeded demo data included.

---

## User Story (Informal)
> *As a team member, I want to register, log in, and manage tasks on a shared board so our team can track work progress. Each task has a title, description, status (Todo / InProgress / Done), and due date, and can be assigned to any team member.*

---

## Technology Stack
| Layer | Technology |
|-------|-----------|
| Database | SQLite, raw parameterized SQL |
| Data Access | ADO.NET (`Microsoft.Data.Sqlite`) — no EF, Dapper, MediatR |
| Backend | .NET 8, ASP.NET Web API, Clean Architecture |
| Auth | JWT Bearer tokens (BCrypt password hashing) |
| Testing | xUnit, Moq, FluentAssertions |
| Frontend | Vite + React 18 + TypeScript |
| Styling | CSS Modules or Tailwind CSS (lightweight) |

---

## Solution Structure

```
TaskManagementSystem/
├── src/
│   ├── TaskManagement.Domain/             # Entities, enums, repository interfaces
│   ├── TaskManagement.Application/        # Business logic services, DTOs, validation
│   ├── TaskManagement.Infrastructure/     # ADO.NET repos, JWT service, DB setup
│   ├── TaskManagement.WebAPI/             # Controllers, middleware, DI config
│   └── task-management-frontend/          # Vite + React + TypeScript
├── tests/
│   ├── TaskManagement.Domain.Tests/
│   ├── TaskManagement.Application.Tests/
│   ├── TaskManagement.Infrastructure.Tests/
│   └── TaskManagement.WebAPI.Tests/
├── database/
│   ├── 001_schema.sql                     # Tables: Users, Tasks
│   └── 002_seed_data.sql                  # Demo users + tasks
├── TaskManagementSystem.sln
└── README.md
```

---

## Database Design

### Users Table
| Column | Type | Notes |
|--------|------|-------|
| Id | TEXT | PK, GUID string |
| Username | TEXT | Unique, NOT NULL |
| Email | TEXT | Unique, NOT NULL |
| PasswordHash | TEXT | NOT NULL |
| CreatedAt | TEXT | ISO8601 UTC datetime |

### Tasks Table
| Column | Type | Notes |
|--------|------|-------|
| Id | TEXT | PK, GUID string |
| Title | TEXT | NOT NULL |
| Description | TEXT | Nullable |
| Status | INTEGER | 0=Todo, 1=InProgress, 2=Done |
| DueDate | TEXT | ISO8601 UTC datetime, NOT NULL |
| AssignedUserId | TEXT | FK → Users(Id), Nullable |
| CreatedByUserId | TEXT | FK → Users(Id), NOT NULL |
| CreatedAt | TEXT | ISO8601 UTC datetime |
| UpdatedAt | TEXT | Nullable |

### Raw SQL
All data access uses parameterized inline SQL queries (e.g., `SELECT`, `INSERT`, `UPDATE`, `DELETE`) within the repository implementations. No stored procedures.

---

## API Endpoints

### Auth Controller (`/api/auth`) — No authorization required
| Verb | Route | Description |
|------|-------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login, returns JWT token |

### Tasks Controller (`/api/tasks`) — Requires JWT authorization
| Verb | Route | Description |
|------|-------|-------------|
| GET | `/api/tasks` | List all tasks |
| GET | `/api/tasks/{id}` | Get task by ID |
| POST | `/api/tasks` | Create a new task |
| PUT | `/api/tasks/{id}` | Update an existing task |
| DELETE | `/api/tasks/{id}` | Delete a task |

### Users Controller (`/api/users`) — Mixed authorization
| Verb | Route | Description | Auth |
|------|-------|-------------|------|
| GET | `/api/users` | List all users (for assignment) | Yes |

### Health (`/api/health`) — Non-authorized endpoint
| Verb | Route | Description |
|------|-------|-------------|
| GET | `/api/health` | Health check (public) |

---

## Implementation Steps

### Phase 1: Solution Scaffolding
1. Create .NET solution and all project/test projects with correct references
2. Add NuGet packages: `Microsoft.Data.Sqlite`, `BCrypt.Net-Next`, `System.IdentityModel.Tokens.Jwt`, `xunit`, `Moq`, `FluentAssertions`, `Microsoft.AspNetCore.Mvc.Testing`
3. Create database SQL scripts (schema + seed data, raw SQL)
4. Configure `appsettings.json` with SQLite connection string (file-based DB) and JWT settings

*Dependencies: None — this phase must complete first.*

### Phase 2: Domain Layer (TDD)
5. Define `TaskStatus` enum (Todo, InProgress, Done)
6. Define `User` entity (Id, Username, Email, PasswordHash, CreatedAt)
7. Define `TaskItem` entity (Id, Title, Description, Status, DueDate, AssignedUserId, CreatedByUserId, CreatedAt, UpdatedAt) — named `TaskItem` to avoid conflict with `System.Threading.Tasks.Task`
8. Define repository interfaces: `IUserRepository`, `ITaskRepository`
9. Define `IPasswordHasher` and `IJwtTokenService` interfaces
10. Write domain entity unit tests (validation logic on entity creation)

*Dependencies: Phase 1*

### Phase 3: Application Layer — Business Logic (TDD)
11. Define DTOs: `RegisterRequest`, `LoginRequest`, `LoginResponse`, `TaskCreateRequest`, `TaskUpdateRequest`, `TaskResponse`, `UserResponse`
12. Implement `IAuthService` / `AuthService` — register (validate unique username/email, hash password, store) and login (verify credentials, issue JWT)
13. Implement `ITaskService` / `TaskService` — full CRUD with validation (title required, due date not in past, valid status transitions)
14. Write unit tests for `AuthService` (mock repos + password hasher + JWT service)
15. Write unit tests for `TaskService` (mock repo, test all validation rules)

*Dependencies: Phase 2. Steps 12-13 and 14-15 can be done TDD-style (test first, then implement).*

### Phase 4: Infrastructure Layer (TDD)
16. Implement `SqliteConnectionFactory` (creates `SqliteConnection` from connection string)
17. Implement `UserRepository` using ADO.NET + raw parameterized SQL
18. Implement `TaskRepository` using ADO.NET + raw parameterized SQL
19. Implement `BcryptPasswordHasher` (wraps BCrypt.Net)
20. Implement `JwtTokenService` (generates JWT with claims: UserId, Username)
21. Implement `DatabaseInitializer` — runs schema + seed SQL on startup to create tables and insert demo data
22. Write integration tests for repositories (using in-memory SQLite test database)
23. Write unit tests for `JwtTokenService` and `BcryptPasswordHasher`

*Dependencies: Phase 2 for interfaces. Steps 17-20 parallel with each other.*

### Phase 5: Web API Layer (TDD)
24. Configure `Program.cs`: DI registration, JWT auth middleware, CORS for React, Swagger
25. Implement `AuthController` (POST register, POST login)
26. Implement `TasksController` (full CRUD, `[Authorize]` attribute)
27. Implement `UsersController` (GET all users, `[Authorize]`)
28. Add `HealthController` — no `[Authorize]` (public endpoint)
29. Add global exception handling middleware
30. Write controller unit tests (mock services, verify HTTP responses)
31. Write integration tests using `WebApplicationFactory`

*Dependencies: Phase 3 + Phase 4.*

### Phase 6: React Frontend
32. Scaffold Vite + React + TS project in `src/task-management-frontend/`
33. Set up project structure: `components/`, `pages/`, `services/`, `hooks/`, `types/`
34. Implement API service layer (Axios/fetch wrapper with JWT interceptor)
35. Implement Auth pages: Login and Register forms
36. Implement Task Board page: Kanban columns mapped to `TaskItemStatus` enum (Todo / InProgress / Done). Moving a task between columns calls `PUT /api/tasks/{id}` to update its status on the backend. Use drag-and-drop (e.g., `@hello-pangea/dnd`) or click-to-move buttons.
37. Implement Task CRUD: create modal, edit modal, delete confirmation
38. Implement responsive layout (mobile-friendly)
39. Add auth context/provider for JWT token management
40. Add protected routes (redirect to login if unauthenticated)

*Dependencies: Phase 5 (API must be running). Steps 35-40 parallel after 32-34.*

### Phase 7: Documentation & Polish
41. Write comprehensive README with: setup instructions, prerequisites, architecture diagram (text), seeded credentials, API endpoint docs
42. Ensure seed data includes: 2 demo users, 5+ sample tasks across all statuses
43. Final testing pass — run all unit + integration tests

*Dependencies: All phases.*

---

## Seeded Demo Data
- **User 1**: username `admin`, email `admin@taskboard.com`, password `Admin123!`
- **User 2**: username `johndoe`, email `john@taskboard.com`, password `John123!`
- **Tasks**: 5-6 sample tasks with varying statuses, due dates, and assignments

---

## Relevant Files (to create)

### Database
- `database/001_schema.sql` — CREATE TABLE Users, Tasks with FKs and indexes (SQLite syntax)
- `database/002_seed_data.sql` — INSERT demo users (pre-hashed passwords) and tasks

### Domain Layer
- `src/TaskManagement.Domain/Entities/User.cs` — User entity
- `src/TaskManagement.Domain/Entities/TaskItem.cs` — TaskItem entity
- `src/TaskManagement.Domain/Enums/TaskItemStatus.cs` — Todo/InProgress/Done enum
- `src/TaskManagement.Domain/Interfaces/IUserRepository.cs` — CRUD interface for users
- `src/TaskManagement.Domain/Interfaces/ITaskRepository.cs` — CRUD interface for tasks
- `src/TaskManagement.Domain/Interfaces/IPasswordHasher.cs` — Hash + Verify
- `src/TaskManagement.Domain/Interfaces/IJwtTokenService.cs` — GenerateToken

### Application Layer
- `src/TaskManagement.Application/DTOs/` — All request/response DTOs
- `src/TaskManagement.Application/Services/AuthService.cs` — Registration + login logic
- `src/TaskManagement.Application/Services/TaskService.cs` — Task CRUD + validation
- `src/TaskManagement.Application/Interfaces/IAuthService.cs`
- `src/TaskManagement.Application/Interfaces/ITaskService.cs`

### Infrastructure Layer
- `src/TaskManagement.Infrastructure/Data/SqliteConnectionFactory.cs`
- `src/TaskManagement.Infrastructure/Repositories/UserRepository.cs` — ADO.NET + raw parameterized SQL
- `src/TaskManagement.Infrastructure/Repositories/TaskRepository.cs` — ADO.NET + raw parameterized SQL
- `src/TaskManagement.Infrastructure/Auth/BcryptPasswordHasher.cs`
- `src/TaskManagement.Infrastructure/Auth/JwtTokenService.cs`
- `src/TaskManagement.Infrastructure/Data/DatabaseInitializer.cs` — Runs SQL scripts on startup

### Web API
- `src/TaskManagement.WebAPI/Program.cs` — DI, auth, CORS, Swagger config
- `src/TaskManagement.WebAPI/Controllers/AuthController.cs`
- `src/TaskManagement.WebAPI/Controllers/TasksController.cs`
- `src/TaskManagement.WebAPI/Controllers/UsersController.cs`
- `src/TaskManagement.WebAPI/Controllers/HealthController.cs`
- `src/TaskManagement.WebAPI/Middleware/ExceptionHandlingMiddleware.cs`

### Tests
- `tests/TaskManagement.Domain.Tests/` — Entity validation tests
- `tests/TaskManagement.Application.Tests/` — AuthService + TaskService tests (mocked dependencies)
- `tests/TaskManagement.Infrastructure.Tests/` — Repository integration tests, JWT/hasher unit tests
- `tests/TaskManagement.WebAPI.Tests/` — Controller unit tests + integration tests

### Frontend
- `src/task-management-frontend/src/services/api.ts` — Axios client with JWT interceptor
- `src/task-management-frontend/src/services/authService.ts` — Login/register API calls
- `src/task-management-frontend/src/services/taskService.ts` — Task CRUD API calls
- `src/task-management-frontend/src/context/AuthContext.tsx` — JWT state management
- `src/task-management-frontend/src/pages/LoginPage.tsx`
- `src/task-management-frontend/src/pages/RegisterPage.tsx`
- `src/task-management-frontend/src/pages/TaskBoardPage.tsx`
- `src/task-management-frontend/src/components/TaskCard.tsx`
- `src/task-management-frontend/src/components/TaskForm.tsx`

---

## Verification
1. Run `dotnet test` from solution root — all unit + integration tests pass
2. Start API (`dotnet run`), call `GET /api/health` — returns 200 (no auth)
3. Call `GET /api/tasks` without token — returns 401 Unauthorized
4. `POST /api/auth/register` with new user — returns 201
5. `POST /api/auth/login` with seeded credentials (`admin` / `Admin123!`) — returns JWT
6. Use JWT to `POST /api/tasks` — returns 201 with created task
7. `GET /api/tasks` with JWT — returns list including new task
8. `PUT /api/tasks/{id}` — updates task, verify with GET
9. `DELETE /api/tasks/{id}` — returns 204, verify task removed
10. Start React frontend (`npm run dev`), login with seeded credentials
11. Verify task board displays tasks in columns, CRUD operations work via UI
12. Verify responsive layout on mobile viewport

---

## Decisions
- **SQLite** — lightweight, file-based, zero-install database; raw parameterized SQL for all data access (no stored procedures)
- **JWT Bearer tokens** for stateless auth — standard SPA pattern
- **`TaskItem`** as entity name to avoid collision with `System.Threading.Tasks.Task`
- **BCrypt** for password hashing (industry standard, adaptive cost factor)
- **No MediatR** — services called directly from controllers per assignment constraint
- **DatabaseInitializer** runs SQL scripts on startup for easy setup (no manual DB setup needed)
- **Scope**: Single-role system (all authenticated users can manage all tasks). Role-based access deliberately excluded for simplicity.
