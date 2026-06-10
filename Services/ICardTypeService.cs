using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Entities;

namespace CardGeneratorBackend.Services
{
    public interface ICardTypeService
    {
        public Task<CardType> GetCardTypeById(Guid id);

        public Task<IEnumerable<CardType>> GetAllCardTypes();

        public Task<CardType> CreateCardType(CardType creationData);

        public Task<UploadURLResponseDTO> CreateCardTypeImageUploadURL(Guid typeId, string fileName);

        public Task<CardType> UpdateCardTypeWithId(Guid typeId, CardTypeUpdateDTO updateDTO);
    }
}
