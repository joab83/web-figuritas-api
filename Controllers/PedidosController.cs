using Figuritas.Api.Models;
using Figuritas.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Figuritas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController(IPedidoRepository pedidoRepository) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("CreatePedido")]
    [RequestSizeLimit(16 * 1024)]
    [ProducesResponseType<CreatePedidoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CreatePedidoResponse>> Create(
        CreatePedidoRequest request,
        CancellationToken cancellationToken)
    {
        var pedidoId = await pedidoRepository.CreateAsync(request, cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            new CreatePedidoResponse(pedidoId));
    }
}
