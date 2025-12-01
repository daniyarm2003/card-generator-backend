using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Environment;
using Google.Apis.Drive.v3;
using Microsoft.Extensions.Options;

namespace CardGeneratorBackend.FileManagement
{
    public class FileIOHandlerFactoryImpl(IOptions<DiskFileStorageParameters> diskFileOptions, 
        IOptions<GoogleServiceParameters> googleServiceOptions, 
        DriveService driveService) : IFileIOHandlerFactory
    {
        private string DiskFileDirectoryPath { get; set; } = diskFileOptions.Value.BaseDirectoryPath;
        private string GoogleDriveFolderId { get; set; } = googleServiceOptions.Value.GoogleDriveStorageFolderId;

        public IFileIOHandler GetIOHandler(TrackedFile file)
        {
            if(file.StorageLocation == Enums.FileStorageLocation.GoogleDrive)
            {
                return new GoogleDriveFileIOHandler(driveService, GoogleDriveFolderId);
            }

            return new DiskFileIOHandler(DiskFileDirectoryPath);
        }
    }
}
