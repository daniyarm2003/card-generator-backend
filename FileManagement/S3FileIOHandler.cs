using Amazon.S3;
using Amazon.S3.Model;
using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Environment;
using HeyRed.Mime;
using Microsoft.Extensions.Options;

namespace CardGeneratorBackend.FileManagement
{
    public class S3FileIOHandler(IAmazonS3 s3Client, string bucketName, string awsRegionName) : IFileIOHandler
    {
        private const int PRESIGNED_URL_EXPIRATION_MINUTES = 15;

        private readonly IAmazonS3 mS3Client = s3Client;
        private readonly string mBucketName = bucketName;
        private readonly string mAWSRegionName = awsRegionName;
        public Task DeleteFile(TrackedFile file)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = mBucketName,
                Key = file.Path
            };

            return mS3Client.DeleteObjectAsync(deleteRequest);
        }

        public async Task<Stream> GetReadStream(TrackedFile file)
        {
            var response = await mS3Client.GetObjectAsync(mBucketName, file.Path);
            return response.ResponseStream;
        }

        public async Task<byte[]> ReadAllData(TrackedFile file)
        {
            using Stream readStream = await GetReadStream(file);
            using var memStream = new MemoryStream();

            readStream.CopyTo(memStream);

            return memStream.ToArray();
        }

        public async Task WriteAllData(TrackedFile file, byte[] data)
        {
            using var memStream = new MemoryStream(data);

            var putRequest = new PutObjectRequest
            {
                BucketName = mBucketName,
                Key = file.Path,
                InputStream = memStream
            };

            await mS3Client.PutObjectAsync(putRequest);
        }

        public string GetReadURL(TrackedFile file)
        {
            return $"https://{mBucketName}.s3.{mAWSRegionName}.amazonaws.com/{file.Path}";
        }

        public async Task<string> GetUploadURL(TrackedFile file)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = mBucketName,
                Key = file.Path,
                Expires = DateTime.UtcNow.AddMinutes(PRESIGNED_URL_EXPIRATION_MINUTES),
                Verb = HttpVerb.PUT,
                ContentType = MimeTypesMap.GetMimeType(file.Path),
            };

            return await mS3Client.GetPreSignedURLAsync(request);
        }
    }
}