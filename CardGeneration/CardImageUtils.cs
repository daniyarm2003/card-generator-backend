using SkiaSharp;

namespace CardGeneratorBackend.CardGeneration
{
    public static class CardImageUtils
    {
        public static float GetFontLineHeight(this SKFont font)
        {
            var fontMetrics = font.Metrics;
            return fontMetrics.Descent - fontMetrics.Ascent + fontMetrics.Leading;
        }

        public static SKFont CreateBasicFontFromFile(string fontPath, float fontSize)
        {
            return new SKFont
            {
                Typeface = SKTypeface.FromFile(fontPath),
                Size = fontSize
            };
        }

        public static void WriteMultiLineCenteredText(this SKCanvas canvas, string text, SKFont font, SKPaint paint, float centerX, float maxWidth, ref float y)
        {
            var textLines = text.Split('\n');
            float textYOffset = font.GetFontLineHeight();

            foreach (var line in textLines)
            {
                string remainingText = line;

                if (remainingText.Length == 0)
                {
                    y += textYOffset;
                    continue;
                }

                while (remainingText.Length > 0)
                {
                    int bytesRead = font.BreakText(remainingText, maxWidth, paint);
                    int spaceIndex = remainingText[..bytesRead].LastIndexOf(' ');

                    // If there is a space before the end of the line, break at that space
                    if (spaceIndex >= 0 && bytesRead < remainingText.Length)
                    {
                        bytesRead = spaceIndex + 1;
                    }

                    string textSection = remainingText[..bytesRead];
                    remainingText = remainingText[bytesRead..];

                    canvas.DrawText(textSection, centerX, y, SKTextAlign.Center, font, paint);
                    y += textYOffset;
                }
            }
        }
    }
}