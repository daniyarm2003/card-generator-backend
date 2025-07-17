using CardGeneratorBackend.DTO;
using SkiaSharp;

namespace CardGeneratorBackend.CardGeneration
{
    public interface ICardImageGenerator
    {
        public Task<SKCanvas> GenerateCardImage(CardDTO card, SKCanvas cardCanvas, int width, int height);
    }
}
