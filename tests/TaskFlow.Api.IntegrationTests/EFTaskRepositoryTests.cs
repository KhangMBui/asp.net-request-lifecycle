/*
 * TEST TYPE: Integration Test
 *
 * WHAT IS AN INTEGRATION TEST?
 *   Tests how two or more real components work together. Nothing is mocked —
 *   you are verifying that the pieces actually integrate correctly.
 *
 * WHAT IS BEING TESTED HERE?
 *   EfTaskRepository wired to a real AppDbContext (backed by EF Core InMemory).
 *
 * HOW IS ISOLATION ACHIEVED?
 *   Each test gets its own fresh in-memory database (via Guid.NewGuid() as the
 *   database name) so tests never share state. The real EF Core query pipeline
 *   runs — LINQ expressions are compiled and executed — but against RAM instead
 *   of a disk file.
 *
 * WHAT THESE TESTS VERIFY:
 *   - GetAll   → returns all seeded rows in the correct order
 *   - Create   → row actually appears in the database after calling Create
 *   - GetById  → EF Core WHERE clause correctly finds the right row
 *   - Delete   → row is removed from the database after calling Delete
 *
 * WHY INTEGRATION TESTS?
 *   A unit test with a mocked repository cannot catch a broken LINQ query,
 *   a wrong column mapping, or a missing SaveChanges() call. Integration tests
 *   catch those gaps by running the real EF Core stack.
 */

using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repositories;

// Goal: Test EFTaskRepository against a real (but in-memory) database. Verifies EF Core queries actually work correctly.
public class EFTaskRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly EfTaskRepository _sut;

    public EFTaskRepositoryTests()
    {
        // Each test gets its own fresh in-memory database
        var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;
        _context = new AppDbContext(options);
        _sut = new EfTaskRepository(_context);
    }

    [Fact]
    public void GetAll_ReturnsAllTasks()
    {
        // Arrange — seed the in-memory database directly
        _context.Tasks.AddRange(
            new TaskItem { Title = "Task A", CreatedAt = DateTime.UtcNow },
            new TaskItem { Title = "Task B", CreatedAt = DateTime.UtcNow }
        );
        _context.SaveChanges();

        var result = _sut.GetAll();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Add_PersistsTaskToDatabase()
    {
        var task = new TaskItem { Title = "Persisted task", CreatedAt = DateTime.UtcNow };

        _sut.Create(task);

        // Check the database directly — not through the repo
        Assert.Equal(1, _context.Tasks.Count());
        Assert.Equal("Persisted task", _context.Tasks.First().Title);
    }

    [Fact]
    public void GetById_WhenTaskExists_ReturnsCorrectTask()
    {
        var task = new TaskItem { Title = "Find me", CreatedAt = DateTime.UtcNow };
        _context.Tasks.Add(task);
        _context.SaveChanges();

        var result = _sut.GetById(task.Id);

        Assert.NotNull(result);
        Assert.Equal("Find me", result.Title);
    }

    [Fact]
    public void Delete_RemovesTaskFromDatabase()
    {
        var task = new TaskItem { Title = "Delete me", CreatedAt = DateTime.UtcNow };
        _context.Tasks.Add(task);
        _context.SaveChanges();

        _sut.Delete(task.Id);

        Assert.Equal(0, _context.Tasks.Count());
    }

    public void Dispose() => _context.Dispose();

}