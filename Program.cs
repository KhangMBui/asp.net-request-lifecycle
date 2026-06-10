/*
 * PROGRAM.CS — Application Entry Point & Composition Root
 *
 * This file does two things:
 *   1. Registers services into the DI (Dependency Injection) container (builder.Services.Add...).
 *   2. Configures the middleware pipeline that every HTTP request walks through (app.Use...).
 *
 * DEPENDENCY INJECTION (DI):
 *   Instead of classes creating their own dependencies with `new`, ASP.NET Core creates
 *   and injects them automatically based on what you register here.
 *
 *   Lifetimes:
 *     AddSingleton  — one shared instance for the entire app lifetime
 *     AddScoped     — one instance per HTTP request (most common for DB work)
 *     AddTransient  — a brand-new instance every time it's requested
 *
 * REQUEST LIFECYCLE FOR THIS APP:
 *
 *   HTTP Request (browser / Postman / client)
 *     ↓
 *   Kestrel (built-in web server — receives and parses the raw HTTP bytes)
 *     ↓
 *   RequestLoggingMiddleware   — logs incoming method + path
 *     ↓
 *   GlobalExceptionMiddleware  — wraps everything below in a try/catch
 *     ↓
 *   UseHttpsRedirection        — redirects HTTP → HTTPS if needed
 *     ↓
 *   UseAuthorization           — checks permissions (not enforced yet in this app)
 *     ↓
 *   MapControllers             — routes to the matching controller action
 *     ↓
 *   ActionLoggingFilter        — logs which action is about to run (before)
 *     ↓
 *   Controller action runs     — e.g. TaskController.Create(...)
 *     ↓
 *   TaskService                — business logic
 *     ↓
 *   EfTaskRepository           — data access (LINQ → SQL)
 *     ↓
 *   AppDbContext → SQLite      — actual database read / write
 *     ↓
 *   ActionLoggingFilter        — logs elapsed time + result type (after)
 *     ↓
 *   ResponseWrapperFilter      — wraps 2xx responses in ApiResponse<T>
 *     ↓
 *   [response travels back UP through middleware]
 *     ↓
 *   GlobalExceptionMiddleware  — if an exception was thrown, returns ApiErrorResponse
 *     ↓
 *   RequestLoggingMiddleware   — logs outgoing status code + elapsed ms
 *     ↓
 *   HTTP Response sent back to client
 */

using TaskFlow.Api.Repositories;
using TaskFlow.Api.Services;
using TaskFlow.Api.Middleware;
using TaskFlow.Api.Filters;
using TaskFlow.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ActionLoggingFilter>();
    options.Filters.Add<ResponseWrapperFilter>();
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHealthChecks();

// in-memory data
// builder.Services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();

// EF Core database
builder.Services.AddScoped<ITaskRepository, EfTaskRepository>();

builder.Services.AddScoped<ITaskService, TaskService>(); // Use AddSingleton if in-memory data is used instead of EF Core


var app = builder.Build();

// Auto-apply migrations on startup so the DB schema exists when the container boots
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "TaskFlow API v1");
    });

}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapHealthChecks("/healthz");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Makes the auto-generated Program class visible to WebApplicationFactory in functional tests
public partial class Program { }
