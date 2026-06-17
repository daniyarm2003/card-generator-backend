using System.ComponentModel.DataAnnotations.Schema;

namespace CardGeneratorBackend.Entities
{
    [Table("global_state")]
    public class GlobalState
    {
        public const string GLOBAL_STATE_UUID = "00000000-0000-0000-0000-000000000001";

        [Column("id")]
        public required Guid Id { get; set; }

        [Column("should_update_card_embeddings")]
        public required bool ShouldUpdateCardEmbeddings { get; set; }
    }
}