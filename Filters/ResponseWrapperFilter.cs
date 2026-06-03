/*
 * FILTER — ResponseWrapperFilter
 *
 * What is a Result Filter?
 *   A result filter (IResultFilter) runs just before and after the action's IActionResult
 *   is written to the HTTP response. This is the last chance to modify the response body
 *   before bytes are sent to the client.
 *
 * This filter enforces a consistent response envelope for all successful responses.
 * Instead of the client receiving a raw TaskItem, it receives:
 *   {
 *     "success": true,
 *     "data": { ...TaskItem... },
 *     "timestamp": "2026-06-02T..."
 *   }
 *
 * Error responses (4xx, 5xx) are intentionally skipped — those are already handled by
 * GlobalExceptionMiddleware, which returns an ApiErrorResponse with its own shape.
 *
 * Registered globally in Program.cs (options.Filters.Add<ResponseWrapperFilter>()).
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaskFlow.Api.Common;

namespace TaskFlow.Api.Filters;

public class ResponseWrapperFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        // catches response like 
        // return Ok(tasks);
        // return CreatedAtAction(...);
        // return NotFound(new { message = "..." });
        if (context.Result is ObjectResult objectResult)
        {
            var statusCode = objectResult.StatusCode
                ?? context.HttpContext.Response.StatusCode;

            // Only wrap 200 OK and 201 Created
            // Do not wrap 400, 404, 500
            // Because error responses are handled by exception handling and custom error format
            var isSuccessfulStatusCode = statusCode >= 200 && statusCode < 300;

            if (!isSuccessfulStatusCode)
            {
                return;
            }

            objectResult.Value = ApiResponse<object>.Ok(objectResult.Value);
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // Nothing needed after the result executes for now.
    }
}