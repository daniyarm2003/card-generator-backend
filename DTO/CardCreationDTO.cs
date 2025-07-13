using CardGeneratorBackend.DTO.Validation;
using CardGeneratorBackend.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CardGeneratorBackend.DTO
{
    public record CardCreationDTO
    {
        [Required, StringLength(64)]
        public string Name { get; init; }

        [Required]
        public CardVariant Variant { get; init; }

        [RequiredWithCardVariant(CardVariant.REGULAR), Range(1, 32)]
        public int? Level { get; init; }

        [RequiredWithCardVariant(CardVariant.REGULAR), Range(0, int.MaxValue)]
        public int? Attack { get; init; }

        [RequiredWithCardVariant(CardVariant.REGULAR), Range(0, int.MaxValue)]
        public int? Health { get; init; }

        [RequiredWithCardVariant(CardVariant.REGULAR)]
        public string? Quote { get; init; }

        public string? Effect { get; init; }

        [Required]
        public Guid TypeId { get; init; }

        [JsonConstructor]
        public CardCreationDTO(string name, CardVariant variant, int? level, int? attack, int? health, string? quote, string? effect, Guid typeId)
        {
            Name = name;
            Variant = variant;
            Level = level;
            Attack = attack;
            Health = health;
            Quote = quote;
            Effect = effect;
            TypeId = typeId;
        }
    }
}
