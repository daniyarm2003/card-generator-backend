using CardGeneratorBackend.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CardGeneratorBackend.Entities
{
    [Table("tracked_files")]
    public class TrackedFile
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("path")]
        public required string Path { get; set; }

        [Column("storage_location")]
        public FileStorageLocation StorageLocation { get; set; } = FileStorageLocation.Disk;
    }
}
