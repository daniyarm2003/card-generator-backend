using CardGeneratorBackend.DTO;
using Pgvector;

namespace CardGeneratorBackend.Services
{
    public interface ICardEmbeddingService
    {
        public string GetCardEmbedText(CardDTO card);

        public Task<Vector> GetCardSearchEmbedding(string searchQuery);

        public Task<Vector> GetCardEmbedding(CardDTO card);

        public Task UpdateAllCardEmbeddings(bool skipExisting);
    }
}