using CardGeneratorBackend.Exceptions;
using CardGeneratorBackend.Services;
using HeyRed.Mime;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CardGeneratorBackend.Controllers
{

    [ApiController]
    [Route("api/files")]
    public class TrackedFileController(ITrackedFileService fileService) : ControllerBase
    {
        private readonly ITrackedFileService mFileService = fileService;

        [HttpGet("{id}/content")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReadTrackedFile(Guid id)
        {
            var fileDownloadInfo = await mFileService.ReadFileWithId(id);
            var mimeType = MimeTypesMap.GetMimeType(fileDownloadInfo.Name) ?? "application/octet-stream";

            Response.Headers.CacheControl = "no-store";

            return new FileContentResult(fileDownloadInfo.Contents, mimeType);
        }
    }
}
