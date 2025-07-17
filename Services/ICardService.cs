using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Entities;
using System.Drawing;

namespace CardGeneratorBackend.Services
{
    public interface ICardService
    {
        public Task<IEnumerable<Card>> GetAllCards();

        public Task<PaginationDTO<Card>> GetCardsPaginated(int pageNum, int pageSize);

        public Task<Card> CreateCard(CardCreationDTO dto);

        public Task<Card> UpdateCardWithId(Guid id, CardUpdateDTO dto);

        public Task<Card> UpdateCardDisplayImage(Guid id, string filename, byte[] data);

        public Task<Card> GenerateAndUpdateCardImage(Guid id, string filename);
    }
}
