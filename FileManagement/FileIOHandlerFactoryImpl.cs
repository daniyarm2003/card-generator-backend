using Amazon.S3;
using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Environment;
using Google.Apis.Drive.v3;
using Microsoft.Extensions.Options;

namespace CardGeneratorBackend.FileManagement
{
    public class FileIOHandlerFactoryImpl(IOptions<DiskFileStorageParameters> diskFileOptions, 
        IOptions<AWSParameters> awsOptions, IAmazonS3 s3Service, EnvironmentHelper envHelper) : IFileIOHandlerFactory
    {
        private string DiskFileDirectoryPath { get; set; } = diskFileOptions.Value.BaseDirectoryPath;
        private string S3BucketName { get; set; } = awsOptions.Value.S3BucketName;
        private string AWSRegionName { get; set; } = awsOptions.Value.Region;

        public IFileIOHandler GetIOHandler(TrackedFile file)
        {

            if(file.StorageLocation == Enums.FileStorageLocation.S3)
            {
                return new S3FileIOHandler(s3Service, S3BucketName, AWSRegionName);
            }

            return new DiskFileIOHandler(DiskFileDirectoryPath, envHelper.GetBackendURL());
        }
    }
}
