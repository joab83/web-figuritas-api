using Figuritas.Api.Models;

namespace Figuritas.Api.Repositories;

public interface IPedidoRepository
{
    Task<int> CreateAsync(
        CreatePedidoRequest request,
        CancellationToken cancellationToken = default);
}
