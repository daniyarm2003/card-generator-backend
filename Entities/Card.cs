using CardGeneratorBackend.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CardGeneratorBackend.Entities
{
    [Table("cards")]
    public class Card
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("name")]
        public required string Name { get; set; }

        [Column("number")]
        public required int Number { get; set; }

        [Column("variant")]
        public required CardVariant Variant { get; set; }

        [Column("level")]
        public int? Level { get; set; }

        [Column("attack")]
        public int? Attack { get; set; }

        [Column("health")]
        public int? Health { get; set; }

        [Column("quote")]
        public string? Quote { get; set; }

        [Column("effect")]
        public string? Effect { get; set; }

        [Column("type_id")]
        public Guid TypeId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("TypeId")]
        public required CardType Type { get; set; }

        [Column("display_image_id")]
        public Guid? DisplayImageId { get; set; }

        [ForeignKey("DisplayImageId")]
        public TrackedFile? DisplayImage { get; set; }

        [Column("card_image_id")]
        public Guid? CardImageId { get; set; }

        [ForeignKey("CardImageId")]
        public TrackedFile? CardImage { get; set; }
    }
}
