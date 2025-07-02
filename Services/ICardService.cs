using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Entities;

namespace CardGeneratorBackend.Services
{
    public interface ICardService
    {
        public Task<IEnumerable<Card>> GetAllCards();

        public Task<PaginationDTO<Card>> GetCardsPaginated(int pageNum, int pageSize);

        public Task<Card> CreateCard(CardCreationDTO dto);
    }
}
