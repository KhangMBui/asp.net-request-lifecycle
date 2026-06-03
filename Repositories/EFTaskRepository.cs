/*
 * REPOSITORY IMPLEMENTATION — EfTaskRepository
 *
 * Persists tasks to a SQLite database using Entity Framework Core (EF Core).
 *
 * What is EF Core?
 *   An ORM (Object-Relational Mapper) that lets you work with a relational database
 *   using C# objects and LINQ instead of raw SQL. You write queries against DbSet<T>
 *   and EF Core translates them to SQL at runtime.
 *
 * AppDbContext is injected here — it represents the database session for the current
 * HTTP request (Scoped lifetime). SaveChanges() flushes all tracked changes to the DB.
 *
 * Change tracking:
 *   When you load an entity from the context (e.g. via FirstOrDefault), EF Core tracks it.
 *   Mutating properties on that object and calling SaveChanges() is enough to persist the
 *   update — you don't need to call an explicit Update() method on the DbSet.
 */

using TaskFlow.Api.Data;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Repositories;

public class EfTaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public EfTaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<TaskItem> GetAll()
    {
        return _context.Tasks
            .OrderByDescending(task => task.CreatedAt)
            .ToList();
    }

    public TaskItem? GetById(int id)
    {
        return _context.Tasks.FirstOrDefault(task => task.Id == id);
    }

    public TaskItem Create(TaskItem task)
    {
        task.CreatedAt = DateTime.UtcNow;

        _context.Tasks.Add(task);
        _context.SaveChanges();

        return task;
    }

    public bool Update(TaskItem task)
    {
        var existingTask = _context.Tasks.FirstOrDefault(existingTask => existingTask.Id == task.Id);

        if (existingTask == null)
        {
            return false;
        }

        existingTask.Title = task.Title;
        existingTask.IsCompleted = task.IsCompleted;

        _context.SaveChanges();

        return true;
    }

    public bool Delete(int id)
    {
        var task = _context.Tasks.FirstOrDefault(task => task.Id == id);

        if (task == null)
        {
            return false;
        }

        _context.Tasks.Remove(task);
        _context.SaveChanges();

        return true;
    }
}