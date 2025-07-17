using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CardGeneratorBackend.DTO
{
    public record CardUpdateDTO
    {
        [StringLength(64)]
        public string? Name { get; init; }

        [Range(1, int.MaxValue)]
        public int? Number { get; init; }

        [Range(1, 32)]
        public int? Level { get; init; }

        [Range(0, int.MaxValue)]
        public int? Attack { get; init; }

        [Range(0, int.MaxValue)]
        public int? Health { get; init; }

        public string? Quote { get; init; }

        public string? Effect { get; init; }

        public Guid? TypeId { get; init; }

        [JsonConstructor]
        public CardUpdateDTO(string? name, int? number, int? level, int? attack, int? health, string? quote, string? effect, Guid? typeId)
        {
            Name = name;
            Number = number;
            Level = level;
            Attack = attack;
            Health = health;
            Quote = quote;
            Effect = effect;
            TypeId = typeId;
        }
    }
}
