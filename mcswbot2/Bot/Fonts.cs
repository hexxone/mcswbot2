using System.Drawing;
using System.Drawing.Text;

namespace mcswbot2.Bot
{
    internal static class Fonts
    {
        internal static FontFamily GetCustomFont()
        {
            var privateFonts = new PrivateFontCollection();
            privateFonts.AddFontFile("./fonts/segoe_ui.ttf");
            return privateFonts.Families[0];
        }

        internal static FontFamily GetDefaultFontName()
        {
            var f = GetSansFontName();
            if (f == null) f = GetSerifFontName();
            if (f == null) f = GetMonospaceFontName();
            if (f == null) f = SystemFonts.DefaultFont.FontFamily;
            return f;
        }

        internal static FontFamily GetSansFontName()
        {
            string[] sansFonts = new string[] { "Segoe UI", "DejaVu Sans", "Helvetica" };
            return GetValidFontName(sansFonts);
        }

        internal static FontFamily GetSerifFontName()
        {
            string[] serifFonts = new string[] { "Times New Roman", "DejaVu Serif", "Times" };
            return GetValidFontName(serifFonts);
        }

        internal static FontFamily GetMonospaceFontName()
        {
            string[] monospaceFonts = new string[] { "Consolas", "DejaVu Sans Mono", "Courier" };
            return GetValidFontName(monospaceFonts);
        }

        internal static FontFamily GetValidFontName(string fontName)
        {
            foreach (FontFamily installedFont in FontFamily.Families)
                if (string.Equals(installedFont.Name, fontName, System.StringComparison.OrdinalIgnoreCase))
                    return installedFont;

            return GetDefaultFontName();
        }

        internal static FontFamily GetValidFontName(string[] fontNames)
        {
            foreach (string preferredFont in fontNames)
                foreach (FontFamily font in FontFamily.Families)
                    if (string.Equals(preferredFont, font.Name, System.StringComparison.OrdinalIgnoreCase))
                        return font;
            return null;
        }
    }
}
