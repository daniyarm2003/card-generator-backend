using CardGeneratorBackend.CardGeneration;
using CardGeneratorBackend.DTO;
using CardGeneratorBackend.DTO.Mappers;
using CardGeneratorBackend.Environment;
using CardGeneratorBackend.Services;
using HeyRed.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CardGeneratorBackend.Controllers
{
    [ApiController]
    [Route("/api/cards")]
    public class CardController(ICardService cardService, IFileUploadValidationService fileUploadValidationService, CardDTOMapper cardDTOMapper) : ControllerBase
    {
        private readonly ICardService mCardService = cardService;
        private readonly IFileUploadValidationService mFileUploadValidationService = fileUploadValidationService;
        private readonly CardDTOMapper mCardDTOMapper = cardDTOMapper;

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
                return Ok(cards.Select(mCardDTOMapper.ToDTO));
            }
            else
            {
                pageNum ??= 1;

                var data = await mCardService.GetCardsPaginated((int)pageNum, (int)pageSize);
                return Ok(data.Select(card => mCardDTOMapper.ToDTO(card)));
            }
        }

        [HttpPost]
        [ProducesResponseType<CardDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCard([FromBody] CardCreationDTO dto)
        {
            var createdCard = await mCardService.CreateCard(dto);
            return Ok(mCardDTOMapper.ToDTO(createdCard));
        }

        [HttpPatch("{id}")]
        [ProducesResponseType<CardDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCard(Guid id, [FromBody] CardUpdateDTO dto)
        {
            var updatedCard = await mCardService.UpdateCardWithId(id, dto);
            return Ok(mCardDTOMapper.ToDTO(updatedCard));
        }

        [HttpPost("{id}/display-image/upload-url")]
        [ProducesResponseType<UploadURLResponseDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCardDisplayImageUploadURL(Guid id, [FromBody] UploadURLRequestDTO uploadURLRequest)
        {
            var uploadURLResponse = await mCardService.CreateCardDisplayImageUploadURL(id, uploadURLRequest.FileName);
            return Ok(uploadURLResponse);
        }

        [HttpPost("{id}/card-image/update")]
        [ProducesResponseType<CardDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCardImage(Guid id)
        {
            var updatedCard = await mCardService.GenerateAndUpdateCardImage(id);
            return Ok(mCardDTOMapper.ToDTO(updatedCard));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType<CardDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCard(Guid id)
        {
            var cardToDelete = await mCardService.GetCardById(id);
            await mCardService.DeleteCard(cardToDelete);

            return Ok(mCardDTOMapper.ToDTO(cardToDelete));
        }
    }
}
