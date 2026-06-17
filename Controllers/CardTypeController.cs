using CardGeneratorBackend.DTO;
using CardGeneratorBackend.DTO.Mappers;
using CardGeneratorBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CardGeneratorBackend.Controllers
{
    [ApiController]
    [Route("api/types")]
    public class CardTypeController(ICardTypeService cardTypeService, CardTypeDTOMapper cardTypeDTOMapper) : ControllerBase
    {
        private readonly ICardTypeService mCardTypeService = cardTypeService;
        private readonly CardTypeDTOMapper mCardTypeDTOMapper = cardTypeDTOMapper;

        [HttpGet]
        [ProducesResponseType<IEnumerable<CardTypeDTO>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCardTypes()
        {
            var cardTypes = await mCardTypeService.GetAllCardTypes();
            return Ok(cardTypes.Select(mCardTypeDTOMapper.ToDTO));
        }

        [HttpGet("{id}")]
        [ProducesResponseType<CardTypeDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCardTypeById(Guid id)
        {
            var cardType = await mCardTypeService.GetCardTypeById(id);
            return Ok(mCardTypeDTOMapper.ToDTO(cardType));
        }

        [HttpPost]
        [ProducesResponseType<CardTypeDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCardType([FromBody] CardTypeCreationDTO cardTypeDTO)
        {
            var cardTypeCreateData = mCardTypeDTOMapper.ToCreationEntity(cardTypeDTO);
            var insertedCardType = await mCardTypeService.CreateCardType(cardTypeCreateData);

            return Ok(mCardTypeDTOMapper.ToDTO(insertedCardType));
        }

        [HttpPost("{id}/image-upload-url")]
        [ProducesResponseType<UploadURLResponseDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCardTypeImageUploadURL(Guid id, [FromBody] UploadURLRequestDTO uploadURLRequest)
        {
            var uploadURL = await mCardTypeService.CreateCardTypeImageUploadURL(id, uploadURLRequest.FileName);
            return Ok(uploadURL);
        }


        [HttpPatch("{id}")]
        [ProducesResponseType<CardTypeDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> UpdateCardType(Guid id, [FromBody] CardTypeUpdateDTO updateDTO)
        {
            var updatedType = await mCardTypeService.UpdateCardTypeWithId(id, updateDTO);
            return Ok(mCardTypeDTOMapper.ToDTO(updatedType));
        }
    }
}
