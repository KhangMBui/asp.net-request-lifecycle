/*
 * FILTER — ActionLoggingFilter
 *
 * What is a Filter?
 *   Filters run around controller actions — similar to middleware, but scoped tighter.
 *   While middleware runs for every request (including static files, health checks, etc.),
 *   filters only run for MVC/API controller actions.
 *
 *   The five filter types, in the order they run:
 *     1. Authorization filter  — checks if the user has permission (runs first, before anything else)
 *     2. Resource filter       — runs around model binding; good for caching
 *     3. Action filter         — runs just before and after the action method body
 *     4. Result filter         — runs just before and after the IActionResult is written to the response
 *     5. Exception filter      — catches exceptions thrown inside the action (only the action, not middleware)
 *
 * This is an Action Filter (IAsyncActionFilter):
 *   BEFORE — logs the controller name and action name about to execute
 *   AFTER  — logs how long the action took and what result type it returned
 *
 * Registered globally in Program.cs (options.Filters.Add<ActionLoggingFilter>()),
 * so it applies automatically to every controller action in the app.
 */

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TaskFlow.Api.Filters;

public class ActionLoggingFilter : IAsyncActionFilter // Or use IActionFilter for synchronous
{
    private readonly ILogger<ActionLoggingFilter> _logger;

    public ActionLoggingFilter(ILogger<ActionLoggingFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, ActionExecutionDelegate next
    )
    {
        var stopwatch = Stopwatch.StartNew();

        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

        var controllerName = actionDescriptor?.ControllerName ?? "UnknownController";
        var actionName = actionDescriptor?.ActionName ?? "UnknownAction";

        _logger.LogInformation(
            "Executing action: {ActionName} on {ControllerName}",
            actionName,
            controllerName
        );

        var executedContext = await next();

        stopwatch.Stop();

        _logger.LogInformation(
            "Executed action: {ActionName} on {ControllerName} in {ElapsedMilliseconds}ms with status/result {ResultType}",
            actionName,
            controllerName,
            stopwatch.ElapsedMilliseconds,
            executedContext.Result?.GetType().Name ?? "No Result"
        );
    }
}