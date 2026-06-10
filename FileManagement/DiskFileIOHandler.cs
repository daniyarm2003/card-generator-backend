using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Entities;

namespace CardGeneratorBackend.FileManagement
{
    public class DiskFileIOHandler : IFileIOHandler
    {
        private string DirectoryPath { get; set; }
        private string BackendURL { get; set; }

        public DiskFileIOHandler(string dirPath, string backendURL)
        {
            Directory.CreateDirectory(dirPath);
            DirectoryPath = dirPath;
            BackendURL = backendURL;
        }

        private string GetRelativePath(TrackedFile file)
        {
            return Path.Combine(DirectoryPath, file.Path);
        }

        public async Task<byte[]> ReadAllData(TrackedFile file)
        {
            return await File.ReadAllBytesAsync(GetRelativePath(file));
        }

        public async Task<Stream> GetReadStream(TrackedFile file)
        {
            return await Task.FromResult<Stream>(File.OpenRead(GetRelativePath(file)));
        }

        public async Task WriteAllData(TrackedFile file, byte[] data)
        {
            await File.WriteAllBytesAsync(GetRelativePath(file), data);
        }

        public async Task WriteAllData(TrackedFile file, Stream dataStream)
        {
            using var outputStream = File.OpenWrite(GetRelativePath(file));
            await dataStream.CopyToAsync(outputStream);
        }

        public Task DeleteFile(TrackedFile file)
        {
            File.Delete(GetRelativePath(file));
            return Task.CompletedTask;
        }

        public string GetReadURL(TrackedFile file)
        {
            return $"{BackendURL}/api/files/{file.Id}/content";
        }

        public Task<string> GetUploadURL(TrackedFile file)
        {
            return Task.FromResult($"{BackendURL}/api/files/{file.Id}/content");
        }
    }
}
