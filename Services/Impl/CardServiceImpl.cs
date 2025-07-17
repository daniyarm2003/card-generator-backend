using CardGeneratorBackend.CardGeneration;
using CardGeneratorBackend.Config;
using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Exceptions;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace CardGeneratorBackend.Services.Impl
{
    public class CardServiceImpl(CardDatabaseContext dbContext, ICardTypeService cardTypeService, ITrackedFileService trackedFileService, ICardImageGeneratorFactory cardImageGeneratorFactory) : ICardService
    {
        private readonly CardDatabaseContext mDatabaseContext = dbContext;
        private readonly ICardTypeService mCardTypeService = cardTypeService;
        private readonly ITrackedFileService mTrackedFileService = trackedFileService;
        private readonly ICardImageGeneratorFactory mCardImageGeneratorFactory = cardImageGeneratorFactory;

        private static IQueryable<Card> CreateCardSelectQuery(IQueryable<Card> inputQuery)
        {
            return inputQuery
                .Include(card => card.Type)
                    .ThenInclude(type => type.ImageFile)
                .Include(card => card.DisplayImage)
                .Include(card => card.CardImage)
                .OrderBy(card => card.Number);
        }

        public async Task<IEnumerable<Card>> GetAllCards()
        {
            var query = CreateCardSelectQuery(mDatabaseContext.Cards.AsQueryable());
            return await query.ToListAsync();
        }

        public async Task<PaginationDTO<Card>> GetCardsPaginated(int pageNum, int pageSize)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageNum, 1);

            var query = CreateCardSelectQuery(mDatabaseContext.Cards.AsQueryable());
            var count = await query.CountAsync();

            query = query.Skip((pageNum - 1) * pageSize).Take(pageSize);
            var data = await query.ToListAsync();

            return new(data, pageNum, pageSize, count);
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

            if(dto.TypeId != null)
            {
                var newType = await mCardTypeService.GetCardTypeById((Guid)dto.TypeId);
                cardToUpdate.Type = newType;
            }

            if(dto.Name != null)
            {
                cardToUpdate.Name = dto.Name;
            }

            if(dto.Number != null)
            {
                cardToUpdate.Number = (int)dto.Number;
            }

            if(dto.Quote != null)
            {
                cardToUpdate.Quote = dto.Quote;
            }

            if(dto.Effect != null)
            {
                cardToUpdate.Effect = dto.Effect;
            }

            if(cardToUpdate.Variant == Enums.CardVariant.REGULAR)
            {
                if(dto.Attack != null)
                {
                    cardToUpdate.Attack = dto.Attack;
                }

                if(dto.Health != null)
                {
                    cardToUpdate.Health = dto.Health;
                }

                if(dto.Level != null)
                {
                    cardToUpdate.Level = dto.Level;
                }
            }

            var updatedCardResult = mDatabaseContext.Cards.Update(cardToUpdate);
            await mDatabaseContext.SaveChangesAsync();

            return updatedCardResult.Entity;
        }

        public async Task<Card> UpdateCardDisplayImage(Guid id, string filename, byte[] data)
        {
            var cardToUpdate = await CreateCardSelectQuery(mDatabaseContext.Cards.AsQueryable())
                .Where(card => card.Id == id)
                .FirstOrDefaultAsync() ?? throw new EntityNotFoundException(typeof(Card), id);

            cardToUpdate.DisplayImage = await mTrackedFileService.WriteOrReplaceFileContents(cardToUpdate.DisplayImage?.Id, new TrackedFile() {
                Path = filename,
                StorageLocation = Enums.FileStorageLocation.Disk
            }, data);

            var savedCardResult = mDatabaseContext.Cards.Update(cardToUpdate);
            var savedCard = savedCardResult.Entity;

            await mDatabaseContext.SaveChangesAsync();

            return savedCard;
        }

        public async Task<Card> GenerateAndUpdateCardImage(Guid id, string filename)
        {
            var cardToUpdate = await CreateCardSelectQuery(mDatabaseContext.Cards.AsQueryable())
                .Where(card => card.Id == id)
                .FirstOrDefaultAsync() ?? throw new EntityNotFoundException(typeof(Card), id);

            var cardDTO = cardToUpdate.GetDTO();
            var cardBitmap = new SKBitmap(500, 750);

            using var cardCanvas = new SKCanvas(cardBitmap);
            cardCanvas.Clear(SKColors.Transparent);

            var imageGenerator = mCardImageGeneratorFactory.GetCardImageGenerator(cardDTO);
            await imageGenerator.GenerateCardImage(cardDTO, cardCanvas, cardBitmap.Width, cardBitmap.Height);

            using var cardImage = SKImage.FromBitmap(cardBitmap);
            using var cardImageData = cardImage.Encode(SKEncodedImageFormat.Png, 100);

            var imageStream = new MemoryStream();
            cardImageData.SaveTo(imageStream);

            imageStream.Position = 0;
            byte[] imageData = imageStream.ToArray();

            cardToUpdate.CardImage = await mTrackedFileService.WriteOrReplaceFileContents(cardToUpdate.CardImage?.Id, new TrackedFile
            {
                Path = filename,
                StorageLocation = Enums.FileStorageLocation.Disk
            }, imageData);

            var savedCardResult = mDatabaseContext.Cards.Update(cardToUpdate);
            var savedCard = savedCardResult.Entity;

            await mDatabaseContext.SaveChangesAsync();

            return savedCard;
        }
    }
}
