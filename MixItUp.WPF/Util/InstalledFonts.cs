using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;

namespace MixItUp.WPF.Util
{
    public static class InstalledFonts
    {
        public static IEnumerable<string> GetInstalledFonts()
        {
            using (InstalledFontCollection fontsCollection = new InstalledFontCollection())
            {
                FontFamily[] fontFamilies = fontsCollection.Families;
                List<string> fonts = new List<string>();
                foreach (FontFamily font in fontFamilies)
                {
                    if (!string.IsNullOrEmpty(font.Name))
                    {
                        fonts.Add(font.Name);
                    }
                }
                return fonts;
            }
        }
    }
}
