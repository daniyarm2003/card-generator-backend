using CardGeneratorBackend.Config;
using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Entities;
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
    }
}
