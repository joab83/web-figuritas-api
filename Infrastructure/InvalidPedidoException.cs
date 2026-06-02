namespace Figuritas.Api.Infrastructure;

public sealed class InvalidPedidoException(string message) : Exception(message);
