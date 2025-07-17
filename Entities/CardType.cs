using System.ComponentModel.DataAnnotations.Schema;

namespace CardGeneratorBackend.Entities
{
    [Table("card_types")]
    public class CardType
    {
        public const string NONE_TYPE_UUID = "00000000-0000-0000-0000-000000000001";

        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("bg_color_hex_code_1")]
        public required string BackgroundColorHexCode1 { get; set; }

        [Column("bg_color_hex_code_2")]
        public required string BackgroundColorHexCode2 { get; set; }

        [Column("text_color")]
        public required string TextColor { get; set; }

        [Column("name")]
        public required string Name { get; set; }

        public Guid? ImageFileId { get; set; }

        [ForeignKey("ImageFileId")]
        public TrackedFile? ImageFile { get; set; }
    }
}
