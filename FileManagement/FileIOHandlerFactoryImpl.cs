using Amazon.S3;
using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Environment;
using Google.Apis.Drive.v3;
using Microsoft.Extensions.Options;

namespace CardGeneratorBackend.FileManagement
{
    public class FileIOHandlerFactoryImpl(IOptions<DiskFileStorageParameters> diskFileOptions, 
        IOptions<GoogleServiceParameters> googleServiceOptions, 
        IOptions<AWSParameters> awsOptions,
        DriveService driveService, IAmazonS3 s3Service) : IFileIOHandlerFactory
    {
        private string DiskFileDirectoryPath { get; set; } = diskFileOptions.Value.BaseDirectoryPath;
        private string GoogleDriveFolderId { get; set; } = googleServiceOptions.Value.GoogleDriveStorageFolderId;
        private string S3BucketName { get; set; } = awsOptions.Value.S3BucketName;

        public IFileIOHandler GetIOHandler(TrackedFile file)
        {
            if(file.StorageLocation == Enums.FileStorageLocation.GoogleDrive)
            {
                return new GoogleDriveFileIOHandler(driveService, GoogleDriveFolderId);
            }

            if(file.StorageLocation == Enums.FileStorageLocation.S3)
            {
                return new S3FileIOHandler(s3Service, S3BucketName);
            }

            return new DiskFileIOHandler(DiskFileDirectoryPath);
        }
    }
}
