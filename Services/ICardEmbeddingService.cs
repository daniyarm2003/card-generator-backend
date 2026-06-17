using CardGeneratorBackend.DTO;

namespace CardGeneratorBackend.Services
{
    public interface ICardEmbeddingService
    {
        public string GetCardEmbedText(CardDTO card);

        public Task UpdateAllCardEmbeddings(bool skipExisting);
    }
}