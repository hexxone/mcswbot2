using mcswbot2.Bot.Objects;
using mcswlib.ServerStatus;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;

namespace mcswbot2.Bot
{
    internal static class Imaging
    {

        /// <summary>
        ///     returns all the time-plottable online player count data
        /// </summary>
        /// <returns></returns>
        internal static PlottableData GetUserData(ServerStatus Status)
        {
            var nauw = DateTime.Now;
            var lx = new List<double>();
            var ly = new List<double>();
            foreach (var infoBase in Status.Updater.History)
            {
                var diff = nauw.Subtract(infoBase.RequestDate.AddMilliseconds(infoBase.RequestTime)).TotalMinutes;
                lx.Add(diff);
                ly.Add(infoBase.CurrentPlayerCount);
            }
            return new PlottableData(Status.Label, lx.ToArray(), ly.ToArray());
        }

        /// <summary>
        ///     returns all the time-plottable ping data
        /// </summary>
        /// <returns></returns>
        internal static PlottableData GetPingData(ServerStatus Base)
        {
            var nauw = DateTime.Now;
            var lx = new List<double>();
            var ly = new List<double>();
            foreach (var infoBase in Base.Updater.History)
            {
                var diff = nauw.Subtract(infoBase.RequestDate.AddMilliseconds(infoBase.RequestTime)).TotalMinutes;
                lx.Add(diff);
                ly.Add(infoBase.RequestTime);
            }
            return new PlottableData(Base.Label, lx.ToArray(), ly.ToArray());
        }

        /// <summary>
        ///     Will Plot and save Data to a file
        /// </summary>
        /// <param name="dat"></param>
        internal static Bitmap PlotData(PlottableData[] dat, string xLab, string yLab)
        {
            var plt = new ScottPlot.Plot(355, 200);
            plt.XLabel(xLab);
            plt.YLabel(yLab);
            plt.Legend(true);
            foreach (var da in dat)
                if (da.DataX.Length > 0)
                    plt.PlotScatter(da.DataX, da.DataY, null, 1D, 5D, da.Label);
            return plt.GetBitmap();
        }

        /// <summary>
        ///     Wrapper for scaling and writing text to an image
        /// </summary>
        /// <param name="input"></param>
        /// <param name="txt"></param>
        /// <returns></returns>
        internal static Bitmap MakeSticker(Bitmap input, string txt)
        {
            using (var scaled = MakeThumbnail(input))
                return WriteText(scaled, Utils.NoHtml(txt));
        }

        /// <summary>
        ///     Writes given text centered on the image
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="txt"></param>
        /// <returns></returns>
        internal static Bitmap WriteText(Bitmap bmp, string txt)
        {
            var rectf = new RectangleF(0, 0, bmp.Width, bmp.Height);

            var dn = DateTime.Now;
            var blurVal = 3;
            if (dn.DayOfWeek != DayOfWeek.Saturday && dn.DayOfWeek != DayOfWeek.Sunday && dn.Hour > 8 && dn.Hour < 17) blurVal = 20;
            // fast gaussian
            var gb = new GaussianBlur(bmp);
            var blurred = gb.Process(blurVal);
            // Create graphic object that will draw onto the bitmap
            using (var g = Graphics.FromImage(blurred))
            {
                // NOTE that path gradient brushes do not obey the smoothing mode. 
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // The interpolation mode determines how intermediate values between two endpoints are calculated.
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                // Use this property to specify either higher quality, slower rendering, or lower quality, faster rendering of the contents of this Graphics object.
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                // This one is important
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                // dampening
                using (Brush brush = new SolidBrush(Color.FromArgb(137, Color.Black)))
                    g.FillRectangle(brush, rectf);

                var fSize = 26;
                var df = Fonts.GetCustomFont();
                var sf = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                // Draw the text
                var fo = new Font(df, fSize);
                g.DrawString(txt, fo, Brushes.White, rectf, sf);
                g.Flush();
            }
            // Now save or use the bitmap
            return blurred;
        }

        /// <summary>
        ///     Auto-Scale and fit images to Thumbnails of size 512x512
        /// </summary>
        /// <param name="rawImage"></param>
        /// <returns></returns>
        internal static Bitmap MakeThumbnail(Bitmap rawImage, int size = 512)
        {
            // calculate crop
            var minSide = Math.Min(rawImage.Width, rawImage.Height);
            int off_x = 0, off_y = 0;
            if (minSide < rawImage.Width) off_x = (rawImage.Width - minSide) / 2;
            if (minSide < rawImage.Height) off_y = (rawImage.Height - minSide) / 2;
            // make new bitmap
            var scaledBitmap = new Bitmap(size, size);
            Graphics graph = Graphics.FromImage(scaledBitmap);
            graph.InterpolationMode = InterpolationMode.High;
            graph.CompositingQuality = CompositingQuality.HighQuality;
            graph.SmoothingMode = SmoothingMode.AntiAlias;
            // fill white background
            graph.FillRectangle(new SolidBrush(Color.White), new RectangleF(0, 0, size, size));
            // scale fill cropped image
            graph.DrawImage(rawImage,
                new Rectangle(0, 0, size, size),
                new Rectangle(off_x, off_y, minSide, minSide),
                GraphicsUnit.Pixel);
            // done
            return scaledBitmap;
        }

        /// <summary>
        /// ...
        /// </summary>
        /// <param name="recurseTry"></param>
        /// <param name="recurseTries"></param>
        /// <returns></returns>
        internal static Bitmap TheThingWeDontTalkAbout(int recurseTry = 0, int recurseTries = 5)
        {
            try
            {
                var booru = new BooruSharp.Booru.Gelbooru();
                var result = booru.GetRandomImage(new[] { "" }).Result;
                if (result.fileUrl == null) throw new ArgumentNullException("No result!");
                var request = System.Net.WebRequest.Create(result.fileUrl);
                var response = request.GetResponse();
                var responseStream = response.GetResponseStream();
                return new Bitmap(responseStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Imaging-Exception: " + ex);
                if (recurseTry < recurseTries)
                    return TheThingWeDontTalkAbout(recurseTry + 1);
            }
            return null;
        }
    }
}
