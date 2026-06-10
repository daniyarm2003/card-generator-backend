using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Services;

namespace CardGeneratorBackend.DTO.Mappers
{
    public class CardTypeDTOMapper(ITrackedFileService trackedFileService)
    {
        private readonly ITrackedFileService mTrackedFileService = trackedFileService;

        public CardTypeDTO ToDTO(CardType cardType)
        {
            return new(cardType.Id, cardType.BackgroundColorHexCode1, cardType.BackgroundColorHexCode2, cardType.TextColor, cardType.Name, cardType.ImageFile?.Id, cardType.ImageFile != null ? mTrackedFileService.GetFileReadURL(cardType.ImageFile) : null);
        }

        public CardType ToCreationEntity(CardTypeCreationDTO creationDTO)
        {
            return new CardType
            {
                BackgroundColorHexCode1 = creationDTO.BackgroundColorHexCode1,
                BackgroundColorHexCode2 = creationDTO.BackgroundColorHexCode2,
                Name = creationDTO.Name,
                TextColor = creationDTO.TextColor
            };
        }
    }
}