using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Services;
using SkiaSharp;
using System.Drawing;

namespace CardGeneratorBackend.CardGeneration
{
    public class BaseCardImageGenerator(ITrackedFileService trackedFileService) : ICardImageGenerator
    {
        private readonly ITrackedFileService mTrackedFileService = trackedFileService;

        public async Task<SKCanvas> GenerateCardImage(CardDTO card, SKCanvas cardCanvas, int width, int height)
        {
            ArgumentNullException.ThrowIfNull(card);
            ArgumentNullException.ThrowIfNullOrEmpty(card.Name);
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

            using var radialShader = SKShader.CreateRadialGradient(
                new SKPoint(width / 2, height / 2),
                width * 0.75f,
                [bgCol1, bgCol2],
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
                Color = textCol,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            var bgRect = new SKRect(0.0f, 0.0f, width, height);
            // var bgRoundRect = new SKRoundRect(bgRect, 10.0f);

            cardCanvas.DrawRect(bgRect, bgPaint);
            cardCanvas.DrawRect(bgRect, outlinePaint);

            using var nameFont = new SKFont
            {
                Typeface = SKTypeface.FromFile("Assets/Kalam/Kalam-Bold.ttf"),
                Size = 35
            };

            using var textPaint = new SKPaint
            {
                Color = textCol,
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

            if(card.Level is not null)
            {
                string cardLevelText = $"Level {card.Level}";

                using var levelFont = new SKFont
                {
                    Typeface = SKTypeface.FromFile("Assets/Changa/Changa-VariableFont_wght.ttf"),
                    Size = 25
                };

                var levelLocation = new SKPoint(15, topTextY);

                cardCanvas.DrawText(cardLevelText, levelLocation, SKTextAlign.Left, levelFont, textPaint);
            }

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

                var quoteFontMetrics = quoteFont.Metrics;

                while (quoteText.Length > 0)
                {
                    int bytesRead = quoteFont.BreakText(quoteText, quoteMaxWidth, textPaint);

                    string quoteSection = quoteText[..bytesRead];
                    quoteText = quoteText[bytesRead..];

                    var quoteTextLocation = new SKPoint(width / 2, textLineOffsetY);

                    textLineOffsetY += quoteFontMetrics.Descent - quoteFontMetrics.Ascent + quoteFontMetrics.Leading;

                    cardCanvas.DrawText(quoteSection, quoteTextLocation, SKTextAlign.Center, quoteFont, textPaint);
                }
            }

            float bottomSectionHeight = height * 0.1f;

            if(!string.IsNullOrWhiteSpace(card.Effect))
            {
                var effectBoxRect = new SKRect(cardImageRect.Left, textLineOffsetY, cardImageRect.Right, height - bottomSectionHeight);

                outlinePaint.StrokeWidth = 1.0f;
                cardCanvas.DrawRect(effectBoxRect, outlinePaint);

                string[] effectLines = card.Effect.Split('\n');

                using var effectFont = new SKFont
                {
                    Typeface = SKTypeface.FromFile("Assets/Righteous/Righteous-Regular.ttf"),
                    Size = 20
                };

                var effectFontMetrics = effectFont.Metrics;

                foreach(var line in effectLines)
                {
                    string lineString = line;
                    float maxEffectWidth = effectBoxRect.Width - 20.0f;

                    if(lineString.Length == 0)
                    {
                        textLineOffsetY += effectFontMetrics.Descent - effectFontMetrics.Ascent + effectFontMetrics.Leading;
                    }

                    while(lineString.Length > 0)
                    {
                        int bytesRead = effectFont.BreakText(lineString, maxEffectWidth, textPaint);
                        int spaceIndex = lineString[..bytesRead].LastIndexOf(' ');

                        if (spaceIndex >= 0 && bytesRead < lineString.Length)
                        {
                            bytesRead = spaceIndex + 1;
                        }

                        string effectSection = lineString[..bytesRead];
                        lineString = lineString[bytesRead..];

                        textLineOffsetY += effectFontMetrics.Descent - effectFontMetrics.Ascent + effectFontMetrics.Leading;
                        var effectTextLocation = new SKPoint(width / 2, textLineOffsetY);

                        cardCanvas.DrawText(effectSection, effectTextLocation, SKTextAlign.Center, effectFont, textPaint);
                    }
                }
            }

            using var numberFont = new SKFont
            {
                Typeface = SKTypeface.FromFile("Assets/Press_Start_2P/PressStart2P-Regular.ttf"),
                Size = 25
            };

            using var statFont = new SKFont
            {
                Typeface = SKTypeface.FromFile("Assets/Press_Start_2P/PressStart2P-Regular.ttf"),
                Size = 20
            };

            float bottomTextBaselineY = height - 15.0f;

            var numberLocation = new SKPoint(width / 2, bottomTextBaselineY);
            cardCanvas.DrawText(card.Number.ToString(), numberLocation, SKTextAlign.Center, numberFont, textPaint);

            if(card.Health is not null)
            {
                string hpText = $"HP {card.Health}";
                var hpLocation = new SKPoint(15.0f, bottomTextBaselineY);

                cardCanvas.DrawText(hpText, hpLocation, SKTextAlign.Left, statFont, textPaint);
            }

            if (card.Attack is not null)
            {
                string attackText = $"ATK {card.Attack}";
                var attackLocation = new SKPoint(width - 15.0f, bottomTextBaselineY);

                cardCanvas.DrawText(attackText, attackLocation, SKTextAlign.Right, statFont, textPaint);
            }

            return cardCanvas;
        }
    }
}
