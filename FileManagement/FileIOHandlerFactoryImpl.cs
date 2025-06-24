using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Environment;
using Microsoft.Extensions.Options;

namespace CardGeneratorBackend.FileManagement
{
    public class FileIOHandlerFactoryImpl(IOptions<DiskFileStorageParameters> diskFileOptions) : IFileIOHandlerFactory
    {
        private string DiskFileDirectoryPath { get; set; } = diskFileOptions.Value.BaseDirectoryPath;

        public IFileIOHandler GetIOHandler(TrackedFile file)
        {
            return new DiskFileIOHandler(DiskFileDirectoryPath);
        }
    }
}
