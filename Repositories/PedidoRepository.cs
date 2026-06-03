using Figuritas.Api.Infrastructure;
using Figuritas.Api.Models;
using MySqlConnector;

namespace Figuritas.Api.Repositories;

public sealed class PedidoRepository(IConfiguration configuration) : IPedidoRepository
{
    private const string GetStickerStockQuery = """
        SELECT `disponible`,
               `habilitado`
        FROM `stickers`
        WHERE `id_album` = @IdAlbum
          AND `sku` = @Sku
        FOR UPDATE;
        """;

    private const string InsertPedidoQuery = """
        INSERT INTO `pedidos`
               (`nombre`, `numero_telefono`, `comentario`, `estado`)
        VALUES (@Nombre, @NumeroTelefono, @Comentario, @Estado);
        """;

    private const string DecreaseStickerStockQuery = """
        UPDATE `stickers`
        SET `disponible` = `disponible` - @Cantidad
        WHERE `id_album` = @IdAlbum
          AND `sku` = @Sku
          AND `habilitado` = 1
          AND `disponible` >= @Cantidad;
        """;

    private const string InsertPedidoStickerQuery = """
        INSERT INTO `rel_pedido_stickers`
               (`id_pedido`, `id_album`, `sku`, `cantidad`)
        VALUES (@IdPedido, @IdAlbum, @Sku, @Cantidad);
        """;

    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Connection string 'DefaultConnection' was not configured.");

    public async Task<int> CreateAsync(
        CreatePedidoRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateDistinctStickers(request.Stickers);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var sticker in request.Stickers)
            {
                await ValidateStickerStockAsync(
                    connection,
                    transaction,
                    sticker,
                    cancellationToken);
            }

            var pedidoId = await InsertPedidoAsync(
                connection,
                transaction,
                request,
                cancellationToken);

            foreach (var sticker in request.Stickers)
            {
                await DecreaseStickerStockAsync(
                    connection,
                    transaction,
                    sticker,
                    cancellationToken);
                await InsertPedidoStickerAsync(
                    connection,
                    transaction,
                    pedidoId,
                    sticker,
                    cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            return pedidoId;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private static void ValidateDistinctStickers(IReadOnlyList<CreatePedidoStickerRequest> stickers)
    {
        var duplicatedSticker = stickers
            .GroupBy(
                sticker => (sticker.AlbumId, Sku: sticker.Sku.Trim()),
                StringTupleComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicatedSticker is not null)
        {
            throw new InvalidPedidoException(
                $"El sticker '{duplicatedSticker.Key.Sku}' del album " +
                $"'{duplicatedSticker.Key.AlbumId}' esta repetido en el pedido.");
        }
    }

    private static async Task ValidateStickerStockAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        CreatePedidoStickerRequest sticker,
        CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand(GetStickerStockQuery, connection, transaction);
        command.Parameters.Add("@IdAlbum", MySqlDbType.Int32).Value = sticker.AlbumId;
        command.Parameters.Add("@Sku", MySqlDbType.VarChar, 50).Value = sticker.Sku.Trim();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidPedidoException(
                $"El sticker '{sticker.Sku}' del album '{sticker.AlbumId}' no existe.");
        }

        var habilitado = Convert.ToBoolean(reader["habilitado"]);

        if (!habilitado)
        {
            throw new InvalidPedidoException(
                $"El sticker '{sticker.Sku}' del album '{sticker.AlbumId}' no esta habilitado.");
        }

        var disponible = Convert.ToInt32(reader["disponible"]);

        if (disponible < sticker.Cantidad)
        {
            throw new InvalidPedidoException(
                $"El sticker '{sticker.Sku}' del album '{sticker.AlbumId}' no tiene stock suficiente. " +
                $"Disponible: {disponible}. Solicitado: {sticker.Cantidad}.");
        }
    }

    private static async Task<int> InsertPedidoAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        CreatePedidoRequest request,
        CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand(InsertPedidoQuery, connection, transaction);
        command.Parameters.Add("@Nombre", MySqlDbType.VarChar, 200).Value =
            (object?)request.Nombre?.Trim() ?? DBNull.Value;
        command.Parameters.Add("@NumeroTelefono", MySqlDbType.VarChar, 200).Value =
            (object?)request.NumeroTelefono?.Trim() ?? DBNull.Value;
        command.Parameters.Add("@Comentario", MySqlDbType.VarChar, 200).Value =
            (object?)request.Comentario?.Trim() ?? DBNull.Value;
        command.Parameters.Add("@Estado", MySqlDbType.VarChar, 50).Value = "Pendiente";

        await command.ExecuteNonQueryAsync(cancellationToken);

        return checked((int)command.LastInsertedId);
    }

    private static async Task DecreaseStickerStockAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        CreatePedidoStickerRequest sticker,
        CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand(DecreaseStickerStockQuery, connection, transaction);
        command.Parameters.Add("@IdAlbum", MySqlDbType.Int32).Value = sticker.AlbumId;
        command.Parameters.Add("@Sku", MySqlDbType.VarChar, 50).Value = sticker.Sku.Trim();
        command.Parameters.Add("@Cantidad", MySqlDbType.Int32).Value = sticker.Cantidad;

        var updatedRows = await command.ExecuteNonQueryAsync(cancellationToken);

        if (updatedRows == 0)
        {
            throw new InvalidPedidoException(
                $"No se pudo actualizar el stock del sticker '{sticker.Sku}' " +
                $"del album '{sticker.AlbumId}'.");
        }
    }

    private static async Task InsertPedidoStickerAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        int pedidoId,
        CreatePedidoStickerRequest sticker,
        CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand(InsertPedidoStickerQuery, connection, transaction);
        command.Parameters.Add("@IdPedido", MySqlDbType.Int32).Value = pedidoId;
        command.Parameters.Add("@IdAlbum", MySqlDbType.Int32).Value = sticker.AlbumId;
        command.Parameters.Add("@Sku", MySqlDbType.VarChar, 50).Value = sticker.Sku.Trim();
        command.Parameters.Add("@Cantidad", MySqlDbType.Int32).Value = sticker.Cantidad;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private sealed class StringTupleComparer : IEqualityComparer<(int AlbumId, string Sku)>
    {
        public static readonly StringTupleComparer OrdinalIgnoreCase = new();

        public bool Equals((int AlbumId, string Sku) x, (int AlbumId, string Sku) y)
        {
            return x.AlbumId == y.AlbumId
                && string.Equals(x.Sku, y.Sku, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode((int AlbumId, string Sku) obj)
        {
            return HashCode.Combine(
                obj.AlbumId,
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Sku));
        }
    }
}
