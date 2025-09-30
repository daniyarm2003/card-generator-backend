using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Services;

namespace CardGeneratorBackend.CardGeneration
{
    public class DefaultCardImageGeneratorFactory(ITrackedFileService trackedFileService) : ICardImageGeneratorFactory
    {
        private readonly ITrackedFileService mTrackedFileService = trackedFileService;

        public ICardImageGenerator GetCardImageGenerator(CardDTO card)
        {
            if(card.Variant == Enums.CardVariant.NEBULA)
            {
                return new NebulaCardImageGenerator(mTrackedFileService);
            }
            
            return new RegularCardImageGenerator(mTrackedFileService);
        }
    }
}
