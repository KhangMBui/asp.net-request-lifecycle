/*
 * TEST TYPE: Unit Test
 *
 * WHAT IS A UNIT TEST?
 *   Tests a single class (the "unit") in complete isolation.
 *   Every external dependency is replaced with a fake (a "mock") so the test
 *   only exercises the logic inside the class itself — nothing else.
 *
 * WHAT IS BEING TESTED HERE?
 *   TaskService — the business logic layer.
 *
 * HOW IS ISOLATION ACHIEVED?
 *   ITaskRepository is replaced with a Moq mock. The mock is programmed to
 *   return specific data or verify specific calls, so no database is touched.
 *
 * WHAT THESE TESTS VERIFY:
 *   - GetAll        → passes through whatever the repository returns
 *   - GetById       → returns the task when found; throws NotFoundException when not
 *   - Create        → maps the DTO correctly and calls repository.Create exactly once
 *   - Delete        → throws NotFoundException when the task does not exist
 *
 * WHY UNIT TESTS?
 *   They run in milliseconds, require no infrastructure, and pinpoint exactly
 *   which method broke. Write these first — they are the cheapest safety net.
 */

using Moq;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Exceptions;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repositories;
using TaskFlow.Api.Services;
using Xunit;


// Goal: Test TaskService in total isolation. Fake the repository with Moq so the test has zero external dependencies — no database, no HTTP
public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repoMock;
    private readonly TaskService _sut; // sut = system under test

    public TaskServiceTests()
    {
        _repoMock = new Mock<ITaskRepository>();
        _sut = new TaskService(_repoMock.Object);
    }

    // [Fact] marks a method as a standard unit test case
    [Fact]
    public void GetAll_ReturnsAllTasksFromRepository()
    {
        // Arrange - set up what the fake repo should return
        var tasks = new List<TaskItem>
        {
            new() { Id = 1, Title = "Buy milk", IsCompleted = false },
            new() { Id = 2, Title = "Write tests", IsCompleted = true },
        };
        _repoMock.Setup(r => r.GetAll()).Returns(tasks);

        // Act - call the real service method
        var result = _sut.GetAll();

        // Assert - verify the output
        Assert.Equal(2, result.Count);
        Assert.Equal("Buy milk", result[0].Title);
    }

    [Fact]
    public void GetById_WhenTaskExists_ReturnsTask()
    {
        // Arrange
        var task = new TaskItem { Id = 1, Title = "Buy coke" };
        _repoMock.Setup(r => r.GetById(1)).Returns(task);

        // Act
        var result = _sut.GetById(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Buy coke", result.Title);
    }

    [Fact]
    public void GetById_WhenTaskDoesNotExist_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetById(99)).Returns((TaskItem?)null);

        // Assert.Throws verifies the exception is thrown
        Assert.Throws<NotFoundException>(() => _sut.GetById(99));
    }

    [Fact]
    public void Create_MapsRequestToTaskitemAndCallsRepo()
    {
        var request = new CreateTaskRequest { Title = "New task", IsCompleted = false };

        // We don't care what the repo returns here, just that it was called
        _repoMock.Setup(r => r.Create(It.IsAny<TaskItem>())).Verifiable();

        _sut.Create(request);

        // Verify the repo's Create method was called exactly once
        _repoMock.Verify(r => r.Create(It.Is<TaskItem>(t => t.Title == "New task")), Times.Once);
    }

    [Fact]
    public void Delete_WhenTaskDoesNotExist_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetById(99)).Returns((TaskItem?)null);

        Assert.Throws<NotFoundException>(() => _sut.Delete(99));
    }
}