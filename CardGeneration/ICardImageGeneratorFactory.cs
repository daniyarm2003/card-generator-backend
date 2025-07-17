using CardGeneratorBackend.DTO;

namespace CardGeneratorBackend.CardGeneration
{
    public interface ICardImageGeneratorFactory
    {
        public ICardImageGenerator GetCardImageGenerator(CardDTO card);
    }
}
