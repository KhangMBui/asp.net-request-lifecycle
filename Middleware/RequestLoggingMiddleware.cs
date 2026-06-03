/*
 * MIDDLEWARE — RequestLoggingMiddleware
 *
 * What is Middleware?
 *   Middleware is code that runs in the HTTP request pipeline — before and/or after
 *   your controller action. Each piece of middleware receives the request, can do work,
 *   then calls _next(context) to pass it to the next middleware in the chain.
 *   On the way back (after _next returns), it can inspect or modify the response.
 *
 *   Middleware is registered in Program.cs with app.UseMiddleware<T>() and runs
 *   in the exact order it was added. Think of it as a stack of wrappers.
 *
 * Middleware flow:
 *
 *   Request enters
 *     ↓
 *   [before logic — runs on the way IN]
 *     ↓
 *   await _next(context)  ← passes to the next middleware / controller
 *     ↓
 *   [after logic — runs on the way OUT, after the response is ready]
 *     ↓
 *   Response sent to client
 *
 * This middleware specifically:
 *   BEFORE — logs the incoming HTTP method and path
 *   AFTER  — logs the response status code and how long the request took (ms)
 */

using System.Diagnostics;

namespace TaskFlow.Api.Middleware;
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger
    )
    {
        this._next = next;
        this._logger = logger;
    }

    // HttpContext is the object that represents the current HTTP request and response
    // It gives access to:
    //      context.Request.Method
    //      context.Request.Path
    //      context.Request.Headers
    //      context.Request.StatusCode
    //      context.Request.User
    //      context.Request.Items
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var method = context.Request.Method;
        var path = context.Request.Path;

        _logger.LogInformation(
            "Incoming request: {Method} {Path}",
            method,
            path
        );

        await _next(context); // Pass this request to the next middleware (this is called short-circuiting)

        stopwatch.Stop();

        var statusCode = context.Response.StatusCode;

        _logger.LogInformation(
            "Outgoing response: {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
            method,
            path,
            statusCode,
            stopwatch.ElapsedMilliseconds
        );
    }
}