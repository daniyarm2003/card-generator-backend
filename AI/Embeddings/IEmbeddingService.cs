using Pgvector;

namespace CardGeneratorBackend.AI.Embeddings
{
    public interface IEmbeddingService
    {
        public Task<Vector> GetEmbedding(string text, IEmbeddingCachingStrategy? cachingStrategy);

        public Task<IList<Vector>> GetAllEmbeddings(IList<string> items);
    }
}