using CardGeneratorBackend.Entities;

namespace CardGeneratorBackend.DTO
{
    public record CardTypeDTO(Guid Id, string BackgroundColorHexCode1, string BackgroundColorHexCode2, string TextColor, string Name, Guid? ImageFileId);

    public static class CardTypeDTOMapper
    {
        public static CardTypeDTO GetDTO(this CardType cardType) => new(
            cardType.Id, cardType.BackgroundColorHexCode1, cardType.BackgroundColorHexCode2, cardType.TextColor, cardType.Name, cardType.ImageFile?.Id);
    }
}
