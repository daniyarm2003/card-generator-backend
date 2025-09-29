using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Entities;

namespace CardGeneratorBackend.FileManagement
{
    public class DiskFileIOHandler : IFileIOHandler
    {
        private string DirectoryPath { get; set; }

        public DiskFileIOHandler(string dirPath)
        {
            Directory.CreateDirectory(dirPath);
            DirectoryPath = dirPath;
        }

        private string GetRelativePath(TrackedFile file)
        {
            return Path.Combine(DirectoryPath, file.Path);
        }

        public Task<byte[]> ReadAllData(TrackedFile file)
        {
            return File.ReadAllBytesAsync(GetRelativePath(file));
        }

        public Task<Stream> GetReadStream(TrackedFile file)
        {
            return Task.FromResult<Stream>(File.OpenRead(GetRelativePath(file)));
        }

        public Task WriteAllData(TrackedFile file, byte[] data)
        {
            return File.WriteAllBytesAsync(GetRelativePath(file), data);
        }

        public Task DeleteFile(TrackedFile file)
        {
            File.Delete(GetRelativePath(file));
            return Task.CompletedTask;
        }
    }
}
