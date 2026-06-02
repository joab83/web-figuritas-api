using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Figuritas.Api.Models;

public sealed class CreatePedidoRequest : IValidatableObject
{
    [JsonPropertyName("nombre")]
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string? Nombre { get; init; }

    [JsonPropertyName("numero_telefono")]
    [Required]
    [StringLength(30, MinimumLength = 6)]
    [RegularExpression(@"^\+?[0-9 ()-]+$")]
    public string? NumeroTelefono { get; init; }

    [JsonPropertyName("comentario")]
    [StringLength(200)]
    public string? Comentario { get; init; }

    [JsonPropertyName("stickers")]
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public required IReadOnlyList<CreatePedidoStickerRequest> Stickers { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ContainsOnlyWhitespace(Nombre))
        {
            yield return new ValidationResult(
                "The nombre field cannot contain only whitespace.",
                [nameof(Nombre)]);
        }

        if (ContainsOnlyWhitespace(NumeroTelefono))
        {
            yield return new ValidationResult(
                "The numero_telefono field cannot contain only whitespace.",
                [nameof(NumeroTelefono)]);
        }

        if (ContainsControlCharacters(Nombre)
            || ContainsControlCharacters(NumeroTelefono)
            || ContainsControlCharacters(Comentario))
        {
            yield return new ValidationResult(
                "Text fields cannot contain control characters.");
        }
    }

    private static bool ContainsOnlyWhitespace(string? value)
    {
        return value is not null && string.IsNullOrWhiteSpace(value);
    }

    private static bool ContainsControlCharacters(string? value)
    {
        return value?.Any(char.IsControl) is true;
    }
}

public sealed class CreatePedidoStickerRequest : IValidatableObject
{
    [JsonPropertyName("id_album")]
    [Range(1, int.MaxValue)]
    public int AlbumId { get; init; }

    [JsonPropertyName("sku")]
    [Required]
    [StringLength(50)]
    public required string Sku { get; init; }

    [JsonPropertyName("cantidad")]
    [Range(1, 1000)]
    public int Cantidad { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Sku))
        {
            yield return new ValidationResult(
                "The sku field cannot contain only whitespace.",
                [nameof(Sku)]);
        }
        else if (Sku.Any(char.IsControl))
        {
            yield return new ValidationResult(
                "The sku field cannot contain control characters.",
                [nameof(Sku)]);
        }
    }
}
