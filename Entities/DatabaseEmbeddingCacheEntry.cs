using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace CardGeneratorBackend.Entities
{
    [Table("embedding_cache_entry")]
    public class DatabaseEmbeddingCacheEntry
    {
        [Column("embedded_text")]
        public required string Text { get; set; }

        [Column("embedding", TypeName = "vector(768)")]
        public required Vector Embedding { get; set; }
    }
}