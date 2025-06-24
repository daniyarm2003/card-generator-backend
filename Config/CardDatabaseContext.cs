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

        private readonly string mConnectionString = pgOptions.Value.ConnectionString;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(mConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrackedFile>()
                .Property(file => file.StorageLocation)
                .HasConversion(
                    loc => loc.ToString(),
                    locStr => Enum.Parse<FileStorageLocation>(locStr)
                );
        }
    }
}
