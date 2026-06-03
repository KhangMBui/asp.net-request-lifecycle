/*
 * MIDDLEWARE — GlobalExceptionMiddleware
 *
 * What is Global Exception Handling?
 *   Without this, an unhandled exception bubbles up and either crashes the request
 *   with a generic 500 page or leaks a stack trace to the client — both are bad.
 *   This middleware wraps the entire pipeline in a try/catch so any exception thrown
 *   from any layer (controller, service, repository) is caught and handled here.
 *
 * How it works:
 *   - Passes the request down the pipeline normally (await _next(context)).
 *   - If an exception is thrown anywhere below, the catch block intercepts it.
 *   - AppException subclasses (NotFoundException, BadRequestException, ConflictException)
 *     are "expected" domain errors — logged as warnings, returned with their own status code.
 *   - All other exceptions are unexpected bugs — logged as errors, returned as 500.
 *   - In both cases the client gets a clean ApiErrorResponse JSON, not a stack trace.
 *
 * This makes error handling centralized: services just throw NotFoundException("...") and
 * this middleware takes care of the rest — no try/catch needed in controllers or services.
 */

using System.Text.Json;
using TaskFlow.Api.Common;
using TaskFlow.Api.Exceptions;

namespace TaskFlow.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger
    )
    {
        this._next = next;
        this._logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Let the request continues. 
        // If no exception happens, the middleware does nothing special
        try
        {
            await _next(context);
        }
        // But if something throws Exception, catch it...
        catch (Exception exception)
        {
            // ...and handle it gracefully.
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var isAppException = exception is AppException;

        if (isAppException)
        {
            _logger.LogWarning(
                exception,
                "Handled application exception while processing request {Method} {Path}",
                context.Request.Method,
                context.Request.Path
            );
        }
        else
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred while processing request {Method} {Path}",
                context.Request.Method,
                context.Request.Path
            );
        }

        var statusCode = exception is AppException appException
            ? appException.StatusCode
            : StatusCodes.Status500InternalServerError;

        var message = exception is AppException
            ? exception.Message
                : "An unexpected error occurred.";

        var response = ApiErrorResponse.Create(
            message: message,
            statusCode: statusCode,
            traceId: context.TraceIdentifier
        );

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
