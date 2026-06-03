/*
 * EXCEPTION — ConflictException (409)
 *
 * Thrown when a request conflicts with existing state — e.g. creating a resource that already
 * exists, or two concurrent requests trying to modify the same record.
 * GlobalExceptionMiddleware catches this and returns a 409 ApiErrorResponse to the client.
 */

namespace TaskFlow.Api.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message)
        : base(message, StatusCodes.Status409Conflict)
    {
    }
}