using System.Text.RegularExpressions;
using CardGeneratorBackend.AI.Embeddings;
using CardGeneratorBackend.Config;
using CardGeneratorBackend.DTO;
using CardGeneratorBackend.DTO.Mappers;
using CardGeneratorBackend.Environment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Extensions;
using Pgvector;

namespace CardGeneratorBackend.Services.Impl
{
    class CardEmbeddingServiceImpl(IEmbeddingService embeddingService, CardDatabaseContext databaseContext, IOptions<GoogleServiceParameters> geminiParams, CardDTOMapper cardDTOMapper, ILogger<CardEmbeddingServiceImpl> logger, [FromKeyedServices("embedding_db_cache")] IEmbeddingCachingStrategy cachingStrategy) : ICardEmbeddingService
    {
        private readonly IEmbeddingService mEmbeddingService = embeddingService;
        private readonly CardDatabaseContext mDatabaseContext = databaseContext;
        private readonly CardDTOMapper mCardDTOMapper = cardDTOMapper;
        private readonly ILogger<CardEmbeddingServiceImpl> mLogger = logger;

        private readonly IEmbeddingCachingStrategy mCachingStrategy = cachingStrategy;

        private readonly bool mIsFreeTier = geminiParams.Value.IsGeminiAPIFreeTier;

        public async Task<Vector> GetCardEmbedding(CardDTO card)
        {
            var embedText = GetCardEmbedText(card);
            return await mEmbeddingService.GetEmbedding(embedText, null);
        }

        public string GetCardEmbedText(CardDTO card)
        {
            return $"""
            {card.Name} is a(n) {card.Variant.GetDisplayName()} card

            Name: {card.Name}
            Type: {card.Type.Name}
            Quote: {card.Quote ?? "No Quote"}
            Effect: {Regex.Replace(card.Effect ?? "No Effect", "\\s+", "")}
            """;
        }

        public async Task<Vector> GetCardSearchEmbedding(string searchQuery)
        {
            return await mEmbeddingService.GetEmbedding(searchQuery, mCachingStrategy);
        }

        public async Task UpdateAllCardEmbeddings(bool skipExisting)
        {
            mLogger.LogInformation("Updating all card embeddings");

            int maxBatchSize = mIsFreeTier ? 20 : 5000;
            int batchCooldownSeconds = 90;

            var cardQuery = mDatabaseContext.Cards.AsQueryable();

            if(skipExisting)
            {
                cardQuery = cardQuery.Where(card => card.TextEmbedding == null);
            }

            var finalQuery = cardQuery.Include(card => card.Type).OrderBy(card => card.CreatedAt);
            
            int batchStartIndex = 0;
            while(true)
            {
                var cardBatch = await finalQuery.Skip(batchStartIndex).Take(maxBatchSize).ToListAsync();
                batchStartIndex += maxBatchSize;

                if(cardBatch == null || cardBatch.Count == 0)
                {
                    break;
                }

                mLogger.LogInformation("Updating card embeddings for {} cards: {}", cardBatch.Count, cardBatch.First().Name);

                var embedTextBatch = cardBatch
                    .Select(mCardDTOMapper.ToDTO)
                    .Select(GetCardEmbedText)
                    .ToList();

                var embeddings = await mEmbeddingService.GetAllEmbeddings(embedTextBatch);

                if(embeddings.Count != cardBatch.Count)
                {
                    throw new EmbeddingException("Received a mismatched number of embeddings from model");
                }

                for(int i = 0; i < cardBatch.Count; i++)
                {
                    cardBatch[i].TextEmbedding = embeddings[i];
                }

                mDatabaseContext.Cards.UpdateRange(cardBatch);

                mLogger.LogInformation("Generated embeddings for {} cards", cardBatch.Count);
                
                for(int i = 0; i < batchCooldownSeconds; i += batchCooldownSeconds / 10)
                {
                    mLogger.LogInformation("Cooldown until next batch: {} seconds remaining", batchCooldownSeconds - i);
                    await Task.Delay(100 * batchCooldownSeconds);
                }
            }

            await mDatabaseContext.SaveChangesAsync();

            await mDatabaseContext.GlobalStateEntity.ExecuteUpdateAsync(setters => setters
                .SetProperty(state => state.ShouldUpdateCardEmbeddings, false));

            mLogger.LogInformation("Updated all card embeddings");
        }
    }
}