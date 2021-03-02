using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using mcswbot2.Minecraft;
using SkiaSharp;

namespace mcswbot2.Static
{
    static class SkiaPlotter
    {
        internal struct PlottableData
        {
            internal string Label { get; }

            private List<double> DataX { get; }
            private List<double> DataY { get; }

            // amount of entries
            internal int Length => DataX.Count;

            public double[] X => DataX.ToArray();
            public double[] Y => DataY.ToArray();
            public double XMin { get; set; }
            public double XMax { get; set; }
            public double YMin { get; set; }
            public double YMax { get; set; }


            /// <summary>
            ///     Adds data [x,y]
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            internal void Add(double x, double y)
            {
                DataX.Add(x);
                if (x < XMin) XMin = x;
                if (x > XMax) XMax = x;
                DataY.Add(y);
                if (y < YMin) YMin = y;
                if (y > YMax) YMax = y;
            }

            /// <summary>
            ///     Returns data [x,y]
            /// </summary>
            /// <param name="index"></param>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            /// <returns></returns>
            internal Tuple<double, double> Get(int index)
            {
                if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index), "Invalid Index!");
                return new Tuple<double, double>(DataX[index], DataY[index]);
            }

            internal PlottableData(string lbl)
            {
                Label = lbl;
                DataX = new List<double>();
                DataY = new List<double>();
                XMin = YMin = double.MaxValue;
                XMax = YMax = double.MinValue;
            }
        }

        private const int LineWidth = 3;

        /// <summary>
        ///     returns all the time-plottable online player count data
        /// </summary>
        /// <returns></returns>
        internal static PlottableData GetUserData(ServerStatus status)
        {
            var dt = DateTime.Now;
            var res = new PlottableData(status.Label);

            // Add all data points
            foreach (var sib in status.Watcher.InfoHistory.Select(siw => siw).OrderByDescending(sib => sib.RequestDate))
                res.Add((sib.RequestDate - dt).TotalDays, sib.CurrentPlayerCount);
            
            res.XMin = Math.Min(-0.1, res.XMin);
            res.YMin = -1; // fix for better visibility
            return res;
        }

        /// <summary>
        ///     returns all the time-plottable ping data
        /// </summary>
        /// <returns></returns>
        internal static PlottableData GetPingData(ServerStatus status)
        {
            var dt = DateTime.Now;
            var res = new PlottableData(status.Label);

            // Add all data points
            foreach (var sib in status.Watcher.InfoHistory.Select(siw => siw).OrderByDescending(sib => sib.RequestDate))
                res.Add((sib.RequestDate - dt).TotalDays, sib.RequestTime);

            res.XMin = Math.Min(-0.1, res.XMin);
            res.YMin = 0; // fix for better visibility
            return res;
        }


        /// <summary>
        ///     Will Plot and save Data to a SKImage
        /// </summary>
        /// <param name="dat"></param>
        internal static SKImage PlotData(IEnumerable<PlottableData> dat, string xLbl, string yLbl, int pxWidth = 690, int pxHeight = 420, bool interpolate = false)
        {
            var plt = new ScottPlot.Plot(pxWidth, pxHeight);

            var allXMin = dat.Min(s => s.XMin);
            var allYMin = dat.Min(s => s.YMin);
            var allYMax = dat.Max(s => s.YMax);
            plt.Axis(allXMin * 1.1, 0, allYMin, allYMax * 1.2);

            plt.Style(ScottPlot.Style.Black);
            plt.Ticks(useMultiplierNotation: false);
            plt.XLabel(xLbl);
            plt.YLabel(yLbl);
            plt.Legend();

            var colorCnt = 0;
            foreach (var da in dat)
            {
                if (da.Length <= 4) continue;
                var col = ColorByIndx(colorCnt++);

                // original points
                plt.PlotScatter(da.X, da.Y, lineWidth: interpolate ? 0 : 1, markerSize: 5d, label: da.Label, color:col);

                // interpolated lines
                if (!interpolate) continue;
                var nsi = new ScottPlot.Statistics.Interpolation.NaturalSpline(da.X, da.Y, 30);
                plt.PlotScatter(nsi.interpolatedXs, nsi.interpolatedYs, lineWidth: 1d, markerSize: 0d, label: null, color: col);
            }

            using var bm = plt.GetBitmap();
            using var stream = new System.IO.MemoryStream();
            bm.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            return SKImage.FromEncodedData(stream.ToArray());
        }

        // TODO
        /// <summary>
        ///     Will Plot and save Data to a SKImage
        /// </summary>
        /// <param name="dat"></param>
        private static SKImage PlotData2(IEnumerable<PlottableData> dat, string xLbl, string yLbl, int pxWidth = 720, int pxHeight = 480)
        {
            var leftAxis = false;
            // 15% width is reserved for Axis
            var plotWidth = pxWidth * 0.85;
            // 30% height is reserved for Axis & legend
            var plotHeight = pxHeight * 0.70;
            // position offset
            var plotXPos = leftAxis ? pxWidth - plotWidth : 0d;
            var plotYPos = leftAxis ? pxHeight - plotHeight : 0d;

            var legend = new Dictionary<string, SKColor>();

            using var g = SKSurface.Create(new SKImageInfo(pxWidth, pxHeight));
            var canvas = g.Canvas;

            var pDat = dat as PlottableData[] ?? dat.ToArray();
            var allXMin = pDat.OrderBy(d => d.XMin).Last().XMin;
            var allXMax = pDat.OrderBy(d => d.XMax).First().XMax;
            var allYMin = pDat.OrderBy(d => d.XMin).Last().YMin;
            var allYMax = pDat.OrderBy(d => d.XMax).First().YMax;
            var xRange = Math.Max(0.0001d, allXMax - allXMin);
            var yRange = Math.Max(0.0001d, allYMax - allYMin);
            var colorIndx = 0;

            foreach (var pd in pDat)
            {
                // choose color
                var lineColor = SKColorByIndx(colorIndx++);
                // add color and name to legend
                legend.Add(pd.Label, lineColor);
                // make line material
                using var linePaint = new SKPaint
                {
                    Color = lineColor,
                    StrokeWidth = LineWidth,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                };

                // get first point
                var (fistX, firstY) = pd.Get(0);
                var lastP = new SKPoint(
                    (float)(plotXPos + plotWidth - plotWidth * (fistX - -allXMin) / xRange),
                    (float)(plotYPos + plotHeight - plotHeight * (firstY - -allYMin) / yRange));

                for (var i = 1; i < pd.Length; i++)
                {
                    // get & translate origin
                    var (thisX, thisY) = pd.Get(i);
                    var thisP = new SKPoint(
                        (float)(plotXPos + plotWidth - plotWidth * (thisX - -allXMin) / xRange),
                        (float)(plotYPos + plotHeight - plotHeight * (thisY - -allYMin) / yRange));
                    // draw from Last to this point
                    canvas.DrawLine(lastP, thisP, linePaint);
                    // update Last point
                    lastP = thisP;
                }
            }

            // draw legend (bottom 15%)
            var legCnt = legend.Count;
            var legSplit = pxWidth / legCnt;
            var dotWidth = pxHeight * 0.15;
            var legWidth = legSplit - dotWidth;
            var legKeys = legend.Keys.ToArray();
            for (var i = 0; i < legCnt; i++)
            {
                var key = legKeys[i];

            }

            // draw axis (bottom 15% and left or right 15%)
            using var axisPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = LineWidth + 2,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };
            // x
            canvas.DrawLine(new SKPoint((float)plotXPos, (float)plotHeight), new SKPoint((float)(plotXPos+plotWidth), (float)plotHeight), axisPaint);
            // y
            var drawYx = (float)(leftAxis ? 0d : plotWidth);
            canvas.DrawLine(new SKPoint(drawYx, 0f), new SKPoint(drawYx, (float)plotHeight), axisPaint);

            // return
            canvas.Flush();
            var image = g.Snapshot();
            g.Dispose();
            return image;
        }

        private static SKColor SKColorByIndx(int indx)
        {
            return (indx % 7) switch
            {
                1 => SKColors.LawnGreen,
                2 => SKColors.CadetBlue,
                4 => SKColors.Orange,
                5 => SKColors.Purple,
                6 => SKColors.Olive,
                _ => SKColors.IndianRed
            };
        }

        private static Color ColorByIndx(int indx)
        {
            return (indx % 7) switch
            {
                1 => Color.LawnGreen,
                2 => Color.DodgerBlue,
                4 => Color.Orange,
                5 => Color.DarkOrchid,
                6 => Color.Olive,
                _ => Color.OrangeRed
            };
        }
    }
}
