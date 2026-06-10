/*
 * SERVICE — TaskService
 *
 * Concrete implementation of ITaskService.
 *
 * Responsibilities:
 *   - Maps incoming DTOs (CreateTaskRequest, UpdateTaskRequest) to domain models (TaskItem).
 *   - Delegates data access to ITaskRepository (doesn't know or care if it's SQLite or in-memory).
 *   - This is where you'd add business rules, e.g.:
 *       "A task title must be unique"
 *       "A completed task cannot be re-opened"
 *
 * ITaskRepository is injected via the constructor by the DI container (registered in Program.cs).
 */

using TaskFlow.Api.DTOs;
using TaskFlow.Api.Exceptions;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repositories;

namespace TaskFlow.Api.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        this._taskRepository = taskRepository;
    }

    public List<TaskItem> GetAll()
    {
        return _taskRepository.GetAll();
    }

    public TaskItem? GetById(int id)
    {
        var task = _taskRepository.GetById(id);
        if (task == null) throw new NotFoundException($"Task {id} not found.");
        return task;
    }

    public TaskItem Create(CreateTaskRequest request)
    {
        var newTask = new TaskItem
        {
            Title = request.Title,
            IsCompleted = request.IsCompleted,
        };

        return _taskRepository.Create(newTask);
    }

    public bool Update(int id, UpdateTaskRequest request)
    {
        var task = _taskRepository.GetById(id);

        if (task == null)
        {
            return false;
        }

        task.Title = request.Title;
        task.IsCompleted = request.IsCompleted;

        return _taskRepository.Update(task);
    }

    public bool Delete(int id)
    {
        var task = _taskRepository.GetById(id);
        if (task == null) throw new NotFoundException($"Task {id} not found.");
        return _taskRepository.Delete(id);
    }
}