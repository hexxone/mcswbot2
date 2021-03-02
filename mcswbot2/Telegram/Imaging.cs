using System;
using SkiaSharp;

namespace mcswbot2.Telegram
{
    internal static class Imaging
    {
        /// <summary>
        ///     Wrapper for scaling and writing text to an image
        /// </summary>
        /// <param name="input"></param>
        /// <param name="txt"></param>
        /// <returns></returns>
        internal static SKImage MakeSticker(SKImage input, string txt)
        {
            using var scaled = MakeThumbnail(input);
            return WriteText(scaled, Utils.NoHtml(txt));
        }

        /// <summary>
        ///     Writes given text centered on the image
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="txt"></param>
        /// <returns></returns>
        private static SKImage WriteText(SKImage bmp, string txt)
        {
            // Create graphic object that will draw onto the bitmap
            using var blr = BlurInternal(bmp);
            using var g = SKSurface.Create(new SKImageInfo(blr.Width, blr.Height));
            var canvas = g.Canvas;
            // copy blurred iamge
            canvas.DrawImage(blr, 0, 0);

            // Process all lines
            var Lines = txt.Split("\r\n");
            float LineHeight = blr.Height / Lines.Length;
            for (var ln = 0; ln < Lines.Length; ln++)
            {
                // get & check line
                var line = Lines[ln];
                if(string.IsNullOrWhiteSpace(line)) continue;

               // Calculate & draw text
                var fSize = LineHeight * 0.4f;
                var lineStart = ln * LineHeight;
                CanvasDrawText(canvas, line, fSize, new SKRect(0, lineStart, blr.Width, lineStart + LineHeight));
            }

            // Done boii
            canvas.Flush();
            var result = g.Snapshot();
            g.Dispose();
            return result;
        }

        // Draw centered text
        private static void CanvasDrawText(SKCanvas cvs, string txt, float fSize, SKRect pos, int shadow = 3)
        {
            var scale = 0.95f;

            // main texture
            using var textPain = new SKPaint()
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                TextSize = fSize,
                TextAlign = SKTextAlign.Center,
                Color = SKColors.White,
            };
            // shadow texture, used to measure text scale because its slightly bigger.
            using var shadowPain = new SKPaint()
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                TextSize = fSize,
                TextAlign = SKTextAlign.Center,
                Color = SKColors.Black,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Solid, 5),
                BlendMode = SKBlendMode.HardLight
            };


            // bounding box stuff
            var textBounds = new SKRect();
            shadowPain.MeasureText(txt, ref textBounds);
            // adaptive downscaling
            if (textBounds.Width > pos.Width)
            {
                scale *= (pos.Width / textBounds.Width);
                fSize *= scale;
            }


            // text position
            var xPos = (pos.Width / 2) - textBounds.MidX * scale;
            var yPos = pos.Top + (pos.Height / 2) - textBounds.MidY;

            // font stuffs
            var ff = Fonts.GetCustomFont();
            var fa = SKTypeface.FromFamilyName(ff.Name);
            using var fo = new SKFont(fa, fSize);
            using var te = SKTextBlob.Create(txt, fo);

            // draw shadow
            if (shadow > 0)
            {
                for (var i = 0; i < shadow; i++)
                    cvs.DrawText(te, xPos, yPos, shadowPain);

                cvs.Flush();
            }

            // draw text #NoFilter
            cvs.DrawText(te, xPos, yPos, textPain);
            cvs.Flush();
        }

        /// <summary>
        ///     Auto-Scale and fit images to Thumbnails of size 512x512
        /// </summary>
        /// <param name="rawImage"></param>
        /// <returns></returns>
        private static SKImage MakeThumbnail(SKImage rawImage, int size = 512)
        {
            // calculate crop
            var minSide = Math.Min(rawImage.Width, rawImage.Height);
            int off_x = 0, off_y = 0;
            if (minSide < rawImage.Width) off_x = (rawImage.Width - minSide) / 2;
            if (minSide < rawImage.Height) off_y = (rawImage.Height - minSide) / 2;
            // make new bitmap


            var info = new SKImageInfo(size, size);
            SKImage result = null;
            using (var surface = SKSurface.Create(info))
            {
                var graph = surface.Canvas;

                // scale fill cropped image
                graph.DrawImage(rawImage,
                    new SKRect(off_x, off_y, minSide, minSide),
                    new SKRect(0, 0, size, size));

                graph.Flush();
                result = surface.Snapshot();
            }
            // done
            return result;
        }

        /// <summary>
        ///     Blurs given Image, ready for writing text on it
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static SKImage BlurInternal(SKImage input)
        {
            using var g = SKSurface.Create(new SKImageInfo(input.Width, input.Height));
            var canvas = g.Canvas;

            // draw darkening rect
            /* TODO: dont destroy pngs, just make them darker
            using (var darkPaint = new SKPaint())
            {
                darkPaint.BlendMode = SKBlendMode.Luminosity; 
                darkPaint.Color = new SKColor(0, 0, 0, 100);
                canvas.DrawRect(new SKRect(0, 0, input.Width, input.Height), darkPaint);
            }
            */

            // draw blurred image
            var dn = DateTime.Now;
            var blurVal = (dn.DayOfWeek != DayOfWeek.Saturday && dn.DayOfWeek != DayOfWeek.Sunday && dn.Hour > 6 && dn.Hour < 18) ? 28 : 7;
            using (var blurPain = new SKPaint())
            {
                blurPain.ImageFilter = SKImageFilter.CreateBlur(blurVal, blurVal);
                canvas.DrawImage(input, 0, 0, blurPain);
            }

            canvas.Flush();
            var result = g.Snapshot();
            g.Dispose();
            return result;
        }
    }
}
