using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Services;

namespace CardGeneratorBackend.DTO.Mappers
{
    public class CardDTOMapper(ITrackedFileService trackedFileService, CardTypeDTOMapper cardTypeDTOMapper)
    {
        private readonly ITrackedFileService mTrackedFileService = trackedFileService;
        private readonly CardTypeDTOMapper mCardTypeDTOMapper = cardTypeDTOMapper;

        public CardDTO ToDTO(Card card)
        {
            return new CardDTO(card.Id, card.Variant, card.Name, card.Number, card.Level, card.Attack, card.Health, card.Quote,
                card.Effect, card.CreatedAt, mCardTypeDTOMapper.ToDTO(card.Type), card.DisplayImage?.Id, card.CardImage?.Id);
        }
    }
}