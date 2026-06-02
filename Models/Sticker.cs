using System.Text.Json.Serialization;

namespace Figuritas.Api.Models;

public sealed record Sticker(
    [property: JsonPropertyName("id_album")] int AlbumId,
    [property: JsonPropertyName("sku")] string Sku,
    [property: JsonPropertyName("nombre")] string Nombre,
    [property: JsonPropertyName("precio")] decimal Precio,
    [property: JsonPropertyName("disponible")] int Disponible,
    [property: JsonPropertyName("grupo")] string Grupo,
    [property: JsonPropertyName("destacada")] bool Destacada,
    [property: JsonPropertyName("habilitado")] bool Habilitado);
