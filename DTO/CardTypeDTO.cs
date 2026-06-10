using CardGeneratorBackend.Entities;

namespace CardGeneratorBackend.DTO
{
    public record CardTypeDTO(Guid Id, string BackgroundColorHexCode1, string BackgroundColorHexCode2, string TextColor, string Name, Guid? ImageFileId, string? ImageFileReadURL);
}
