using CardGeneratorBackend.Config;
using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CardGeneratorBackend.Services.Impl
{
    public class CardServiceImpl(CardDatabaseContext dbContext, ICardTypeService cardTypeService) : ICardService
    {
        private readonly CardDatabaseContext mDatabaseContext = dbContext;
        private readonly ICardTypeService mCardTypeService = cardTypeService;

        private static IQueryable<Card> CreateCardSelectQuery(IQueryable<Card> inputQuery)
        {
            return inputQuery
                .Include(card => card.Type)
                    .ThenInclude(type => type.ImageFile)
                .Include(card => card.DisplayImage)
                .Include(card => card.CardImage)
                .OrderBy(card => card.CreatedAt);
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
    }
}
