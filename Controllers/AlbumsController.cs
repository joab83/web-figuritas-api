using Figuritas.Api.Models;
using Figuritas.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Figuritas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlbumsController(IAlbumRepository albumRepository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<Album>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Album>> GetById(
        [FromQuery(Name = "id_album"), BindRequired] int albumId,
        CancellationToken cancellationToken)
    {
        var album = await albumRepository.GetByIdAsync(albumId, cancellationToken);

        return album is null ? NotFound() : Ok(album);
    }
}
