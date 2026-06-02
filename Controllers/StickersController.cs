using Figuritas.Api.Models;
using Figuritas.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Figuritas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StickersController(IStickerRepository stickerRepository) : ControllerBase
{
    [HttpGet("groups")]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetGroups(
        CancellationToken cancellationToken)
    {
        var groups = await stickerRepository.GetGroupsAsync(cancellationToken);

        return Ok(groups);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<Sticker>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Sticker>>> GetByAlbum(
        [FromQuery(Name = "id_album"), BindRequired] int albumId,
        CancellationToken cancellationToken)
    {
        var stickers = await stickerRepository.GetByAlbumAsync(albumId, cancellationToken);

        return Ok(stickers);
    }
}
