using System.Globalization;
using System.Threading.RateLimiting;
using Figuritas.Api.Infrastructure;
using Figuritas.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole();

var createPedidoPermitLimit = builder.Configuration.GetValue(
    "RateLimiting:CreatePedido:PermitLimit",
    5);
var createPedidoWindowSeconds = builder.Configuration.GetValue(
    "RateLimiting:CreatePedido:WindowSeconds",
    60);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<RequestBodyTooLargeExceptionHandler>();
builder.Services.AddExceptionHandler<InvalidPedidoExceptionHandler>();
builder.Services.AddExceptionHandler<DatabaseExceptionHandler>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        var response = context.HttpContext.Response;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds)
                .ToString(CultureInfo.InvariantCulture);
        }

        await response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too many requests",
                Detail = "Too many orders were submitted. Try again later."
            },
            cancellationToken);
    };

    options.AddPolicy("CreatePedido", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = createPedidoPermitLimit,
                Window = TimeSpan.FromSeconds(createPedidoWindowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});
builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IStickerRepository, StickerRepository>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseRateLimiter();
app.MapControllers();

app.Run();

public partial class Program;
