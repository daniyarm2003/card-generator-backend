using CardGeneratorBackend.Config;
using CardGeneratorBackend.DTO;
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
            return await mDatabaseContext.CardTypes.Include(t => t.ImageFile).OrderBy(t => t.Name).ToListAsync();
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

        public async Task<CardType> UpdateCardTypeWithId(Guid typeId, CardTypeUpdateDTO updateDTO)
        {
            var type = await mDatabaseContext.CardTypes.Include(t => t.ImageFile).Where(t => t.Id == typeId).FirstOrDefaultAsync()
                ?? throw new EntityNotFoundException(typeof(CardType), typeId);

            if(updateDTO.BackgroundColorHexCode1 != null)
            {
                type.BackgroundColorHexCode1 = updateDTO.BackgroundColorHexCode1;
            }

            if (updateDTO.BackgroundColorHexCode2 != null)
            {
                type.BackgroundColorHexCode2 = updateDTO.BackgroundColorHexCode2;
            }

            if (updateDTO.TextColor != null)
            {
                type.TextColor = updateDTO.TextColor;
            }

            if(updateDTO.Name != null)
            {
                type.Name = updateDTO.Name;
            }

            var savedTypeUpdateData = mDatabaseContext.CardTypes.Update(type);
            var savedType = savedTypeUpdateData.Entity;

            await mDatabaseContext.SaveChangesAsync();

            return savedType;
        }
    }
}
