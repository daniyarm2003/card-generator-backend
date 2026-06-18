using CardGeneratorBackend.Config;
using CardGeneratorBackend.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace CardGeneratorBackend.AI.Embeddings
{
    public class DatabaseEmbeddingCachingStrategy(CardDatabaseContext databaseContext) : IEmbeddingCachingStrategy
    {
        private readonly CardDatabaseContext mDbContext = databaseContext;

        public async Task CacheEmbedding(string text, Vector embedding)
        {
            await mDbContext.AddAsync(new DatabaseEmbeddingCacheEntry
            {
                Text = text,
                Embedding = embedding
            });

            await mDbContext.SaveChangesAsync();
        }

        public async Task<Vector?> GetCachedEmbedding(string text)
        {
            return await mDbContext.EmbeddingCacheEntries
                .Where(entry => entry.Text == text)
                .Select(entry => entry.Embedding)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsCached(string text)
        {
            return await mDbContext.EmbeddingCacheEntries.AnyAsync(entry => entry.Text == text);
        }
    }
}