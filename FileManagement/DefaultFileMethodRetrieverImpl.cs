using CardGeneratorBackend.Enums;

namespace CardGeneratorBackend.FileManagement
{
    public class DefaultFileMethodRetrieverImpl(IWebHostEnvironment environment) : IDefaultFileMethodRetriever
    {
        public FileStorageLocation GetDefaultStorageLocation()
        {
            if(environment.IsDevelopment())
            {
                return FileStorageLocation.Disk;
            }
            else
            {
                return FileStorageLocation.S3;
            }
        }
    }
}