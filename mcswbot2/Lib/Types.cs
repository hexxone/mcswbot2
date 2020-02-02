using System;

namespace mcswbot2.Lib
{
    internal static class Types
    {
        /// <summary>
        ///     Represents some simple data tthat can be plotted.
        /// </summary>
        public struct PlottableData
        {
            public string Label { get; private set; }
            public double[] DataX { get; private set; }
            public double[] DataY { get; private set; }

            public PlottableData(string lbl, double[] x, double[] y)
            {
                if (x == null || y == null || x.Length != y.Length) throw new Exception("Invalid data arguments!");
                Label = lbl; DataX = x; DataY = y;
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
        ///     removes Minecraft Chat Syle informations
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string FixMcChat(string s)
        {
            var l = new[]
            {
                "§4", "§c", "§6", "§e",
                "§2", "§a", "§b", "§3",
                "§1", "§9", "§d", "§5",
                "§f", "§7", "§8", "§0",
                "§l", "§m", "§n", "§o", "§r"
            };
            foreach (var t in l) s = s.Replace(t, "");
            return s;
        }

    }
}