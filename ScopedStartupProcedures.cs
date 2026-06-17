using CardGeneratorBackend.Config;
using CardGeneratorBackend.Services;
using Microsoft.EntityFrameworkCore;

namespace CardGeneratorBackend {    
    public class ScopedStartupProcedures(ILogger<ScopedStartupProcedures> logger, CardDatabaseContext dbContext, ICardEmbeddingService cardEmbeddingService)
    {
        private readonly ILogger<ScopedStartupProcedures> mLogger = logger;
        private readonly CardDatabaseContext mDbContext = dbContext;
        private readonly ICardEmbeddingService mCardEmbeddingService = cardEmbeddingService;

        public async Task RunMainProcedure()
        {
            await mDbContext.Database.MigrateAsync();
            mLogger.LogInformation("Database migrations applied successfully");

            var globalState = await mDbContext.GetGlobalState();
            var updateCardEmbeddings = globalState.ShouldUpdateCardEmbeddings;

            if(updateCardEmbeddings)
            {
                await mCardEmbeddingService.UpdateAllCardEmbeddings(false);
            }
        }
    }
}