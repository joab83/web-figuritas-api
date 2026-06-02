using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace Figuritas.Api.Infrastructure;

public sealed class DatabaseExceptionHandler(
    ILogger<DatabaseExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not MySqlException)
        {
            return false;
        }

        logger.LogError(exception, "MySQL request failed.");

        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Database unavailable",
                Detail = "The database could not be reached. Try again later."
            },
            cancellationToken);

        return true;
    }
}
