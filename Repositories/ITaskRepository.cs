/*
 * REPOSITORY INTERFACE — ITaskRepository
 *
 * What is the Repository Pattern?
 *   A repository abstracts all data access behind a simple interface.
 *   Instead of writing EF Core queries or SQL directly in your service,
 *   you call methods like GetAll() or Create() and the repository handles the details.
 *
 *   Benefits:
 *     - Swap databases without touching business logic
 *       (just register a different implementation in Program.cs)
 *     - Easy to mock in unit tests — no database required
 *     - Services stay focused on logic, not on how data is fetched
 *
 * Two implementations exist in this app:
 *   InMemoryTaskRepository — tasks live in a C# List (no database, lost on restart)
 *   EfTaskRepository       — tasks are persisted to SQLite via Entity Framework Core
 */

using TaskFlow.Api.Models;

namespace TaskFlow.Api.Repositories;

public interface ITaskRepository
{
    List<TaskItem> GetAll();

    TaskItem? GetById(int id);

    TaskItem Create(TaskItem task);

    bool Update(TaskItem task);

    bool Delete(int id);
}