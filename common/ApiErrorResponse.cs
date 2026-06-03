/*
 * COMMON — ApiErrorResponse (Error Envelope)
 *
 * The error counterpart to ApiResponse<T>.
 *
 * Used by GlobalExceptionMiddleware to return a structured JSON error body instead
 * of a raw exception stack trace or ASP.NET's default problem details format.
 *
 * Example output:
 *   {
 *     "success": false,
 *     "message": "Task with id 5 was not found.",
 *     "statusCode": 404,
 *     "timestamp": "2026-06-02T...",
 *     "traceId": "0HN7K2..."
 *   }
 *
 * TraceId is the request's unique identifier — developers can cross-reference it
 * with server logs to find exactly which log lines belong to a failing request.
 */

namespace TaskFlow.Api.Common;

public class ApiErrorResponse : IApiResponse
{
    public bool Success { get; set; } = false;

    public string Message { get; set; } = string.Empty;

    public int StatusCode { get; set; }

    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O");

    public string? TraceId { get; set; }

    public static ApiErrorResponse Create(
        string message, int statusCode, string? traceId = null
    )
    {
        return new ApiErrorResponse
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow.ToString("O"),
            TraceId = traceId
        };
    }
}