/*
 * CONTROLLER — TaskController
 *
 * What is a Controller?
 *   A controller is the entry point for HTTP requests. Its only jobs are:
 *     1. Parse and validate the incoming request (handled largely by [ApiController])
 *     2. Call the appropriate service method
 *     3. Return the right HTTP response (Ok, NotFound, Created, etc.)
 *
 *   Controllers should NOT contain business logic or database code — those belong
 *   in Services and Repositories respectively.
 *
 * Key attributes:
 *   [ApiController]              — enables automatic model validation, returns 400 if
 *                                  [Required]/[MaxLength] etc. fail before the action runs;
 *                                  also infers [FromBody] / [FromRoute] automatically.
 *   [Route("api/[controller]")]  — sets the base URL to /api/task
 *                                  ([controller] is replaced with the class name minus "Controller")
 *   [HttpGet], [HttpPost], etc.  — map each method to an HTTP verb + optional route segment
 *
 * ControllerBase:
 *   Provides helper methods that return IActionResult with the correct status code:
 *     Ok(data)              → 200
 *     CreatedAtAction(...)  → 201 + Location header
 *     NoContent()           → 204
 *     NotFound(...)         → 404
 *
 * This controller handles all CRUD operations for tasks:
 *   GET    /api/task        → GetAll
 *   GET    /api/task/{id}   → GetById
 *   GET    /api/task/crash  → Crash (throws an exception to test GlobalExceptionMiddleware)
 *   POST   /api/task        → Create
 *   PUT    /api/task/{id}   → Update
 *   DELETE /api/task/{id}   → Delete
 */

using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TaskController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    [EndpointSummary("Get all tasks")]
    [EndpointDescription("Returns the full list of tasks.")]
    [ProducesResponseType<List<TaskItem>>(StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var tasks = _taskService.GetAll();
        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Get a task by ID")]
    [EndpointDescription("Returns a single task matching the given ID.")]
    [ProducesResponseType<TaskItem>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(int id)
    {
        var task = _taskService.GetById(id);

        if (task == null)
        {
            return NotFound(new
            {
                message = $"Task with id {id} was not found."
            });
        }

        return Ok(task);
    }

    [HttpGet("crash")]
    [EndpointSummary("Crash test endpoint")]
    [EndpointDescription("Throws an exception to test global exception middleware.")]
    public IActionResult Crash()
    {
        throw new Exception("This is a test exception.");
    }

    [HttpPost]
    [EndpointSummary("Create a new task")]
    [EndpointDescription("Creates a new task. The ID and creation timestamp are assigned automatically by the server.")]
    [ProducesResponseType<TaskItem>(StatusCodes.Status201Created)]
    public IActionResult Create([FromBody] CreateTaskRequest request)
    {
        var newTask = _taskService.Create(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = newTask.Id },
            newTask
        );
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Update a task")]
    [EndpointDescription("Updates the title and completion status of an existing task.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, [FromBody] UpdateTaskRequest request)
    {
        var updated = _taskService.Update(id, request);

        if (!updated)
        {
            return NotFound(new
            {
                message = $"Task with id {id} was not found."
            });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [EndpointSummary("Delete a task")]
    [EndpointDescription("Permanently removes the task with the given ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        var deleted = _taskService.Delete(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = $"Task with id {id} was not found."
            });
        }

        return NoContent();
    }
}