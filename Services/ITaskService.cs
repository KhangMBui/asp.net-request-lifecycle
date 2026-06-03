/*
 * SERVICE INTERFACE — ITaskService
 *
 * What is a Service?
 *   The service layer holds business logic — the rules and operations that define
 *   what the app actually does. It sits between the controller (HTTP layer) and
 *   the repository (data layer):
 *
 *     Controller → Service → Repository → Database
 *
 *   The controller's job is only to parse the request and return a response.
 *   The service's job is to decide what to do. The repository's job is to store/retrieve data.
 *   Keeping these concerns separate makes each layer easy to test and change independently.
 *
 * Why an interface?
 *   Defining ITaskService lets the DI container inject whichever concrete implementation
 *   is registered in Program.cs — without the controller ever knowing the difference.
 *   This also makes it trivial to swap in a mock during unit tests.
 */

using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

public interface ITaskService
{
    List<TaskItem> GetAll();

    TaskItem? GetById(int id);

    TaskItem Create(CreateTaskRequest request);

    bool Update(int id, UpdateTaskRequest request);

    bool Delete(int id);
}