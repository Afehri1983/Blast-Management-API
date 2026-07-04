using BlastManagement.Application.Exceptions;
using BlastManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BlastManagement.Api.ExceptionHandling;

internal sealed class ApplicationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Domain rule violation"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency conflict"),
            _ => (0, string.Empty)
        };

        if (statusCode == 0)
        {
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
