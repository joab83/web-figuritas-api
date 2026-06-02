using Figuritas.Api.Models;

namespace Figuritas.Api.Repositories;

public interface IStickerRepository
{
    Task<IReadOnlyList<string>> GetGroupsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Sticker>> GetByAlbumAsync(
        int albumId,
        CancellationToken cancellationToken = default);
}
