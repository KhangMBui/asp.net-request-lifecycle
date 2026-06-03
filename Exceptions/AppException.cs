/*
 * EXCEPTION — AppException (Abstract Base)
 *
 * What is a Custom Exception Hierarchy?
 *   Instead of throwing a generic Exception or manually returning status codes from services,
 *   you define domain-specific exception types. This lets you express intent clearly:
 *
 *     throw new NotFoundException($"Task {id} not found.")
 *
 *   ...is far more readable than passing status codes around as integers.
 *
 * AppException is the abstract base class. It carries a StatusCode so that
 * GlobalExceptionMiddleware knows which HTTP status to use in the response —
 * without any other layer needing to know HTTP exists.
 *
 * Concrete subclasses:
 *   NotFoundException    → 404 Not Found
 *   BadRequestException  → 400 Bad Request
 *   ConflictException    → 409 Conflict
 *
 * Any layer (service, repository) can throw these, and GlobalExceptionMiddleware
 * will catch and handle them automatically.
 */

namespace TaskFlow.Api.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }

    protected AppException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}