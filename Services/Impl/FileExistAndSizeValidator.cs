using CardGeneratorBackend.Environment;
using HeyRed.Mime;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CardGeneratorBackend.Services.Impl
{
    public class FileExistAndSizeValidator(IOptions<FileUploadParameters> fileUploadOptions) : IFileUploadValidationService
    {
        private readonly FileUploadParameters mFileUploadOptions = fileUploadOptions.Value;

        public Task<IActionResult?> ValidateFileUpload(ControllerBase controller, IFormFile file)
        {


            if(file is null)
            {
                return Task.FromResult<IActionResult?>(controller.BadRequest("File was not found in request"));
            }
            else if (file.Length > mFileUploadOptions.MaxFileSizeBytesParsed)
            {
                return Task.FromResult<IActionResult?>(
                    controller.BadRequest($"File's size in bytes is greater than the maximum allowed size of {mFileUploadOptions.MaxFileSizeBytesParsed}"));
            }

            return Task.FromResult<IActionResult?>(null);
        }
    }
}
