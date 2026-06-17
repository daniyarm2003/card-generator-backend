using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Entities;
using System.Drawing;

namespace CardGeneratorBackend.Services
{
    public interface ICardService
    {
        public Task<PaginationDTO<CardDTO>> GetCards(CardRetrievalQueryDTO queryDTO);

        public Task<CardDTO> GetCardById(Guid id);

        public Task<Card> CreateCard(CardCreationDTO dto);

        public Task<Card> UpdateCardWithId(Guid id, CardUpdateDTO dto);

        public Task<UploadURLResponseDTO> CreateCardDisplayImageUploadURL(Guid id, string filename);

        public Task<Card> GenerateAndUpdateCardImage(Guid id);

        public Task DeleteCard(Card card);

        public Task DeleteCardById(Guid id);
    }
}
