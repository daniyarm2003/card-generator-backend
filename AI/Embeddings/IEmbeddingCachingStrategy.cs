using Pgvector;

namespace CardGeneratorBackend.AI.Embeddings
{
    public interface IEmbeddingCachingStrategy
    {
        public Task<bool> IsCached(string text);

        public Task<Vector?> GetCachedEmbedding(string text);

        public Task CacheEmbedding(string text, Vector embedding);
    }
}