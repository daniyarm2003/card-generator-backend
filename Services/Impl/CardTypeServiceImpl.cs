using CardGeneratorBackend.Config;
using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CardGeneratorBackend.Services.Impl
{
    public class CardTypeServiceImpl(CardDatabaseContext dbContext, ITrackedFileService fileService) : ICardTypeService
    {
        private readonly CardDatabaseContext mDatabaseContext = dbContext;
        private readonly ITrackedFileService mFileService = fileService;

        public async Task<CardType> CreateCardType(CardType creationData)
        {
            var createdType = await mDatabaseContext.CardTypes.AddAsync(creationData) 
                ?? throw new EntityNotSavedException(creationData, "Unable to create new card type");

            await mDatabaseContext.SaveChangesAsync();

            return createdType.Entity;
        }

        public async Task<IEnumerable<CardType>> GetAllCardTypes()
        {
            return await mDatabaseContext.CardTypes.Include(t => t.ImageFile).ToListAsync();
        }

        public async Task<CardType> UpdateCardTypeImage(Guid typeId, string fileName, byte[] data)
        {
            var type = await mDatabaseContext.CardTypes.Include(t => t.ImageFile).Where(t => t.Id == typeId).FirstOrDefaultAsync() 
                ?? throw new EntityNotFoundException(typeof(CardType), typeId);

            if(type.ImageFile is not null)
            {
                await mFileService.DeleteFile(type.ImageFile);
            }

            var newImageFile = new TrackedFile() { 
                Path = fileName,
                StorageLocation = Enums.FileStorageLocation.Disk
            };

            newImageFile = await mFileService.CreateAndWriteFile(newImageFile, data);
            type.ImageFile = newImageFile;

            var savedTypeUpdateData = mDatabaseContext.CardTypes.Update(type);
            var savedType = savedTypeUpdateData.Entity;

            await mDatabaseContext.SaveChangesAsync();

            return savedType;
        }
    }
}
