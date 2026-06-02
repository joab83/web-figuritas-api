using System.Globalization;
using Figuritas.Api.Models;
using MySqlConnector;

namespace Figuritas.Api.Repositories;

public sealed class StickerRepository(IConfiguration configuration) : IStickerRepository
{
    private const string GetGroupsQuery = """
        SELECT DISTINCT `grupo`
        FROM `stickers`
        ORDER BY `grupo` ASC;
        """;

    private const string GetByAlbumQuery = """
        SELECT `id_album`,
               `sku`,
               `nombre`,
               `precio`,
               `disponible`,
               `grupo`,
               `destacada`,
               `habilitado`
        FROM `stickers`
        WHERE `id_album` = @IdAlbum
          AND `habilitado` = 1;
        """;

    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Connection string 'DefaultConnection' was not configured.");

    public async Task<IReadOnlyList<string>> GetGroupsAsync(
        CancellationToken cancellationToken = default)
    {
        var groups = new List<string>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new MySqlCommand(GetGroupsQuery, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            groups.Add(Convert.ToString(reader["grupo"], CultureInfo.InvariantCulture)
                ?? string.Empty);
        }

        return groups;
    }

    public async Task<IReadOnlyList<Sticker>> GetByAlbumAsync(
        int albumId,
        CancellationToken cancellationToken = default)
    {
        var stickers = new List<Sticker>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new MySqlCommand(GetByAlbumQuery, connection);
        command.Parameters.Add("@IdAlbum", MySqlDbType.Int32).Value = albumId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            stickers.Add(new Sticker(
                AlbumId: Convert.ToInt32(reader["id_album"], CultureInfo.InvariantCulture),
                Sku: Convert.ToString(reader["sku"], CultureInfo.InvariantCulture) ?? string.Empty,
                Nombre: Convert.ToString(reader["nombre"], CultureInfo.InvariantCulture) ?? string.Empty,
                Precio: Convert.ToDecimal(reader["precio"], CultureInfo.InvariantCulture),
                Disponible: Convert.ToInt32(reader["disponible"], CultureInfo.InvariantCulture),
                Grupo: Convert.ToString(reader["grupo"], CultureInfo.InvariantCulture) ?? string.Empty,
                Destacada: Convert.ToBoolean(reader["destacada"], CultureInfo.InvariantCulture),
                Habilitado: Convert.ToBoolean(reader["habilitado"], CultureInfo.InvariantCulture)));
        }

        return stickers;
    }
}
