/*
 * DTO — UpdateTaskRequest
 *
 * Used for PUT /api/task/{id}. Only exposes the fields the client is allowed to change.
 *
 *   - Id comes from the route parameter ({id}), not the body — prevents accidental ID changes.
 *   - CreatedAt is excluded — it should never change after the task is first created.
 *
 * See CreateTaskRequest for a general explanation of what DTOs are and why they exist.
 */

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs;

public class UpdateTaskRequest
{
    [Required]
    [MaxLength(200)]
    [Description("The updated title of the task.")]
    public string Title { get; set; } = string.Empty;

    [Description("The updated completion status of the task.")]
    public bool IsCompleted { get; set; }
}
