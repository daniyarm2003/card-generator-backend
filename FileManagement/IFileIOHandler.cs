using CardGeneratorBackend.Entities;

namespace CardGeneratorBackend.FileManagement
{
    public interface IFileIOHandler
    {
        public Task<byte[]> ReadAllData(TrackedFile file);

        public Task WriteAllData(TrackedFile file, byte[] data);

        public Task DeleteFile(TrackedFile file);
    }
}
