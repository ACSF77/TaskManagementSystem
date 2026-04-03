# Task Management System

A full-stack task management application with a Kanban-style board. Built with **ASP.NET Core 8 Web API** (Clean Architecture, raw SQL with SQLite) and a **React + TypeScript** (Vite) frontend.

---

## Architecture

```
src/
├── TaskManagement.Domain          # Entities, Enums, Interfaces (no dependencies)
├── TaskManagement.Application     # DTOs, Service interfaces/implementations, Exceptions
├── TaskManagement.Infrastructure  # SQLite repositories (raw ADO.NET), BCrypt, JWT
├── TaskManagement.WebAPI          # Controllers, Middleware, DI configuration
└── task-management-frontend/      # React + TypeScript + Vite (Kanban board)

tests/
├── TaskManagement.Domain.Tests
├── TaskManagement.Application.Tests
├── TaskManagement.Infrastructure.Tests
└── TaskManagement.WebAPI.Tests
```

**Key design decisions:**
- **Clean Architecture** — dependencies flow inward (Domain → Application → Infrastructure → WebAPI)
- **No Entity Framework, Dapper, or MediatR** — raw parameterized SQL via `Microsoft.Data.Sqlite`
- **JWT Bearer authentication** with BCrypt password hashing
- **77 unit/integration tests** across all layers (xUnit + Moq + FluentAssertions)

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or later)
- [Node.js 18+](https://nodejs.org/) with npm

---

## Getting Started

### 1. Clone & restore

```bash
git clone https://github.com/ACSF77/TaskManagementSystem
cd TaskManagementSystem
dotnet restore
```

### 2. Run backend tests

```bash
dotnet test
```

All 77 tests should pass.

### 3. Start the API

```bash
cd src/TaskManagement.WebAPI
dotnet run
```

The API starts at **http://localhost:5231**. Swagger UI is available at http://localhost:5231/swagger.

On first run the database (`taskmanagement.db`) is automatically created and seeded with demo data.

### 4. Start the React frontend

```bash
cd src/task-management-frontend
npm install
npm run dev
```

The frontend starts at **http://localhost:5173**. Open it in your browser.

---

## Demo Credentials

| Username   | Password    |
|------------|-------------|
| `admin`    | `Admin123!` |
| `johndoe`  | `John123!`  |

---

## API Endpoints

| Method | Endpoint           | Auth     | Description                 |
|--------|--------------------|----------|-----------------------------|
| POST   | `/api/auth/register` | No     | Register a new user         |
| POST   | `/api/auth/login`    | No     | Login, returns JWT token    |
| GET    | `/api/tasks`         | Yes    | List all tasks              |
| GET    | `/api/tasks/{id}`    | Yes    | Get task by ID              |
| POST   | `/api/tasks`         | Yes    | Create a new task           |
| PUT    | `/api/tasks/{id}`    | Yes    | Update a task               |
| DELETE | `/api/tasks/{id}`    | Yes    | Delete a task               |
| GET    | `/api/users`         | Yes    | List all users              |
| GET    | `/api/health`        | No     | Health check                |

Include the JWT token in the `Authorization: Bearer <token>` header for protected endpoints.

---

## Frontend Features

- **Login / Register** pages with form validation
- **Kanban board** with three columns: To Do, In Progress, Done
- **Drag and drop** tasks between columns (updates status via API)
- **Create / Edit / Delete** tasks with a modal form
- **Responsive** layout (mobile-friendly single-column view)
- Overdue tasks highlighted with a red border

---

## Tech Stack

**Backend:** .NET 8, ASP.NET Core Web API, SQLite (raw ADO.NET), BCrypt.Net, JWT  
**Frontend:** React 18, TypeScript, Vite, @hello-pangea/dnd, React Router  
**Testing:** xUnit, Moq, FluentAssertions, WebApplicationFactory

---

## Development Process with GitHub Copilot

This project was built entirely through an interactive session with **GitHub Copilot** (agent mode). The full implementation plan, including architecture decisions, phased steps, and file inventory, is documented in [`plan.md`](plan.md).

### How Copilot was used

Copilot acted as an automated coding agent — scaffolding the solution, implementing each layer with TDD, and building the React frontend. The workflow followed a conversational loop:

1. **Requirements gathering** — Copilot asked clarifying questions before writing any code (database choice, auth strategy, frontend tooling, user story theme).
2. **Plan generation** — A detailed phased plan was produced and reviewed before implementation began.
3. **Iterative implementation** — Each phase was executed sequentially (Domain → Application → Infrastructure → WebAPI → Frontend), with tests written alongside production code.
4. **Bug diagnosis & fixes** — When issues arose (e.g., invalid GUID hex characters in seed data, file-path resolution in integration tests), Copilot diagnosed and fixed them within the session.

### Key decisions made during the session

| Decision | Context |
|----------|---------|
| **SQLite with raw parameterized SQL** | Assignment prohibited EF, Dapper, and MediatR. Initially considered SQL Server LocalDB, but switched to SQLite for zero-install simplicity. No stored procedures — all queries are inline parameterized SQL. |
| **Clean Architecture (4 layers)** | Domain has no dependencies; Application depends only on Domain; Infrastructure implements Domain interfaces; WebAPI wires everything with DI. |
| **JWT Bearer authentication** | Standard stateless auth pattern for SPAs. BCrypt chosen for password hashing (adaptive cost factor). |
| **`TaskItem` entity name** | Avoids collision with `System.Threading.Tasks.Task`. |
| **Kanban board with drag-and-drop** | Board columns map to the `TaskItemStatus` enum. Dragging a card between columns fires a `PUT /api/tasks/{id}` call to persist the status change. |
| **No role-based access** | Deliberately kept single-role (all authenticated users can manage all tasks) to match assignment scope. |
| **`DatabaseInitializer` with inline SQL** | Schema and seed SQL are embedded in C# code rather than read from `.sql` files, avoiding working-directory issues in integration tests. |

The full plan with solution structure, database schema, API endpoints, implementation phases, and verification checklist is available in [`plan.md`](plan.md).
