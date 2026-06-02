using System.Text.Json.Serialization;

namespace Figuritas.Api.Models;

public sealed record Album(
    [property: JsonPropertyName("id_album")] int AlbumId,
    [property: JsonPropertyName("descripcion")] string Descripcion,
    [property: JsonPropertyName("habilitado")] bool Habilitado);
