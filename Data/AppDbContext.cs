/*
 * DATA — AppDbContext
 *
 * What is Entity Framework Core (EF Core)?
 *   An ORM (Object-Relational Mapper) that lets you interact with a relational database
 *   using C# objects and LINQ instead of raw SQL. You write queries against DbSet<T>,
 *   and EF Core translates them into the appropriate SQL at runtime.
 *
 * What is DbContext?
 *   DbContext is EF Core's main class — it represents one session with the database.
 *   It handles:
 *     - Querying data (LINQ → SQL SELECT)
 *     - Tracking entity changes (knows which objects were added, modified, or deleted)
 *     - Saving changes (SaveChanges() → SQL INSERT / UPDATE / DELETE)
 *
 * DbSet<TaskItem> Tasks:
 *   Represents the "Tasks" table. Query it with LINQ (.Where, .FirstOrDefault, .OrderBy...),
 *   and EF Core generates the corresponding SQL automatically.
 *
 * Migrations (the Migrations/ folder):
 *   Auto-generated files that track schema changes over time.
 *   - `dotnet ef migrations add <Name>`  → creates a new migration file
 *   - `dotnet ef database update`        → applies pending migrations to the database
 */

using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Data;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>(); // TaskItem objects map to a Task table.
}