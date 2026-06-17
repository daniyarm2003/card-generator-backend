using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using CardGeneratorBackend.CardGeneration;
using CardGeneratorBackend.Config;
using CardGeneratorBackend.DTO;
using CardGeneratorBackend.DTO.Mappers;
using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Exceptions;
using CardGeneratorBackend.FileManagement;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace CardGeneratorBackend.Services.Impl
{
    public class CardServiceImpl(CardDatabaseContext dbContext, ICardTypeService cardTypeService, ITrackedFileService trackedFileService, ICardImageGeneratorFactory cardImageGeneratorFactory, IDefaultFileMethodRetriever fileMethodRetriever, CardDTOMapper cardDTOMapper) : ICardService
    {
        private readonly CardDatabaseContext mDatabaseContext = dbContext;
        private readonly ICardTypeService mCardTypeService = cardTypeService;
        private readonly ITrackedFileService mTrackedFileService = trackedFileService;
        private readonly ICardImageGeneratorFactory mCardImageGeneratorFactory = cardImageGeneratorFactory;
        private readonly IDefaultFileMethodRetriever mFileMethodRetriever = fileMethodRetriever;
        private readonly CardDTOMapper mCardDTOMapper = cardDTOMapper;

        private static IQueryable<Card> WithIncludedCardRelations(IQueryable<Card> inputQuery)
        {
            return inputQuery
                .Include(card => card.Type)
                    .ThenInclude(type => type.ImageFile)
                .Include(card => card.DisplayImage)
                .Include(card => card.CardImage);
        }

        private static IQueryable<Card> CreateCardSelectQuery(IQueryable<Card> inputQuery)
        {
            return WithIncludedCardRelations(inputQuery)
                .OrderBy(card => card.Variant == Enums.CardVariant.REGULAR ? 0 : 1)
                    .ThenBy(card => card.Number);
        }

        public async Task<CardDTO> GetCardById(Guid id)
        {
            var card = await WithIncludedCardRelations(mDatabaseContext.Cards)
                .Where(c => c.Id == id)
                .Select(c => mCardDTOMapper.ToDTO(c))
                .FirstOrDefaultAsync() 
                ?? throw new EntityNotFoundException(typeof(Card), id);

            return card;
        }

        public async Task<PaginationDTO<CardDTO>> GetCards(CardRetrievalQueryDTO queryDTO)
        {
            var cardQuery = mDatabaseContext.Cards.AsQueryable();

            if(queryDTO.Variant is not null)
            {
                cardQuery = cardQuery.Where(c => c.Variant == queryDTO.Variant);
            }

            if(queryDTO.Level is not null)
            {
                cardQuery = cardQuery.Where(c => c.Level == queryDTO.Level);
            }

            cardQuery = WithIncludedCardRelations(cardQuery)
                .OrderBy(card => card.Variant == Enums.CardVariant.REGULAR ? 0 : 1)
                    .ThenBy(card => card.Number);

            var itemCount = await cardQuery.CountAsync();
            var pageNum = queryDTO.PageSize is not null ? queryDTO.PageNum : 1;
            var pageSize = queryDTO.PageSize ?? 1;

            if(queryDTO.PageSize is not null)
            {
                cardQuery = cardQuery.Skip((queryDTO.PageNum - 1) * pageSize).Take(pageSize);
            }

            var data = cardQuery.Select(mCardDTOMapper.ToDTO).ToList();
            return new(data, pageNum, pageSize, itemCount);
        }

        public async Task<Card> CreateCard(CardCreationDTO dto)
        {
            var cardType = await mCardTypeService.GetCardTypeById(dto.TypeId);

            var newCard = new Card
            {
                Name = dto.Name,
                Number = dto.Number,
                Variant = dto.Variant,
                Level = dto.Level,
                Attack = dto.Attack,
                Health = dto.Health,
                Quote = dto.Quote,
                Effect = dto.Effect,
                Type = cardType
            };

            var createdCard = await mDatabaseContext.Cards.AddAsync(newCard);
            await mDatabaseContext.SaveChangesAsync();

            return createdCard.Entity;
        }

        public async Task<Card> UpdateCardWithId(Guid id, CardUpdateDTO dto)
        {
            var cardToUpdate = await CreateCardSelectQuery(mDatabaseContext.Cards.AsQueryable())
                .Where(card => card.Id == id)
                .FirstOrDefaultAsync() ?? throw new EntityNotFoundException(typeof(Card), id);

            if (dto.TypeId != null)
            {
                var newType = await mCardTypeService.GetCardTypeById((Guid)dto.TypeId);
                cardToUpdate.Type = newType;
            }

            if (dto.Name != null)
            {
                cardToUpdate.Name = dto.Name;
            }

            if (dto.Number != null)
            {
                cardToUpdate.Number = (int)dto.Number;
            }

            if (dto.Quote != null)
            {
                cardToUpdate.Quote = dto.Quote;
            }

            if (dto.Effect != null)
            {
                cardToUpdate.Effect = dto.Effect;
            }

            if (cardToUpdate.Variant == Enums.CardVariant.REGULAR)
            {
                if (dto.Attack != null)
                {
                    cardToUpdate.Attack = dto.Attack;
                }

                if (dto.Health != null)
                {
                    cardToUpdate.Health = dto.Health;
                }

                if (dto.Level != null)
                {
                    cardToUpdate.Level = dto.Level;
                }
            }

            var updatedCardResult = mDatabaseContext.Cards.Update(cardToUpdate);
            await mDatabaseContext.SaveChangesAsync();

            return updatedCardResult.Entity;
        }

        public async Task<UploadURLResponseDTO> CreateCardDisplayImageUploadURL(Guid id, string filename)
        {
            var card = await mDatabaseContext.Cards
                .Where(card => card.Id == id)
                .Include(card => card.DisplayImage)
                .FirstOrDefaultAsync() ?? throw new EntityNotFoundException(typeof(Card), id);

            if(card.DisplayImage is not null)
            {
                await mTrackedFileService.DeleteFile(card.DisplayImage);
            }

            var sanitizedCardName = Regex.Replace(card.Name, "[^a-zA-Z0-9]", "");
            var actualFileName = $"DisplayImage_{sanitizedCardName}_{Guid.NewGuid()}{Path.GetExtension(filename)}";

            var imageFile = new TrackedFile
            {
                Path = actualFileName,
                StorageLocation = mFileMethodRetriever.GetDefaultStorageLocation()
            };

            var createdImageData = await mDatabaseContext.TrackedFiles.AddAsync(imageFile);
            card.DisplayImage = createdImageData.Entity;

            mDatabaseContext.Cards.Update(card);
            await mDatabaseContext.SaveChangesAsync();

            var uploadURL = await mTrackedFileService.GetFileUploadURL(card.DisplayImage);
            var contentType = HeyRed.Mime.MimeTypesMap.GetMimeType(card.DisplayImage.Path);

            return new UploadURLResponseDTO
            {
                UploadURL = uploadURL,
                ContentType = contentType
            };
        }

        public async Task<Card> GenerateAndUpdateCardImage(Guid id)
        {
            var cardToUpdate = await CreateCardSelectQuery(mDatabaseContext.Cards.AsQueryable())
                .Where(card => card.Id == id)
                .FirstOrDefaultAsync() ?? throw new EntityNotFoundException(typeof(Card), id);

            var cardDTO = mCardDTOMapper.ToDTO(cardToUpdate);
            var cardBitmap = new SKBitmap(500, 750);

            using var cardCanvas = new SKCanvas(cardBitmap);
            cardCanvas.Clear(SKColors.Transparent);

            var imageGenerator = mCardImageGeneratorFactory.GetCardImageGenerator(cardDTO);
            await imageGenerator.GenerateCardImage(cardDTO, cardCanvas, cardBitmap.Width, cardBitmap.Height);

            using var cardImage = SKImage.FromBitmap(cardBitmap);
            using var cardImageData = cardImage.Encode(SKEncodedImageFormat.Png, 100);

            if(cardToUpdate.CardImage is not null)
            {
                await mTrackedFileService.DeleteFile(cardToUpdate.CardImage);
            }

            var sanitizedCardName = Regex.Replace(cardToUpdate.Name, "[^a-zA-Z0-9]", "");
            var filename = $"CardImage_{sanitizedCardName}_{Guid.NewGuid()}.png";

            var newCardImageFile = new TrackedFile
            {
                Path = filename,
                StorageLocation = mFileMethodRetriever.GetDefaultStorageLocation()
            };

            using(var imageStream = cardImageData.AsStream())
            {
                cardToUpdate.CardImage = await mTrackedFileService.CreateAndWriteFile(newCardImageFile, imageStream);
            }

            var savedCardResult = mDatabaseContext.Cards.Update(cardToUpdate);
            var savedCard = savedCardResult.Entity;

            await mDatabaseContext.SaveChangesAsync();

            return savedCard;
        }

        public async Task DeleteCard(Card card)
        {
            var displayImageFile = card.DisplayImage;
            var cardImageFile = card.CardImage;

            mDatabaseContext.Cards.Remove(card);
            await mDatabaseContext.SaveChangesAsync();

            if (displayImageFile is not null)
            {
                await mTrackedFileService.DeleteFile(displayImageFile);
            }

            if (cardImageFile is not null)
            {
                await mTrackedFileService.DeleteFile(cardImageFile);
            }
        }

        public async Task DeleteCardById(Guid id)
        {
            var cardToDelete = await CreateCardSelectQuery(mDatabaseContext.Cards.AsQueryable())
                .Where(card => card.Id == id)
                .FirstOrDefaultAsync() ?? throw new EntityNotFoundException(typeof(Card), id);

            await DeleteCard(cardToDelete);
        }
    }
}
