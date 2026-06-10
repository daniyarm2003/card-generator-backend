using CardGeneratorBackend.Enums;
using System.Text.Json.Serialization;

namespace CardGeneratorBackend.DTO
{
    public record CardDTO(Guid Id, [property: JsonConverter(typeof(JsonStringEnumConverter))] CardVariant Variant, string Name, int Number, int? Level, int? Attack, int? Health, string? Quote, 
        string? Effect, DateTime CreatedAt, CardTypeDTO Type, Guid? DisplayImageId, string? DisplayImageURL, string? CardImageURL);
}
