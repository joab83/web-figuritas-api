using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Figuritas.Api.Infrastructure;

public sealed class RequestBodyTooLargeExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException
            {
                StatusCode: StatusCodes.Status413PayloadTooLarge
            })
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status413PayloadTooLarge,
                Title = "Payload too large",
                Detail = "The request body is too large."
            },
            cancellationToken);

        return true;
    }
}
