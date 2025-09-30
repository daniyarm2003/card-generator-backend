using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Entities;
using CardGeneratorBackend.Services;
using SkiaSharp;
using System.Drawing;

namespace CardGeneratorBackend.CardGeneration
{
    public abstract class BaseCardImageGenerator(ITrackedFileService trackedFileService) : ICardImageGenerator
    {
        protected const float TOP_TEXT_Y = 50.0f;
        protected const float BOTTOM_TEXT_Y_OFFSET = 15.0f;

        protected readonly ITrackedFileService mTrackedFileService = trackedFileService;

        protected SKShader GetBackgroundGradientShader(SKColor innerColor, SKColor outerColor, int width, int height)
        {
            return SKShader.CreateRadialGradient(
                new SKPoint(width / 2, height / 2),
                width * 0.75f,
                [innerColor, outerColor],
                [0.0f, 1.0f],
                SKShaderTileMode.Clamp
            );
        }

        protected SKRect GetCardBoundaryRect(int width, int height)
        {
            return new SKRect(0.0f, 0.0f, width, height);
        }

        protected SKRect GetCardImageRect(int width, int height)
        {
            float cardImagePaddingX = 10.0f;
            float cardImagePosY = TOP_TEXT_Y + 20.0f;

            float cardImageWidth = width - 2.0f * cardImagePaddingX;
            float cardImageHeight = height / 3.0f;

            return new SKRect(cardImagePaddingX, cardImagePosY, cardImagePaddingX + cardImageWidth, cardImagePosY + cardImageHeight);
        }

        protected void DrawCardName(CardDTO card, SKCanvas canvas, SKPaint textPaint, int width)
        {
            using var nameFont = CardImageUtils.CreateBasicFontFromFile("Assets/Kalam/Kalam-Bold.ttf", 35);

            float maxNameWidth = width * 0.5f;

            while (nameFont.MeasureText(card.Name, textPaint) > maxNameWidth)
            {
                nameFont.Size -= 1.0f;
            }

            canvas.DrawText(card.Name, width / 2.0f, TOP_TEXT_Y, SKTextAlign.Center, nameFont, textPaint);
        }

        protected void DrawCardLevel(CardDTO card, SKCanvas canvas, SKPaint textPaint)
        {
            if(card.Level is null)
            {
                return;
            }

            string cardLevelText = $"Level {card.Level}";
            float levelTextX = 15.0f;

            using var levelFont = CardImageUtils.CreateBasicFontFromFile("Assets/Changa/Changa-VariableFont_wght.ttf", 25);

            canvas.DrawText(cardLevelText, levelTextX, TOP_TEXT_Y, levelFont, textPaint);
        }

        protected async Task DrawCardTypeInfo(CardDTO card, SKCanvas canvas, SKPaint textPaint, int width)
        {
            if(card.Type.Id == new Guid(CardType.NONE_TYPE_UUID))
            {
                return;
            }

            float typeImageSize = 40.0f;
            float typeImageRightMargin = 15.0f;
            float typeTextYOffset = 15.0f;
            float typeTextWidth = typeImageSize * 1.2f;

            var typeImageRect = new SKRect(width - typeImageSize - typeImageRightMargin, TOP_TEXT_Y - typeImageSize, width - typeImageRightMargin, TOP_TEXT_Y);

            if (card.Type.ImageFileId is not null)
            {
                var typeImageInfo = await mTrackedFileService.ReadFileWithId((Guid)card.Type.ImageFileId);
                byte[] typeImageData = typeImageInfo.Contents;

                using var typeImage = SKImage.FromEncodedData(typeImageData);

                canvas.DrawImage(typeImage, typeImageRect);
            }

            using var typeFont = CardImageUtils.CreateBasicFontFromFile("Assets/Aldrich/Aldrich-Regular.ttf", 15);

            while (typeFont.MeasureText(card.Type.Name) > typeTextWidth)
            {
                typeFont.Size -= 1.0f;
            }

            canvas.DrawText(card.Type.Name, width - typeImageRightMargin - typeImageSize / 2.0f, TOP_TEXT_Y + typeTextYOffset, SKTextAlign.Center, typeFont, textPaint);
        }

        protected async Task DrawCardDisplayImage(CardDTO card, SKCanvas canvas, SKPaint outlinePaint, int width, int height)
        {
            if(card.DisplayImageId is null)
            {
                return;
            }

            var cardImageRect = GetCardImageRect(width, height);

            var imageInfo = await mTrackedFileService.ReadFileWithId((Guid)card.DisplayImageId);
            byte[] imageData = imageInfo.Contents;

            using var image = SKImage.FromEncodedData(imageData);

            canvas.DrawImage(image, cardImageRect);
            canvas.DrawRect(cardImageRect, outlinePaint);
        }

        protected void DrawCardQuote(CardDTO card, SKCanvas canvas, SKPaint textPaint, int width, float cardImageWidth, ref float y)
        {
            if(string.IsNullOrWhiteSpace(card.Quote))
            {
                return;
            }

            float quoteTextPadding = 5.0f;
            string quoteText = $"\"{card.Quote}\"";
            float quoteMaxWidth = cardImageWidth - 2.0f * quoteTextPadding;

            using var quoteFont = CardImageUtils.CreateBasicFontFromFile("Assets/Courgette/Courgette-Regular.ttf", 20);

            canvas.WriteMultiLineCenteredText(quoteText, quoteFont, textPaint, width / 2, quoteMaxWidth, ref y);
        }

        protected SKRect GetEffectBoxRect(int width, int height, float y)
        {
            var cardImageRect = GetCardImageRect(width, height);
            float bottomSectionHeight = height * 0.1f;

            return new SKRect(cardImageRect.Left, y, cardImageRect.Right, height - bottomSectionHeight);
        }

        protected void DrawCardEffect(CardDTO card, SKCanvas canvas, SKPaint outlinePaint, SKPaint textPaint, int width, int height, ref float y)
        {
            if(string.IsNullOrWhiteSpace(card.Effect))
            {
                return;
            }

            using var effectFont = CardImageUtils.CreateBasicFontFromFile("Assets/Righteous/Righteous-Regular.ttf", 20);
            var effectBoxRect = GetEffectBoxRect(width, height, y);

            canvas.DrawRect(effectBoxRect, outlinePaint);

            y += effectFont.GetFontLineHeight();

            canvas.WriteMultiLineCenteredText(card.Effect, effectFont, textPaint, width / 2, effectBoxRect.Width - 20.0f, ref y);
        }

        protected void DrawCardNumber(CardDTO card, SKCanvas canvas, SKPaint textPaint, int width, int height)
        {
            using var numberFont = CardImageUtils.CreateBasicFontFromFile("Assets/Press_Start_2P/PressStart2P-Regular.ttf", 25);
            float bottomTextBaselineY = height - BOTTOM_TEXT_Y_OFFSET;

            canvas.DrawText(card.Number.ToString(), width / 2.0f, bottomTextBaselineY, SKTextAlign.Center, numberFont, textPaint);
        }

        protected void DrawCardBaseStats(CardDTO card, SKCanvas canvas, SKPaint textPaint, int width, int height)
        {
            using var statFont = CardImageUtils.CreateBasicFontFromFile("Assets/Press_Start_2P/PressStart2P-Regular.ttf", 20);

            float bottomTextBaselineY = height - BOTTOM_TEXT_Y_OFFSET;
            float textMargin = 15.0f;

            if (card.Health is not null)
            {
                string hpText = $"HP {card.Health}";
                var hpLocation = new SKPoint(textMargin, bottomTextBaselineY);

                canvas.DrawText(hpText, hpLocation, SKTextAlign.Left, statFont, textPaint);
            }

            if (card.Attack is not null)
            {
                string attackText = $"ATK {card.Attack}";
                var attackLocation = new SKPoint(width - textMargin, bottomTextBaselineY);

                canvas.DrawText(attackText, attackLocation, SKTextAlign.Right, statFont, textPaint);
            }
        }

        public abstract Task<SKCanvas> GenerateCardImage(CardDTO card, SKCanvas cardCanvas, int width, int height);
    }
}
