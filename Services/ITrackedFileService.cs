using CardGeneratorBackend.Entities;
using CardGeneratorBackend.FileManagement;

namespace CardGeneratorBackend.Services
{
    public record FileDownloadInfo(string Name, byte[] Contents);
    public record FileStreamRetrievalInfo(string Name, Stream FileStream);

    public interface ITrackedFileService : IDefaultFileMethodRetriever
    {
        public Task<TrackedFile> CreateAndWriteFile(TrackedFile file, Stream dataStream);

        public Task<byte[]> ReadFile(TrackedFile file);

        public Task<Stream> GetFileReadStream(TrackedFile file);

        public Task DeleteFile(TrackedFile file);

        public string GetFileDownloadName(TrackedFile file);

        public Task<FileDownloadInfo> ReadFileWithId(Guid id);

        public Task<FileStreamRetrievalInfo> GetFileReadStreamWithId(Guid id);

        public string GetFileReadURL(TrackedFile file);

        public Task<string> GetFileUploadURL(TrackedFile file);
    }
}
