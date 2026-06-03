/*
 * COMMON — ApiResponse<T> (Success Envelope)
 *
 * What is a Response Envelope?
 *   Rather than returning raw data directly (just a TaskItem or a plain string),
 *   it's common practice to wrap all API responses in a consistent shape.
 *   This lets clients always expect the same top-level structure regardless of the endpoint.
 *
 * IApiResponse is a shared marker interface so both ApiResponse<T> (success) and
 * ApiErrorResponse (error) share a common type — useful for generic handling.
 *
 * ApiResponse<T> is used for all successful (2xx) responses.
 * ResponseWrapperFilter automatically wraps ObjectResult values in this shape.
 *
 * Example output:
 *   {
 *     "success": true,
 *     "data": { "id": 1, "title": "...", "isCompleted": false, ... },
 *     "timestamp": "2026-06-02T20:40:51.1234567Z"
 *   }
 */

namespace TaskFlow.Api.Common;

public interface IApiResponse
{
    bool Success { get; }
}

public class ApiResponse<T> : IApiResponse
{
    public bool Success { get; set; }

    public T? Data { get; set; }

    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O"); // "O" means Isostring 8601-style: 2026-06-02T20:40:51.1234567Z

    public static ApiResponse<T> Ok(T? data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Timestamp = DateTime.UtcNow.ToString("O")
        };
    }
}
