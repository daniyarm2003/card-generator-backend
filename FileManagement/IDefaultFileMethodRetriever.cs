using CardGeneratorBackend.Enums;

namespace CardGeneratorBackend.FileManagement
{
    public interface IDefaultFileMethodRetriever
    {
        FileStorageLocation GetDefaultStorageLocation();
    }
}