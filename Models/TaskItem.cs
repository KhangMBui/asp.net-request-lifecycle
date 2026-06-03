/*
 * MODEL — TaskItem
 *
 * What is a Domain Model (Entity)?
 *   A domain model represents a core concept in your application — here, a single task.
 *   This is the object that EF Core maps to a database table (one class = one table,
 *   one property = one column), and what gets returned in API responses.
 *
 * Domain Model vs DTO:
 *   TaskItem              — the full object as it exists in the system (Id, Title, IsCompleted, CreatedAt)
 *   CreateTaskRequest     — what the client sends when creating (no Id or CreatedAt — server assigns those)
 *   UpdateTaskRequest     — what the client sends when updating (only the fields they're allowed to change)
 *
 * Keeping DTOs separate from the model means your API contract and your database schema
 * can evolve independently without breaking each other.
 */

using System.ComponentModel;

namespace TaskFlow.Api.Models;

public class TaskItem
{
    [Description("The unique identifier of the task.")]
    public int Id { get; set; }

    [Description("The title of the task.")]
    public string Title { get; set; } = string.Empty;

    [Description("Whether the task has been completed.")]
    public bool IsCompleted { get; set; }

    [Description("The UTC timestamp when the task was created.")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}