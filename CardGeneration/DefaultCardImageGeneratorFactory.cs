using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Services;

namespace CardGeneratorBackend.CardGeneration
{
    public class DefaultCardImageGeneratorFactory(ITrackedFileService trackedFileService) : ICardImageGeneratorFactory
    {
        private readonly ITrackedFileService mTrackedFileService = trackedFileService;

        public ICardImageGenerator GetCardImageGenerator(CardDTO card)
        {
            return new BaseCardImageGenerator(mTrackedFileService);
        }
    }
}
