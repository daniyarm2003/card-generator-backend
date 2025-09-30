using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Services;
using SkiaSharp;
using System.Drawing;

namespace CardGeneratorBackend.CardGeneration
{
    public class RegularCardImageGenerator(ITrackedFileService trackedFileService) : BaseCardImageGenerator(trackedFileService)
    {
        public override async Task<SKCanvas> GenerateCardImage(CardDTO card, SKCanvas cardCanvas, int width, int height)
        {
            ArgumentNullException.ThrowIfNull(card);
            ArgumentException.ThrowIfNullOrEmpty(card.Name);
            ArgumentNullException.ThrowIfNull(card.Number);

            if (!SKColor.TryParse($"#{card.Type.BackgroundColorHexCode1}", out SKColor bgCol1))
            {
                bgCol1 = SKColors.White;
            }

            if (!SKColor.TryParse($"#{card.Type.BackgroundColorHexCode2}", out SKColor bgCol2))
            {
                bgCol2 = SKColors.White;
            }

            if (!SKColor.TryParse($"#{card.Type.TextColor}", out SKColor textCol))
            {
                textCol = SKColors.Black;
            }

            using var radialShader = GetBackgroundGradientShader(bgCol1, bgCol2, width, height);

            using var bgPaint = new SKPaint
            {
                Shader = radialShader,
                IsAntialias = true
            };

            using var outlinePaint = new SKPaint
            {
                StrokeWidth = 2.0f,
                Color = textCol,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            using var effectOutlinePaint = outlinePaint.Clone();
            effectOutlinePaint.StrokeWidth = 1.0f;

            var bgRect = GetCardBoundaryRect(width, height);

            cardCanvas.DrawRect(bgRect, bgPaint);
            cardCanvas.DrawRect(bgRect, outlinePaint);

            using var textPaint = new SKPaint
            {
                Color = textCol,
                IsAntialias = true
            };

            DrawCardName(card, cardCanvas, textPaint, width);
            DrawCardLevel(card, cardCanvas, textPaint);

            await DrawCardTypeInfo(card, cardCanvas, textPaint, width);
            await DrawCardDisplayImage(card, cardCanvas, outlinePaint, width, height);

            var cardImageRect = GetCardImageRect(width, height);
            float textLineOffsetY = cardImageRect.Bottom + 30.0f;

            DrawCardQuote(card, cardCanvas, textPaint, width, cardImageRect.Width, ref textLineOffsetY);
            DrawCardEffect(card, cardCanvas, effectOutlinePaint, textPaint, width, height, ref textLineOffsetY);

            DrawCardNumber(card, cardCanvas, textPaint, width, height);
            DrawCardBaseStats(card, cardCanvas, textPaint, width, height);

            return cardCanvas;
        }
    }
}