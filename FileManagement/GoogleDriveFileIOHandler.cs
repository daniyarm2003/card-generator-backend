using CardGeneratorBackend.Entities;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Upload;
using HeyRed.Mime;
using static Google.Apis.Drive.v3.FilesResource;

namespace CardGeneratorBackend.FileManagement
{
    public class GoogleDriveFileIOHandler(DriveService driveService, string driveFolderId) : IFileIOHandler
    {
        private readonly DriveService mDriveService = driveService;
        private readonly string mDriveFolderId = driveFolderId;

        private async Task<Google.Apis.Drive.v3.Data.File?> GetFileInFolderByName(string name)
        {
            ListRequest fileListRequest = mDriveService.Files.List();

            fileListRequest.Q = $"'{mDriveFolderId}' in parents and name = '{name}' and trashed = false";
            fileListRequest.Fields = "files(id, name, mimeType)";

            var fileList = await fileListRequest.ExecuteAsync();

            return fileList.Files.Count > 0 ? fileList.Files.First() : null;
        }

        public async Task<Stream> GetReadStream(TrackedFile file)
        {
            var driveFile = await GetFileInFolderByName(file.Path) ?? throw new Exception($"File {file.Path} not found in Drive folder");

            GetRequest fileGetRequest = mDriveService.Files.Get(driveFile.Id);

            fileGetRequest.Alt = GetRequest.AltEnum.Media;

            var memStream = new MemoryStream();

            var result = await fileGetRequest.DownloadAsync(memStream);
            result.ThrowOnFailure();

            memStream.Position = 0;

            return memStream;
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
            var driveFile = await GetFileInFolderByName(file.Path);
            using var memStream = new MemoryStream(data);

            if (driveFile == null)
            {
                string mimeType = MimeTypesMap.GetMimeType(file.Path);

                var fileToCreate = new Google.Apis.Drive.v3.Data.File
                {
                    Name = file.Path,
                    Parents = [mDriveFolderId]
                };

                CreateMediaUpload createRequest = mDriveService.Files.Create(fileToCreate, memStream, mimeType);
                IUploadProgress uploadResult = await createRequest.UploadAsync();

                uploadResult.ThrowOnFailure();
            }
            else
            {
                string mimeType = driveFile.MimeType;

                var fileUpdateData = new Google.Apis.Drive.v3.Data.File
                {
                    
                };

                UpdateMediaUpload updateRequest = mDriveService.Files.Update(fileUpdateData, driveFile.Id, memStream, mimeType);
                IUploadProgress uploadResult = await updateRequest.UploadAsync();

                uploadResult.ThrowOnFailure();
            }
        }
        public async Task DeleteFile(TrackedFile file)
        {
            string fileId = file.Path;
            DeleteRequest deleteRequest = mDriveService.Files.Delete(fileId);

            try
            {
                await deleteRequest.ExecuteAsync();
            }
            catch (Exception)
            {
                throw new Exception($"Unable to delete drive file with ID {fileId}");
            }
        }
    }
}
