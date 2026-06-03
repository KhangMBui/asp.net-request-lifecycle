/*
 * REPOSITORY IMPLEMENTATION — InMemoryTaskRepository
 *
 * Stores tasks in a static List<TaskItem> in memory — no database involved.
 * All data is lost when the app restarts.
 *
 * The `static` keyword means every request shares the same list for the app's lifetime,
 * simulating a shared database. This is useful for early development or unit tests
 * where spinning up a real database is unnecessary overhead.
 *
 * To switch back to this from EF Core, change Program.cs to:
 *   builder.Services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();
 *   (Singleton so the same list is shared across all requests.)
 */

using TaskFlow.Api.Models;

namespace TaskFlow.Api.Repositories;

public class InMemoryTaskRepository : ITaskRepository
{
    private static readonly List<TaskItem> Tasks =
    [
        new TaskItem
        {
            Id = 1,
            Title = "Learn ASP.NET Core controllers",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        },
        new TaskItem
        {
            Id = 2,
            Title = "Understand routing and HTTP responses",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        }
    ];

    public List<TaskItem> GetAll()
    {
        return Tasks.ToList();
    }

    public TaskItem? GetById(int id)
    {
        return Tasks.FirstOrDefault(task => task.Id == id);
    }

    public TaskItem Create(TaskItem task)
    {
        task.Id = Tasks.Count == 0
            ? 1
            : Tasks.Max(existingTask => existingTask.Id) + 1;

        task.CreatedAt = DateTime.UtcNow;

        Tasks.Add(task);

        return task;
    }

    public bool Update(TaskItem task)
    {
        var existingTask = GetById(task.Id);

        if (existingTask == null)
        {
            return false;
        }

        existingTask.Title = task.Title;
        existingTask.IsCompleted = task.IsCompleted;

        return true;
    }

    public bool Delete(int id)
    {
        var task = GetById(id);

        if (task == null)
        {
            return false;
        }

        Tasks.Remove(task);

        return true;
    }
}