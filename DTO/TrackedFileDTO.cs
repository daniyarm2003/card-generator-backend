using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Enums;

namespace CardGeneratorBackend.DTO
{
    public record TrackedFileDTO(Guid Id, string Path, FileStorageLocation StorageLocation);

    public static class TrackedFileDTOMapper
    {
        public static TrackedFileDTO GetDTO(this TrackedFile file) => new(
            file.Id, file.Path, file.StorageLocation);
    }
}
