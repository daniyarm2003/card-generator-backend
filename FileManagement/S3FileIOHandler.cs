using Amazon.S3;
using Amazon.S3.Model;
using CardGeneratorBackend.Entities;

namespace CardGeneratorBackend.FileManagement
{
    public class S3FileIOHandler(IAmazonS3 s3Client, string bucketName) : IFileIOHandler
    {
        private readonly IAmazonS3 mS3Client = s3Client;
        private readonly string mBucketName = bucketName;

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
    }
}