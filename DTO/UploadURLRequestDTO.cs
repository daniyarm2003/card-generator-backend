using System.ComponentModel.DataAnnotations;

namespace CardGeneratorBackend.DTO
{
    public class UploadURLRequestDTO
    {
        [Required]
        public required string FileName { get; set; }
    }
}