using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.Util
{
    public static class ColorSchemes
    {
        public const string DefaultColorScheme = "Black";

        public static readonly Dictionary<string, string> HTMLColorSchemeDictionary = new Dictionary<string, string>()
        {
            { "Amber", "#ffb300" },
            { "Black", "#000000" },
            { "Blue", "#2196f3" },
            { "Blue Grey", "#607d8b" },
            { "Brown", "#795548" },
            { "Cyan", "#00bcd4" },
            { "Deep Orange", "#ff5722" },
            { "Deep Purple", "#673ab7" },
            { "Green", "#4caf50" },
            { "Grey", "#9e9e9e" },
            { "Indigo", "#3f51b5" },
            { "Light Blue", "#03a9f4" },
            { "Light Green", "#8bc34a" },
            { "Lime", "#cddc39" },
            { "Orange", "#ff9800" },
            { "Pink", "#e91e63" },
            { "Purple", "#9c27b0" },
            { "Red", "#f44336" },
            { "Teal", "#009688" },
            { "White", "#ffffff" },
            { "Yellow", "#ffeb3b" },
        };

        public static readonly HashSet<string> WPFColorSchemeDictionary = new HashSet<string>()
        {
            "Black",
            "Blue",
            "Brown",
            "Cyan",
            "Gray",
            "Green",
            "Indigo",
            "Lime",
            "Orange",
            "Pink",
            "Purple",
            "Red",
            "Teal",
            "Transparent",
            "White",
            "Yellow",
        };

        public static string GetColorCode(string name)
        {
            if (!string.IsNullOrEmpty(name) && ColorSchemes.HTMLColorSchemeDictionary.ContainsKey(name))
            {
                return ColorSchemes.HTMLColorSchemeDictionary[name];
            }
            return name;
        }

        public static string GetColorName(string code)
        {
            if (!string.IsNullOrEmpty(code) && ColorSchemes.HTMLColorSchemeDictionary.ContainsValue(code))
            {
                return ColorSchemes.HTMLColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(code)).Key;
            }
            return code;
        }
    }
}
