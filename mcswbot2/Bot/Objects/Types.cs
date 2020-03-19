using System;

namespace mcswbot2.Bot.Objects
{
    /// <summary>
    ///     Represents some simple data that can be plotted.
    /// </summary>
    internal struct PlottableData
    {
        internal string Label { get; private set; }
        internal double[] DataX { get; private set; }
        internal double[] DataY { get; private set; }

        internal PlottableData(string lbl, double[] x, double[] y)
        {
            if (x == null || y == null || x.Length != y.Length) throw new Exception("Invalid data arguments!");
            Label = lbl; DataX = x; DataY = y;
        }
    }
}
