# TaskFlow.Api

A task management REST API built with ASP.NET Core 9 — used as a hands-on reference for understanding the ASP.NET request lifecycle, dependency injection, and the layered architecture of a typical Web API.

---

## Table of Contents

1. [The Request Lifecycle](#the-request-lifecycle)
2. [Middleware](#middleware)
3. [Filters](#filters)
4. [Middleware vs Filters](#middleware-vs-filters)
5. [Dependency Injection](#dependency-injection)
6. [Layered Architecture](#layered-architecture)
7. [Custom Exceptions](#custom-exceptions)
8. [Response Envelope Pattern](#response-envelope-pattern)
9. [Entity Framework Core](#entity-framework-core)
10. [Project Structure](#project-structure)

---

## The Request Lifecycle

When a client sends an HTTP request to an ASP.NET Core API, it travels through a well-defined sequence of stages before a response is returned. Understanding this order is the key to knowing *where* to put code.

```
HTTP Request (browser / Postman / client)
  ↓
Kestrel
  — the built-in web server; receives raw TCP bytes and parses them into an HttpContext
  ↓
Middleware Pipeline  (runs in registration order)
  ├─ RequestLoggingMiddleware   — logs method + path
  └─ GlobalExceptionMiddleware  — wraps everything below in try/catch
  ↓
UseHttpsRedirection             — redirects HTTP → HTTPS
  ↓
UseAuthorization                — evaluates authorization policies
  ↓
MapControllers                  — matches the URL to a controller action
  ↓
Filters  (MVC layer — only for controller actions)
  ├─ Authorization Filters      — (none here, but would run first)
  ├─ Resource Filters           — (none here)
  ├─ Action Filters (before)    — ActionLoggingFilter logs action name
  ↓
Model Binding & Validation
  — [ApiController] binds route/body/query params and runs [Required], [MaxLength] etc.
  — returns 400 automatically if validation fails (before the action body runs)
  ↓
Controller Action executes
  ↓
Service Layer                   — business logic
  ↓
Repository Layer                — data access
  ↓
Database (SQLite via EF Core)
  ↓
Response travels back UP
  ↓
Action Filters (after)          — ActionLoggingFilter logs elapsed time + result type
  ↓
Result Filters                  — ResponseWrapperFilter wraps 2xx in ApiResponse<T>
  ↓
Exception Filters               — (none here; handled by middleware instead)
  ↓
Back through Middleware (in reverse)
  ├─ GlobalExceptionMiddleware  — catches any thrown exception, writes ApiErrorResponse
  └─ RequestLoggingMiddleware   — logs status code + elapsed ms
  ↓
HTTP Response sent to client
```

> **Key rule:** Middleware wraps the entire pipeline. Filters only wrap controller actions. Code that needs to run for *every* request (health checks, static files, etc.) belongs in middleware. Code that is specific to controller behavior belongs in filters.

---

## Middleware

Middleware is code that sits in the HTTP pipeline and runs for **every request**, regardless of which endpoint is hit. Each middleware:

- Receives the current `HttpContext`
- Can inspect or modify the request before passing it on
- Calls `await _next(context)` to hand off to the next middleware
- Can inspect or modify the response after `_next` returns

```
Request in
  ↓
[Middleware A - before logic]
  ↓
  await _next(context)  →  [Middleware B]  →  [Controller]
  ↓                              ↑
[Middleware A - after logic] ←──┘
  ↓
Response out
```

Middleware is registered in `Program.cs` with `app.UseMiddleware<T>()` and runs **in the exact order it is added**.

### Middleware in this app

| Middleware | Purpose |
|---|---|
| `RequestLoggingMiddleware` | Logs `{Method} {Path}` on the way in; logs `{StatusCode}` and elapsed ms on the way out |
| `GlobalExceptionMiddleware` | Wraps the entire pipeline in `try/catch`; returns a structured `ApiErrorResponse` instead of a raw stack trace |

### Short-circuiting

A middleware can choose *not* to call `_next`. This stops the pipeline and sends a response immediately. For example, an auth middleware might return 401 without ever reaching the controller. This is called **short-circuiting**.

---

## Filters

Filters are ASP.NET Core's finer-grained hooks that run **only around MVC controller actions**. They do not run for static files, health checks, or other non-controller middleware endpoints.

There are five filter types, executed in this order:

```
Request arrives at the MVC layer
  ↓
1. Authorization Filters   — canActivate check; short-circuits with 401/403 if denied
  ↓
2. Resource Filters        — run around model binding; useful for output caching
  ↓
3. [Model Binding happens here]
  ↓
4. Action Filters (before) — run just before the action method body
  ↓
   Controller action executes
  ↓
5. Action Filters (after)  — run just after the action method returns
  ↓
6. Result Filters (before) — run just before the IActionResult is written to the response
  ↓
   IActionResult written to response
  ↓
7. Result Filters (after)
  ↓
8. Exception Filters       — catch unhandled exceptions from the action (not from middleware)
```

Filters can be applied:
- **Globally** — `options.Filters.Add<T>()` in `Program.cs` → applies to all controllers
- **Per-controller** — `[ServiceFilter(typeof(T))]` on the class
- **Per-action** — same attribute on a single method

### Filters in this app

| Filter | Type | Purpose |
|---|---|---|
| `ActionLoggingFilter` | `IAsyncActionFilter` | Logs action name before; logs elapsed time and result type after |
| `ResponseWrapperFilter` | `IResultFilter` | Wraps 2xx `ObjectResult` values in `ApiResponse<T>` before writing to the response |

---

## Middleware vs Filters

| | Middleware | Filters |
|---|---|---|
| Scope | Every request (the whole pipeline) | Controller actions only |
| Registered in | `Program.cs` with `app.Use...` | `Program.cs` via `options.Filters.Add` or attributes |
| Access to MVC context | No (only `HttpContext`) | Yes (`ActionExecutingContext`, `ResultExecutingContext`, etc.) |
| Good for | Auth, logging, HTTPS redirect, exception handling, CORS | Response shaping, action logging, authorization policies, validation |
| Execution order | The order they are added | Fixed: Authorization → Resource → Action → Result → Exception |

**Rule of thumb:** If it needs to run before routing even happens, use middleware. If it needs MVC-specific context (controller name, action name, result type), use a filter.

---

## Dependency Injection

ASP.NET Core has DI built in. Instead of classes creating their own dependencies with `new`, you declare what you need in the constructor and the framework injects it automatically.

### Registration (Program.cs)

```csharp
builder.Services.AddScoped<ITaskRepository, EfTaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
```

This tells the DI container: *"When something asks for `ITaskRepository`, create an `EfTaskRepository`."*

### Lifetimes

| Lifetime | Created | Destroyed | Use when |
|---|---|---|---|
| `AddSingleton` | Once, on first request | App shuts down | Stateless services, config, caches |
| `AddScoped` | Once per HTTP request | Request ends | Database contexts, repositories, services that touch the DB |
| `AddTransient` | Every time it's requested | After use | Lightweight, stateless utilities |

> **Why `AddScoped` for EF Core?** `AppDbContext` tracks entity state within a request. If it were a singleton, entity state from one request would bleed into the next. Scoped ensures each request gets its own clean context.

### Interface-based injection

Interfaces decouple the consumer from the implementation:

```
Controller  →  ITaskService  →  TaskService  →  ITaskRepository  →  EfTaskRepository
```

The controller only knows `ITaskService`. Swapping `EfTaskRepository` for `InMemoryTaskRepository` (for testing) requires changing one line in `Program.cs` — nothing else.

---

## Layered Architecture

This app follows a three-layer architecture. Each layer has a single responsibility:

```
┌─────────────────────────────────────────────┐
│  Controller (HTTP layer)                    │
│  Parse request → call service → return HTTP │
└────────────────────┬────────────────────────┘
                     │
┌────────────────────▼────────────────────────┐
│  Service (Business logic layer)             │
│  Rules, validation, DTO → Model mapping     │
└────────────────────┬────────────────────────┘
                     │
┌────────────────────▼────────────────────────┐
│  Repository (Data access layer)             │
│  CRUD operations — no business logic here   │
└────────────────────┬────────────────────────┘
                     │
              Database (SQLite)
```

### Why layer them?

- **Testability** — you can test a service by mocking the repository, without a database.
- **Replaceability** — swap SQLite for PostgreSQL by writing a new repository implementation.
- **Clarity** — it's always obvious where a piece of code belongs.

### DTOs vs Domain Models

| | Domain Model (`TaskItem`) | DTO (`CreateTaskRequest`) |
|---|---|---|
| Purpose | Represents data as it exists in the system | Represents data as it flows in/out of an endpoint |
| Contains | All fields including `Id`, `CreatedAt` | Only what the client is allowed to provide |
| Mapped by | EF Core to a database table | ASP.NET model binding from the request body |

The controller accepts DTOs, the service maps DTOs to models, and the repository persists models.

---

## Custom Exceptions

Rather than returning status codes as integers or checking nulls in the controller, services throw typed exceptions:

```csharp
throw new NotFoundException($"Task {id} not found.");
throw new BadRequestException("Title cannot be empty.");
throw new ConflictException("A task with this title already exists.");
```

`GlobalExceptionMiddleware` catches all exceptions and decides how to respond:

```
Exception thrown
  ↓
Is it an AppException?
  ├─ Yes → log as Warning, use exception.StatusCode, return exception.Message
  └─ No  → log as Error, return 500, return generic "An unexpected error occurred."
  ↓
Write ApiErrorResponse JSON to the response
```

This keeps controllers and services free of `try/catch` blocks, and centralizes all error formatting in one place.

---

## Response Envelope Pattern

All responses from this API share a consistent shape, making it easier for clients to handle them uniformly.

**Success (2xx) — wrapped by `ResponseWrapperFilter`:**
```json
{
  "success": true,
  "data": { "id": 1, "title": "Learn ASP.NET", "isCompleted": false, "createdAt": "..." },
  "timestamp": "2026-06-02T20:40:51.123Z"
}
```

**Error (4xx / 5xx) — written by `GlobalExceptionMiddleware`:**
```json
{
  "success": false,
  "message": "Task with id 99 was not found.",
  "statusCode": 404,
  "timestamp": "2026-06-02T20:40:51.123Z",
  "traceId": "0HN7K2..."
}
```

`traceId` is the request's unique identifier — you can search your logs for it to find every log line that belongs to a single failing request.

---

## Entity Framework Core

EF Core is an ORM (Object-Relational Mapper) that lets you interact with a relational database using C# objects and LINQ instead of raw SQL.

### Key concepts

| Concept | What it does |
|---|---|
| `DbContext` | Represents one database session; tracks entity changes; exposes `DbSet<T>` |
| `DbSet<T>` | Represents a table; query it with LINQ (`.Where`, `.FirstOrDefault`, `.OrderBy`) |
| Change tracking | When you load an entity and mutate it, EF Core detects the change automatically |
| `SaveChanges()` | Flushes all tracked changes to the database as SQL (INSERT / UPDATE / DELETE) |
| Migrations | Version-controlled schema changes — `dotnet ef migrations add <Name>` creates one, `dotnet ef database update` applies it |

### Query flow

```
LINQ query (.Where(...).ToList())
  ↓
EF Core translates to SQL
  ↓
SQL runs against SQLite
  ↓
Results mapped back to C# objects
  ↓
Objects tracked by DbContext
```

---

## Project Structure

```
TaskFlow.Api/
├── Controllers/          # HTTP entry points — parse request, call service, return response
├── Services/             # Business logic — rules, DTO→Model mapping
├── Repositories/         # Data access — CRUD only, no business logic
│   ├── ITaskRepository.cs
│   ├── InMemoryTaskRepository.cs   # No database (List<T>)
│   └── EFTaskRepository.cs         # SQLite via EF Core
├── Middleware/           # Pipeline-level concerns (logging, global exception handling)
├── Filters/              # Action/result-level concerns (response wrapping, action logging)
├── Models/               # Domain models / EF Core entities
├── DTOs/                 # Request/response shapes (what the client sends and receives)
├── Common/               # Shared response envelopes (ApiResponse<T>, ApiErrorResponse)
├── Exceptions/           # Custom exception hierarchy (AppException and subclasses)
├── Data/                 # EF Core DbContext
├── Migrations/           # Auto-generated EF Core schema migrations
└── Program.cs            # Composition root — DI registration + middleware pipeline
```
