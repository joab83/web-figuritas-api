using System.Text.Json.Serialization;

namespace Figuritas.Api.Models;

public sealed record CreatePedidoResponse(
    [property: JsonPropertyName("id_pedido")] int PedidoId);
