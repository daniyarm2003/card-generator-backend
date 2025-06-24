using CardGeneratorBackend.Exceptions;
using CardGeneratorBackend.Services;
using HeyRed.Mime;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CardGeneratorBackend.Controllers
{

    [ApiController]
    [Route("api/files")]
    public class TrackedFileController(ITrackedFileService fileService, ILogger<TrackedFileController> logger) : ControllerBase
    {
        private readonly ITrackedFileService mFileService = fileService;
        private readonly ILogger<TrackedFileController> mLogger = logger;

        [HttpGet("{id}/content")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReadTrackedFile(Guid id)
        {
            try
            {
                var fileDownloadInfo = await mFileService.ReadFileWithId(id);
                var mimeType = MimeTypesMap.GetMimeType(fileDownloadInfo.Name) ?? "application/octet-stream";

                return new FileContentResult(fileDownloadInfo.Contents, mimeType)
                {
                    FileDownloadName = fileDownloadInfo.Name
                };
            }
            catch(EntityNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch(Exception e)
            {
                mLogger.LogError(e, "Unexpected error");
                return Problem("An unexpected server error has occurred");
            }
        }
    }
}
