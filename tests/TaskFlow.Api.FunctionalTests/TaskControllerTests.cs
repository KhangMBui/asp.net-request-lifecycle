/*
 * TEST TYPE: Functional Test
 *
 * WHAT IS A FUNCTIONAL TEST?
 *   Tests the entire application from the outside, exactly as a real client
 *   would. An HTTP request goes in, an HTTP response comes out. Every layer
 *   runs: routing, middleware, filters, controller, service, and repository.
 *
 * WHAT IS BEING TESTED HERE?
 *   The TaskController HTTP API surface — status codes, response shapes, and
 *   end-to-end behavior through the full request pipeline.
 *
 * HOW IS THE APP STARTED?
 *   TaskApiFactory (see TaskApiFactory.cs) boots the real Program.cs in memory
 *   and provides an HttpClient. IClassFixture<TaskApiFactory> means all tests
 *   in this class share one factory instance (and one database).
 *
 * WHAT THESE TESTS VERIFY:
 *   - GET  /api/task        → 200 OK with an empty list on a fresh database
 *   - POST /api/task        → 201 Created; response body contains the new task
 *   - GET  /api/task/9999   → 404 Not Found with ApiErrorResponse shape
 *   - DELETE /api/task/9999 → 404 Not Found when task does not exist
 *
 * WHAT UNIT/INTEGRATION TESTS CANNOT CATCH THAT THIS CAN:
 *   - Wrong HTTP status code (controller returning 200 instead of 201)
 *   - Routing misconfiguration (wrong verb or path)
 *   - Middleware bugs (GlobalExceptionMiddleware not converting exceptions to 404)
 *   - ResponseWrapperFilter not wrapping the body in ApiResponse<T>
 *
 * NOTE ON TEST ISOLATION:
 *   All tests share one in-memory database. A task created by one test is
 *   visible to tests that run after it. Keep this in mind when adding new
 *   tests that assert on counts or empty-state assumptions.
 */

using System.Net;
using System.Net.Http.Json;
using TaskFlow.Api.Common;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;

public class TaskControllerTests : IClassFixture<TaskApiFactory>
{
    private readonly HttpClient _client;

    public TaskControllerTests(TaskApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GET_Tasks_ReturnsEmptyList_WhenNoTasksExist()
    {
        var response = await _client.GetAsync("/api/task");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<TaskItem>>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.Empty(body.Data);
    }

    [Fact]
    public async Task POST_Task_CreatesAndReturnsNewTask()
    {
        var request = new CreateTaskRequest { Title = "Functional test task" };

        var response = await _client.PostAsJsonAsync("/api/task", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<TaskItem>>();
        Assert.NotNull(body);
        Assert.Equal("Functional test task", body.Data.Title);
        Assert.False(body.Data.IsCompleted);
    }

    [Fact]
    public async Task GET_Task_ById_WhenNotFound_Returns404WithErrorShape()
    {
        var response = await _client.GetAsync("/api/task/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // Verify your GlobalExceptionMiddleware returned the correct error shape
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Equal(404, body.StatusCode);
    }

    [Fact]
    public async Task DELETE_Task_Returns404_WhenTaskDoesNotExist()
    {
        var response = await _client.DeleteAsync("/api/task/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
