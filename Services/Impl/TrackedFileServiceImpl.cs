using CardGeneratorBackend.Config;
using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Enums;
using CardGeneratorBackend.Exceptions;
using CardGeneratorBackend.FileManagement;
using Microsoft.EntityFrameworkCore;

namespace CardGeneratorBackend.Services.Impl
{
    public class TrackedFileServiceImpl(CardDatabaseContext dbContext, IFileIOHandlerFactory fileIOHandlerFactory, IDefaultFileMethodRetriever defaultFileMethodRetriever) : ITrackedFileService
    {
        private CardDatabaseContext DatabaseContext { get; } = dbContext;
        private IFileIOHandlerFactory FileIOHandlerFactory { get; } = fileIOHandlerFactory;
        private IDefaultFileMethodRetriever DefaultFileMethodRetriever { get; } = defaultFileMethodRetriever;


        public async Task<TrackedFile> CreateAndWriteFile(TrackedFile file, Stream data)
        {
            var savedFileEntry = await DatabaseContext.TrackedFiles.AddAsync(file) 
                ?? throw new EntityNotSavedException(file, $"File {file.Path} was not saved to database");

            var savedFile = savedFileEntry.Entity;
            var ioHandler = FileIOHandlerFactory.GetIOHandler(savedFile);

            await ioHandler.WriteAllData(savedFile, data);
            await DatabaseContext.SaveChangesAsync();

            return savedFile;
        }

        public async Task DeleteFile(TrackedFile file)
        {
            var ioHandler = FileIOHandlerFactory.GetIOHandler(file);
            await ioHandler.DeleteFile(file);

            DatabaseContext.TrackedFiles.Remove(file);
            await DatabaseContext.SaveChangesAsync();
        }

        public Task<byte[]> ReadFile(TrackedFile file)
        {
            var ioHandler = FileIOHandlerFactory.GetIOHandler(file);
            return ioHandler.ReadAllData(file);
        }

        public Task<Stream> GetFileReadStream(TrackedFile file)
        {
            var ioHandler = FileIOHandlerFactory.GetIOHandler(file);
            return ioHandler.GetReadStream(file);
        }

        public string GetFileDownloadName(TrackedFile file)
        {
            var fileExtension = Path.GetExtension(file.Path);
            return $"{file.Id}{fileExtension}";
        }

        public async Task<FileDownloadInfo> ReadFileWithId(Guid id)
        {
            var trackedFile = await DatabaseContext.TrackedFiles.Where(file => file.Id == id).FirstOrDefaultAsync() 
                ?? throw new EntityNotFoundException(typeof(TrackedFile), id);

            var contents = await ReadFile(trackedFile);

            return new(GetFileDownloadName(trackedFile), contents);
        }

        public async Task<FileStreamRetrievalInfo> GetFileReadStreamWithId(Guid id)
        {
            var trackedFile = await DatabaseContext.TrackedFiles.Where(file => file.Id == id).FirstOrDefaultAsync() 
                ?? throw new EntityNotFoundException(typeof(TrackedFile), id);

            var stream = await GetFileReadStream(trackedFile);

            return new(GetFileDownloadName(trackedFile), stream);
        }

        public FileStorageLocation GetDefaultStorageLocation()
        {
            return DefaultFileMethodRetriever.GetDefaultStorageLocation();
        }

        public string GetFileReadURL(TrackedFile file)
        {
            var ioHandler = FileIOHandlerFactory.GetIOHandler(file);
            return ioHandler.GetReadURL(file);
        }

        public Task<string> GetFileUploadURL(TrackedFile file)
        {
            var ioHandler = FileIOHandlerFactory.GetIOHandler(file);
            return ioHandler.GetUploadURL(file);
        }
    }
}
