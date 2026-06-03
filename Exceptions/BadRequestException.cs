/*
 * EXCEPTION — BadRequestException (400)
 *
 * Thrown when input passes model validation ([Required], [MaxLength], etc.) but still
 * violates a business rule — e.g. a duplicate title or an invalid state transition.
 * GlobalExceptionMiddleware catches this and returns a 400 ApiErrorResponse to the client.
 */

namespace TaskFlow.Api.Exceptions;

public class BadRequestException : AppException
{
    public BadRequestException(string message)
        : base(message, StatusCodes.Status400BadRequest)
    {
    }
}