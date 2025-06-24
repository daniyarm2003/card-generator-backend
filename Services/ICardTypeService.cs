using CardGeneratorBackend.Entities;

namespace CardGeneratorBackend.Services
{
    public interface ICardTypeService
    {
        public Task<IEnumerable<CardType>> GetAllCardTypes();

        public Task<CardType> CreateCardType(CardType creationData);

        public Task<CardType> UpdateCardTypeImage(Guid typeId, string fileName, byte[] data);
    }
}
