using Figuritas.Api.Models;

namespace Figuritas.Api.Repositories;

public interface IAlbumRepository
{
    Task<Album?> GetByIdAsync(
        int albumId,
        CancellationToken cancellationToken = default);
}
