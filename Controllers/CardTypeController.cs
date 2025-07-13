using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Environment;
using CardGeneratorBackend.Exceptions;
using CardGeneratorBackend.Services;
using HeyRed.Mime;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CardGeneratorBackend.Controllers
{
    [ApiController]
    [Route("api/types")]
    public class CardTypeController(ICardTypeService cardTypeService, IOptions<FileUploadParameters> fileUploadOptions, IFileUploadValidationService fileUploadValidationService) : ControllerBase
    {
        private readonly ICardTypeService mCardTypeService = cardTypeService;
        private readonly FileUploadParameters mFileUploadParams = fileUploadOptions.Value;
        private readonly IFileUploadValidationService mFileUploadValidationService = fileUploadValidationService;

        [HttpGet]
        [ProducesResponseType<IEnumerable<CardTypeDTO>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCardTypes()
        {
            var cardTypes = await mCardTypeService.GetAllCardTypes();
            return Ok(cardTypes.Select(type => type.GetDTO()));
        }

        [HttpPost]
        [ProducesResponseType<CardTypeDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCardType([FromBody] CardTypeCreationDTO cardTypeDTO)
        {
            var cardTypeCreateData = cardTypeDTO.ToCreationEntity();
            var insertedCardType = await mCardTypeService.CreateCardType(cardTypeCreateData);

            return Ok(insertedCardType.GetDTO());
        }

        [HttpPatch("{id}")]
        [ProducesResponseType<CardTypeDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> UpdateCardType(Guid id, [FromBody] CardTypeUpdateDTO updateDTO)
        {
            var updatedType = await mCardTypeService.UpdateCardTypeWithId(id, updateDTO);
            return Ok(updatedType.GetDTO());
        }

        [HttpPut("{id}/image")]
        [ProducesResponseType<CardTypeDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCardTypeImage(Guid id, IFormFile imageFile)
        {
            var validationResult = await mFileUploadValidationService.ValidateFileUpload(this, imageFile);

            if(validationResult is not null)
            {
                return validationResult;
            }

            var fileName = imageFile.FileName;
            var mimeType = MimeTypesMap.GetMimeType(fileName);

            if(mimeType is null || !mimeType.StartsWith("image"))
            {
                return BadRequest("Uploaded file is not an image");
            }

            using var stream = new MemoryStream();
            await imageFile.CopyToAsync(stream);

            stream.Position = 0;
            var fileData = stream.ToArray();

            var updatedType = await mCardTypeService.UpdateCardTypeImage(id, $"{Guid.NewGuid()}.{MimeTypesMap.GetExtension(mimeType)}", fileData);

            return Ok(updatedType.GetDTO());
        }
    }
}
