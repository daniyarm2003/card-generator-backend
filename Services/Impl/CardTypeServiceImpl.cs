using CardGeneratorBackend.Config;
using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Exceptions;
using CardGeneratorBackend.FileManagement;
using Microsoft.EntityFrameworkCore;

namespace CardGeneratorBackend.Services.Impl
{
    public class CardTypeServiceImpl(CardDatabaseContext dbContext, ITrackedFileService fileService, IDefaultFileMethodRetriever fileMethodRetriever) : ICardTypeService
    {
        private readonly CardDatabaseContext mDatabaseContext = dbContext;
        private readonly ITrackedFileService mFileService = fileService;
        private readonly IDefaultFileMethodRetriever mFileMethodRetriever = fileMethodRetriever;

        private static IQueryable<CardType> GetDefaultCardTypeQuery(IQueryable<CardType> inQuery)
        {
            return inQuery.Include(type => type.ImageFile);
        }

        public async Task<CardType> GetCardTypeById(Guid id)
        {
            return await GetDefaultCardTypeQuery(mDatabaseContext.CardTypes.AsQueryable()).Where(type => type.Id == id).FirstOrDefaultAsync() 
                ?? throw new EntityNotFoundException(typeof(CardType), id);
        }

        public async Task<CardType> CreateCardType(CardType creationData)
        {
            var createdType = await mDatabaseContext.CardTypes.AddAsync(creationData) 
                ?? throw new EntityNotSavedException(creationData, "Unable to create new card type");

            await mDatabaseContext.SaveChangesAsync();

            return createdType.Entity;
        }

        public async Task<IEnumerable<CardType>> GetAllCardTypes()
        {
            return await GetDefaultCardTypeQuery(mDatabaseContext.CardTypes.AsQueryable()).OrderBy(t => t.Name).ToListAsync();
        }

        public async Task<CardType> UpdateCardTypeImage(Guid typeId, string fileName, byte[] data)
        {
            var type = await GetCardTypeById(typeId);

            type.ImageFile = await mFileService.WriteOrReplaceFileContents(type.ImageFile?.Id, new TrackedFile()
            {
                Path = fileName,
                StorageLocation = mFileMethodRetriever.GetDefaultStorageLocation()
            }, data);

            var savedTypeUpdateData = mDatabaseContext.CardTypes.Update(type);
            var savedType = savedTypeUpdateData.Entity;

            await mDatabaseContext.SaveChangesAsync();

            return savedType;
        }

        public async Task<CardType> UpdateCardTypeWithId(Guid typeId, CardTypeUpdateDTO updateDTO)
        {
            var type = await GetCardTypeById(typeId);

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
