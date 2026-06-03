/*
 * EXCEPTION — NotFoundException (404)
 *
 * Thrown when a requested resource does not exist — e.g. looking up a task ID that isn't in the DB.
 * GlobalExceptionMiddleware catches this and returns a 404 ApiErrorResponse to the client.
 */

namespace TaskFlow.Api.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base(message, StatusCodes.Status404NotFound)
    {
    }
}