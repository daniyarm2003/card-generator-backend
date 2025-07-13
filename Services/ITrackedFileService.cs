using CardGeneratorBackend.Entities;

namespace CardGeneratorBackend.Services
{
    public record FileDownloadInfo(string Name, byte[] Contents);

    public interface ITrackedFileService
    {
        public Task<TrackedFile> CreateAndWriteFile(TrackedFile file, byte[] data);

        public Task<byte[]> ReadFile(TrackedFile file);

        public Task DeleteFile(TrackedFile file);

        public string GetFileDownloadName(TrackedFile file);

        public Task<FileDownloadInfo> ReadFileWithId(Guid id);

        public Task<TrackedFile> WriteOrReplaceFileContents(Guid? fileId, TrackedFile? newFile, byte[] data);
    }
}
