using System.ComponentModel.DataAnnotations;
using CardGeneratorBackend.Enums;

namespace CardGeneratorBackend.DTO
{
    public record CardRetrievalQueryDTO
    {
        [Range(1, 100)]
        public int? PageSize { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNum { get; set; } = 1;

        [StringLength(64)]
        public string? SearchQuery { get; set; }

        public CardVariant? Variant { get; set; }

        [Range(1, 32)]
        public int? Level { get; set; }
    }
}