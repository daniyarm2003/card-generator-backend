using CardGeneratorBackend.CardGeneration;
using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Environment;
using CardGeneratorBackend.Services;
using HeyRed.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CardGeneratorBackend.Controllers
{
    [ApiController]
    [Route("/api/cards")]
    public class CardController(ICardService cardService, IFileUploadValidationService fileUploadValidationService) : ControllerBase
    {
        private readonly ICardService mCardService = cardService;
        private readonly IFileUploadValidationService mFileUploadValidationService = fileUploadValidationService;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCards(int? pageNum, int? pageSize)
        {
            if (pageSize is null)
            {
                var cards = await mCardService.GetAllCards();
                return Ok(cards.Select(card => card.GetDTO()));
            }
            else
            {
                pageNum ??= 1;

                var data = await mCardService.GetCardsPaginated((int)pageNum, (int)pageSize);
                return Ok(data.Select(card => card.GetDTO()));
            }
        }

        [HttpPost]
        [ProducesResponseType<CardDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCard([FromBody] CardCreationDTO dto)
        {
            var createdCard = await mCardService.CreateCard(dto);
            return Ok(createdCard.GetDTO());
        }

        [HttpPatch("{id}")]
        [ProducesResponseType<CardDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCard(Guid id, [FromBody] CardUpdateDTO dto)
        {
            var updatedCard = await mCardService.UpdateCardWithId(id, dto);
            return Ok(updatedCard.GetDTO());
        }

        [HttpPut("{id}/image")]
        [ProducesResponseType<CardDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCardDisplayImage(Guid id, IFormFile imageFile)
        {
            var validationResult = await mFileUploadValidationService.ValidateFileUpload(this, imageFile);

            if (validationResult is not null)
            {
                return validationResult;
            }

            var fileName = imageFile.FileName;
            var mimeType = MimeTypesMap.GetMimeType(fileName);

            if (mimeType is null || !mimeType.StartsWith("image"))
            {
                return BadRequest("Uploaded file is not an image");
            }

            using var stream = new MemoryStream();
            await imageFile.CopyToAsync(stream);

            stream.Position = 0;
            var fileData = stream.ToArray();

            var updatedCard = await mCardService.UpdateCardDisplayImage(id, $"{Guid.NewGuid()}.{MimeTypesMap.GetExtension(mimeType)}", fileData);

            return Ok(updatedCard.GetDTO());
        }

        [HttpPatch("{id}/card-image/update")]
        [ProducesResponseType<CardDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCardImage(Guid id)
        {
            var updatedCard = await mCardService.GenerateAndUpdateCardImage(id, $"CardImage-{Guid.NewGuid()}.png");
            return Ok(updatedCard.GetDTO());
        }

        [HttpDelete("{id}")]
        [ProducesResponseType<CardDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCard(Guid id)
        {
            var cardToDelete = await mCardService.GetCardById(id);
            await mCardService.DeleteCard(cardToDelete);

            return Ok(cardToDelete);
        }
    }
}
