using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;

namespace mcswbot2.Static
{
    // TODO find better way then System.Drawing only for Fonts...
    internal static class Fonts
    {
        internal static FontFamily GetCustomFont()
        {
            try
            {
                var privateFonts = new PrivateFontCollection();
                privateFonts.AddFontFile("./fonts/segoe_ui.ttf");
                return privateFonts.Families[0];
            }
            catch (Exception e)
            {
                Logger.WriteLine("Font Error: " + e, Types.LogLevel.Error);
            }
            return GetDefaultFontName();
        }

        internal static FontFamily GetDefaultFontName()
        {
            return ((GetSansFontName() ?? GetSerifFontName()) ?? GetMonospaceFontName()) ?? SystemFonts.DefaultFont.FontFamily;
        }

        internal static FontFamily GetSansFontName()
        {
            var sansFonts = new string[] { "Segoe UI", "DejaVu Sans", "Helvetica" };
            return GetValidFontName(sansFonts);
        }

        internal static FontFamily GetSerifFontName()
        {
            var serifFonts = new string[] { "Times New Roman", "DejaVu Serif", "Times" };
            return GetValidFontName(serifFonts);
        }

        internal static FontFamily GetMonospaceFontName()
        {
            var monospaceFonts = new string[] { "Consolas", "DejaVu Sans Mono", "Courier" };
            return GetValidFontName(monospaceFonts);
        }

        internal static FontFamily GetValidFontName(string fontName)
        {
            foreach (var installedFont in FontFamily.Families)
                if (string.Equals(installedFont.Name, fontName, StringComparison.OrdinalIgnoreCase))
                    return installedFont;

            return GetDefaultFontName();
        }

        internal static FontFamily GetValidFontName(string[] fontNames)
        {
            return (from preferred in fontNames
                    from font in FontFamily.Families
                    where string.Equals(preferred, font.Name, StringComparison.OrdinalIgnoreCase)
                    select font).FirstOrDefault();
        }
    }
}
