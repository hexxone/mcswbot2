using System.Collections.Generic;

namespace mcswbot2.Lib
{
    public class Types
    {
        /// <summary>
        ///     Represents some simple data tthat can be plotted.
        /// </summary>
        public struct PlottableData
        {
            public string Label { get; private set; }
            public double[] dataX { get; private set; }
            public double[] dataY { get; private set; }

            public PlottableData(string lbl, double[] x, double[] y)
            {
                Label = lbl;  dataX = x; dataY = y;
            }
        }

        /// <summary>
        ///     Used to specially format Event messages in HTML Markup
        /// </summary>
        public enum Formatting
        {
            None,
            Html,
            Markup
        }

        /// <summary>
        ///     Gets HTML colors associated with specific formatting codes
        /// </summary>
        public static Dictionary<char, string> MinecraftColors =>
            new Dictionary<char, string>
            {
                {'0', "#000000"}, {'1', "#0000AA"}, {'2', "#00AA00"}, {'3', "#00AAAA"}, {'4', "#AA0000"},
                {'5', "#AA00AA"}, {'6', "#FFAA00"}, {'7', "#AAAAAA"},
                {'8', "#555555"}, {'9', "#5555FF"}, {'a', "#55FF55"}, {'b', "#55FFFF"}, {'c', "#FF5555"},
                {'d', "#FF55FF"}, {'e', "#FFFF55"}, {'f', "#FFFFFF"}
            };

        /// <summary>
        ///     Gets HTML styles associated with specific formatting codes
        /// </summary>
        public static Dictionary<char, string> MinecraftStyles =>
            new Dictionary<char, string>
            {
                {'k', "none;font-weight:normal;font-style:normal"},
                {'m', "line-through;font-weight:normal;font-style:normal"},
                {'l', "none;font-weight:900;font-style:normal"},
                {'n', "underline;font-weight:normal;font-style:normal;"},
                {'o', "none;font-weight:normal;font-style:italic;"},
                {'r', "none;font-weight:normal;font-style:normal;color:#FFFFFF;"}
            };
    }
}