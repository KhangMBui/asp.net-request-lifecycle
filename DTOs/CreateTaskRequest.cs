/*
 * DTO — CreateTaskRequest
 *
 * What is a DTO (Data Transfer Object)?
 *   A DTO is a class that defines exactly what data flows into or out of an endpoint.
 *   Instead of accepting the full TaskItem as the request body (which would expose Id
 *   and CreatedAt as editable fields), you define only the fields the client is allowed to provide.
 *
 *   For POST /api/task:
 *     - Id is excluded       — assigned by the database automatically
 *     - CreatedAt is excluded — set to DateTime.UtcNow by the server
 *
 * Validation attributes ([Required], [MaxLength]) are enforced automatically by ASP.NET Core
 * when [ApiController] is on the controller. If validation fails, a 400 Bad Request is returned
 * before the controller action body even runs.
 */

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs;

public class CreateTaskRequest
{
    [Required]
    [MaxLength(200)]
    [Description("The title of the task.")]
    public string Title { get; set; } = string.Empty;

    [Description("Whether the task is already completed. Defaults to false.")]
    public bool IsCompleted { get; set; } = false;
}
