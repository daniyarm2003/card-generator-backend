﻿using CardGeneratorBackend.DTO;
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
    public class CardTypeController(ICardTypeService cardTypeService, ILogger<CardTypeController> logger, IOptions<FileUploadParameters> fileUploadOptions) : ControllerBase
    {
        private readonly ICardTypeService mCardTypeService = cardTypeService;
        private readonly ILogger<CardTypeController> mLogger = logger;
        private readonly FileUploadParameters mFileUploadParams = fileUploadOptions.Value;

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
            if(imageFile is null)
            {
                return BadRequest("Image file was not provided");
            }
            else if(imageFile.Length > mFileUploadParams.MaxFileSizeBytesParsed)
            {
                return BadRequest($"Image file's size in bytes is greater than the maximum allowed size of {mFileUploadParams.MaxFileSizeBytesParsed}");
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
