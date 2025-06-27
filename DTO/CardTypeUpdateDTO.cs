using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CardGeneratorBackend.DTO
{
    public record CardTypeUpdateDTO
    {
        [StringLength(8)]
        public string? BackgroundColorHexCode1 { get; init; }

        [StringLength(8)]
        public string? BackgroundColorHexCode2 { get; init; }

        [StringLength(8)]
        public string? TextColor { get; init; }

        public string? Name { get; init; }

        [JsonConstructor]
        public CardTypeUpdateDTO(string? backgroundColorHexCode1, string? backgroundColorHexCode2, string? textColor, string? name)
        {
            BackgroundColorHexCode1 = backgroundColorHexCode1;
            BackgroundColorHexCode2 = backgroundColorHexCode2;
            TextColor = textColor;
            Name = name;
        }
    }
}
