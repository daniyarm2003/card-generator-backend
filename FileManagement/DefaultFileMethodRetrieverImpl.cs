using CardGeneratorBackend.Enums;

namespace CardGeneratorBackend.FileManagement
{
    public class DefaultFileMethodRetrieverImpl() : IDefaultFileMethodRetriever
    {
        public FileStorageLocation GetDefaultStorageLocation()
        {
            return FileStorageLocation.S3;
        }
    }
}