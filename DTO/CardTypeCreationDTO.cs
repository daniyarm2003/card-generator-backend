using CardGeneratorBackend.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CardGeneratorBackend.DTO
{
    public record CardTypeCreationDTO
    {
        [Required, StringLength(8)]
        public string BackgroundColorHexCode1 { get; init; }

        [Required, StringLength(8)]
        public string BackgroundColorHexCode2 { get; init; }

        [Required, StringLength(8)]
        public string TextColor { get; init; }

        [Required]
        public string Name { get; init; }

        [JsonConstructor]
        public CardTypeCreationDTO(string backgroundColorHexCode1, string backgroundColorHexCode2, string textColor, string name)
        {
            BackgroundColorHexCode1 = backgroundColorHexCode1;
            BackgroundColorHexCode2 = backgroundColorHexCode2;
            TextColor = textColor;
            Name = name;
        }
    }

    public static class CardTypeCreationDTOMapper
    {
        public static CardType ToCreationEntity(this CardTypeCreationDTO dto) => new() {
            BackgroundColorHexCode1 = dto.BackgroundColorHexCode1,
            BackgroundColorHexCode2 = dto.BackgroundColorHexCode2,
            Name = dto.Name,
            TextColor = dto.TextColor
        };
    }
}
