using System;
using System.Collections.Generic;
using System.Linq;
using mcswlib.ServerStatus;
using SkiaSharp;

namespace mcswbot2.Bot
{
    static class SkiaPlotter
    {
        private static Random rand = new Random(420 * 69);

        internal struct PlottableData
        {
            internal string Label { get; }

            private List<double> DataX { get; }
            private List<double> DataY { get; }

            // amount of entries
            internal int Length => DataX.Count;

            public double[] X => DataX.ToArray();
            public double[] Y => DataY.ToArray();
            public double xMin { get; private set; }
            public double xMax { get; private set; }
            public double yMin { get; private set; }
            public double yMax { get; private set; }


            /// <summary>
            ///     Adds data [x,y]
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            internal void Add(double x, double y)
            {
                DataX.Add(x);
                if (x < xMin) xMin = x;
                if (x > xMax) xMax = x;
                DataY.Add(y);
                if (y < yMin) yMin = y;
                if (y > yMax) yMax = y;
            }

            /// <summary>
            ///     Returns data [x,y]
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            internal Tuple<double, double> Get(int index)
            {
                if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException("Invalid Index!");
                return new Tuple<double, double>(DataX[index], DataY[index]);
            }

            internal PlottableData(string lbl)
            {
                Label = lbl;
                DataX = new List<double>();
                DataY = new List<double>();
                xMin = yMin = double.MaxValue;
                xMax = yMax = double.MinValue;
            }
        }

        private const int LineWidth = 3;

        /// <summary>
        ///     returns all the time-plottable online player count data
        /// </summary>
        /// <returns></returns>
        internal static PlottableData GetUserData(ServerStatus Status)
        {
            var dt = DateTime.Now;
            var res = new PlottableData(Status.Label);
            foreach (var sib in Status.Updater.History)
            {
                res.Add((dt - sib.RequestDate.AddMilliseconds(sib.RequestTime)).TotalMinutes, sib.CurrentPlayerCount);
            }
            return res;
        }

        /// <summary>
        ///     returns all the time-plottable ping data
        /// </summary>
        /// <returns></returns>
        internal static PlottableData GetPingData(ServerStatus Base)
        {
            var dt = DateTime.Now;
            var res = new PlottableData(Base.Label);
            foreach (var sib in Base.Updater.History)
            {
                res.Add(dt.Subtract(sib.RequestDate.AddMilliseconds(sib.RequestTime)).TotalMinutes, sib.RequestTime);
            }
            return res;
        }


        /// <summary>
        ///     Will Plot and save Data to a file
        /// </summary>
        /// <param name="dat"></param>
        internal static SKImage PlotData(IEnumerable<PlottableData> dat, string xLbl, string yLbl, int pxWidth = 720, int pxHeight = 480)
        {
            var plt = new ScottPlot.Plot(pxWidth, pxHeight);
            plt.XLabel(xLbl);
            plt.YLabel(yLbl);
            plt.Legend();
            foreach (var da in dat)
                if (da.Length > 0)
                    plt.PlotScatter(da.X, da.Y, null, 1D, 5D, da.Label);
            using var bm = plt.GetBitmap();
            using var stream = new System.IO.MemoryStream();
            bm.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            return SKImage.FromEncodedData(stream.ToArray());
        }

        // TODO
        /// <summary>
        ///     Will Plot and save Data to a file
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
            var allXMin = pDat.OrderBy(d => d.xMin).Last().xMin;
            var allXMax = pDat.OrderBy(d => d.xMax).First().xMax;
            var allYMin = pDat.OrderBy(d => d.xMin).Last().yMin;
            var allYMax = pDat.OrderBy(d => d.xMax).First().yMax;
            var xRange = Math.Max(0.0001d, allXMax - allXMin);
            var yRange = Math.Max(0.0001d, allYMax - allYMin);
            var colorIndx = 0;

            foreach (var pd in pDat)
            {
                // choose color
                var lineColor = ColorByIndx(colorIndx++);
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
                    // draw from last to this point
                    canvas.DrawLine(lastP, thisP, linePaint);
                    // update last point
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

        private static SKColor ColorByIndx(int indx)
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
    }
}
