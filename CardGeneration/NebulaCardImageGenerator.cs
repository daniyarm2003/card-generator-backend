using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Services;

using SkiaSharp;

namespace CardGeneratorBackend.CardGeneration
{
    class NebulaCardImageGenerator(ITrackedFileService trackedFileService) : ICardImageGenerator
    {
        private static readonly SKColor NEBULA_BACKGROUND_COLOR_INNER = new SKColor(0xEF, 0x00, 0xEF);
        private static readonly SKColor NEBULA_BACKGROUND_COLOR_OUTER = new SKColor(0xC5, 0x00, 0xC5);
        private static readonly SKColor NEBULA_TEXT_COLOR = new SKColor(0xFF, 0xFF, 0xFF);

        private readonly ITrackedFileService mTrackedFileService = trackedFileService;

        public async Task<SKCanvas> GenerateCardImage(CardDTO card, SKCanvas cardCanvas, int width, int height)
        {
            ArgumentNullException.ThrowIfNull(card);
            ArgumentNullException.ThrowIfNullOrEmpty(card.Name);
            ArgumentNullException.ThrowIfNull(card.Number);

            using var radialShader = SKShader.CreateRadialGradient(
                new SKPoint(width / 2, height / 2),
                width * 0.75f,
                [NEBULA_BACKGROUND_COLOR_INNER, NEBULA_BACKGROUND_COLOR_OUTER],
                [0.0f, 1.0f],
                SKShaderTileMode.Clamp
            );

            using var bgPaint = new SKPaint
            {
                Shader = radialShader,
                IsAntialias = true
            };

            using var outlinePaint = new SKPaint
            {
                StrokeWidth = 2.0f,
                Color = NEBULA_TEXT_COLOR,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            var bgRect = new SKRect(0.0f, 0.0f, width, height);

            cardCanvas.DrawRect(bgRect, bgPaint);
            cardCanvas.DrawRect(bgRect, outlinePaint);

            using var nameFont = new SKFont
            {
                Typeface = SKTypeface.FromFile("Assets/Kalam/Kalam-Bold.ttf"),
                Size = 35
            };

            using var textPaint = new SKPaint
            {
                Color = NEBULA_TEXT_COLOR,
                IsAntialias = true
            };
            
            float maxNameWidth = width * 0.5f;

            while (nameFont.MeasureText(card.Name, textPaint) > maxNameWidth)
            {
                nameFont.Size -= 1.0f;
            }

            float topTextY = 50.0f;

            var nameLocation = new SKPoint(width / 2, topTextY);

            cardCanvas.DrawText(card.Name, nameLocation, SKTextAlign.Center, nameFont, textPaint);

            if(card.Type.Id != new Guid(CardType.NONE_TYPE_UUID))
            {
                float typeImageSize = 40.0f;
                float typeImageRightMargin = 15.0f;

                var typeImageRect = new SKRect(width - typeImageSize - typeImageRightMargin, topTextY - typeImageSize, width - typeImageRightMargin, topTextY);

                if(card.Type.ImageFileId is not null)
                {
                    var typeImageInfo = await mTrackedFileService.ReadFileWithId((Guid)card.Type.ImageFileId);
                    byte[] typeImageData = typeImageInfo.Contents;

                    using var typeImage = SKImage.FromEncodedData(typeImageData);

                    cardCanvas.DrawImage(typeImage, typeImageRect);
                }

                using var typeFont = new SKFont
                {
                    Typeface = SKTypeface.FromFile("Assets/Aldrich/Aldrich-Regular.ttf"),
                    Size = 15
                };

                while(typeFont.MeasureText(card.Type.Name) > typeImageSize * 1.2f)
                {
                    typeFont.Size -= 1.0f;
                }

                var typeTextLocation = new SKPoint(width - typeImageRightMargin - typeImageSize / 2, topTextY + 15);

                cardCanvas.DrawText(card.Type.Name, typeTextLocation, SKTextAlign.Center, typeFont, textPaint);
            }

            float cardImagePaddingX = 10.0f;
            float cardImagePosY = topTextY + 20.0f;
            
            float cardImageWidth = width - 2 * cardImagePaddingX;
            float cardImageHeight = height / 3.0f;

            var cardImageRect = new SKRect(cardImagePaddingX, cardImagePosY, cardImagePaddingX + cardImageWidth, cardImagePosY + cardImageHeight);

            if(card.DisplayImageId is not null)
            {
                var imageInfo = await mTrackedFileService.ReadFileWithId((Guid)card.DisplayImageId);
                byte[] imageData = imageInfo.Contents;

                using var image = SKImage.FromEncodedData(imageData);

                cardCanvas.DrawImage(image, cardImageRect);
            }

            cardCanvas.DrawRect(cardImageRect, outlinePaint);

            float textLineOffsetY = cardImageRect.Bottom + 30.0f;

            if(card.Quote is not null && card.Quote.Trim().Length != 0)
            {
                string quoteText = $"\"{card.Quote}\"";
                float quoteMaxWidth = cardImageRect.Width - 10.0f;

                using var quoteFont = new SKFont
                {
                    Typeface = SKTypeface.FromFile("Assets/Courgette/Courgette-Regular.ttf"),
                    Size = 20
                };

                cardCanvas.WriteMultiLineCenteredText(quoteText, quoteFont, textPaint, width / 2, quoteMaxWidth, ref textLineOffsetY);
            }

            float bottomSectionHeight = height * 0.1f;

            if(!string.IsNullOrWhiteSpace(card.Effect))
            {
                var effectBoxRect = new SKRect(cardImageRect.Left, textLineOffsetY, cardImageRect.Right, height - bottomSectionHeight);

                outlinePaint.StrokeWidth = 1.0f;
                cardCanvas.DrawRect(effectBoxRect, outlinePaint);

                using var effectFont = new SKFont
                {
                    Typeface = SKTypeface.FromFile("Assets/Righteous/Righteous-Regular.ttf"),
                    Size = 20
                };

                textLineOffsetY += effectFont.GetFontLineHeight();
                
                cardCanvas.WriteMultiLineCenteredText(card.Effect, effectFont, textPaint, width / 2, effectBoxRect.Width - 20.0f, ref textLineOffsetY);
            }

            using var numberFont = new SKFont
            {
                Typeface = SKTypeface.FromFile("Assets/Press_Start_2P/PressStart2P-Regular.ttf"),
                Size = 25
            };

            float bottomTextBaselineY = height - 15.0f;

            var numberLocation = new SKPoint(width / 2, bottomTextBaselineY);
            cardCanvas.DrawText(card.Number.ToString(), numberLocation, SKTextAlign.Center, numberFont, textPaint);

            return cardCanvas;
        }
    }
}