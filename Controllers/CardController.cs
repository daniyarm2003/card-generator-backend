using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Environment;
using CardGeneratorBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CardGeneratorBackend.Controllers
{
    [ApiController]
    [Route("/api/cards")]
    public class CardController(ICardService cardService, IOptions<FileUploadParameters> fileUploadOptions) : ControllerBase
    {
        private readonly ICardService mCardService = cardService;
        private readonly FileUploadParameters mFileUploadParams = fileUploadOptions.Value;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCards(int? pageNum, int? pageSize)
        {
            if(pageSize is null)
            {
                var cards = await mCardService.GetAllCards();
                return Ok(cards.Select(card => card.GetDTO()));
            }
            else
            {
                pageNum = pageNum ?? 1;

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
    }
}
