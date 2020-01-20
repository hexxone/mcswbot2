using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ScottPlot
{
    public static class GlobalFont
    {
        public static string GetDefault()
        {
            foreach (FontFamily font in FontFamily.Families)
            {
                var fntu = font.Name.ToUpper();
                if (fntu.Contains("SEGOE") || fntu.Contains("DEJAVU") || fntu.Contains("SANS"))
                    return font.Name;
            }
            Console.WriteLine("No vaild known Font! Using any as fallback..");
            foreach (FontFamily font in FontFamily.Families)
            {
                return font.Name;
            }
            // uh oh
            throw new Exception("No Fonts have been found on the System!!!");
        }
    }
}
