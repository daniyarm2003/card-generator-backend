using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Enums;
using System.Text.Json.Serialization;

namespace CardGeneratorBackend.DTO
{
    public record CardDTO(Guid Id, [property: JsonConverter(typeof(JsonStringEnumConverter))] CardVariant Variant, string Name, int Number, int? Level, int? Attack, int? Health, string? Quote, 
        string? Effect, DateTime CreatedAt, CardTypeDTO Type, Guid? DisplayImageId, Guid? CardImageId);

    public static class CardDTOMapper
    {
        public static CardDTO GetDTO(this Card card)
        {
            return new CardDTO(card.Id, card.Variant, card.Name, card.Number, card.Level, card.Attack, card.Health, card.Quote,
                card.Effect, card.CreatedAt, card.Type.GetDTO(), card.DisplayImage?.Id, card.CardImage?.Id);
        }
    }
}
