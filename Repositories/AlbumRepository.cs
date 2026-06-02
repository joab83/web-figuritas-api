using System.Globalization;
using Figuritas.Api.Models;
using MySqlConnector;

namespace Figuritas.Api.Repositories;

public sealed class AlbumRepository(IConfiguration configuration) : IAlbumRepository
{
    private const string GetByIdQuery = """
        SELECT `id_album`,
               `descripcion`,
               `habilitado`
        FROM `albums`
        WHERE `id_album` = @IdAlbum
          AND `habilitado` = 1;
        """;

    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Connection string 'DefaultConnection' was not configured.");

    public async Task<Album?> GetByIdAsync(
        int albumId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new MySqlCommand(GetByIdQuery, connection);
        command.Parameters.Add("@IdAlbum", MySqlDbType.Int32).Value = albumId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Album(
            AlbumId: Convert.ToInt32(reader["id_album"], CultureInfo.InvariantCulture),
            Descripcion: Convert.ToString(reader["descripcion"], CultureInfo.InvariantCulture)
                ?? string.Empty,
            Habilitado: Convert.ToBoolean(reader["habilitado"], CultureInfo.InvariantCulture));
    }
}
