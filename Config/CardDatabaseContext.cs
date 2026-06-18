using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Enums;
using CardGeneratorBackend.Environment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CardGeneratorBackend.Config
{
    public class CardDatabaseContext(IOptions<PostgresConnectionParameters> pgOptions) : DbContext
    {
        public DbSet<CardType> CardTypes { get; set; }
        public DbSet<TrackedFile> TrackedFiles { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<GlobalState> GlobalStateEntity { get; set; }
        public DbSet<DatabaseEmbeddingCacheEntry> EmbeddingCacheEntries { get; set; }

        private readonly string mConnectionString = pgOptions.Value.ConnectionString;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(mConnectionString, npgSQL =>
            {
                npgSQL.UseVector();
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("vector");

            modelBuilder.Entity<TrackedFile>()
                .Property(file => file.StorageLocation)
                .HasConversion(
                    loc => loc.ToString(),
                    locStr => Enum.Parse<FileStorageLocation>(locStr)
                );

            modelBuilder.Entity<Card>(cardEntity =>
            {
                cardEntity.Property(card => card.Variant)
                    .HasConversion(
                        variant => variant.ToString(),
                        variantStr => Enum.Parse<CardVariant>(variantStr)
                    );

                cardEntity.Property(card => card.TextEmbedding)
                    .HasColumnType("vector(768)");
            }); 

            modelBuilder.Entity<CardType>()
                .HasData(new CardType() {
                    Id = new Guid(CardType.NONE_TYPE_UUID),
                    Name = "None",
                    BackgroundColorHexCode1 = "ffffff",
                    BackgroundColorHexCode2 = "ffffff",
                    TextColor = "000000"
                });

            modelBuilder.Entity<GlobalState>()
                .HasData(new GlobalState
                {
                    Id = new Guid(GlobalState.GLOBAL_STATE_UUID),
                    ShouldUpdateCardEmbeddings = true
                });

            modelBuilder.Entity<DatabaseEmbeddingCacheEntry>(builder =>
            {
                builder.HasKey(entry => entry.Text);

                builder.Property(entity => entity.Embedding)
                    .HasColumnType("vector(768)");
            });
        }

        public async Task<GlobalState> GetGlobalState()
        {
            return await GlobalStateEntity.FirstAsync();
        }
    }
}
