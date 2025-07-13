using Microsoft.AspNetCore.Mvc;

namespace CardGeneratorBackend.Services
{
    public interface IFileUploadValidationService
    {
        public Task<IActionResult?> ValidateFileUpload(ControllerBase controller, IFormFile file);
    }
}
