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
            { "BlueGrey", "#607d8b" },
            { "Brown", "#795548" },
            { "Cyan", "#00bcd4" },
            { "DeepOrange", "#ff5722" },
            { "DeepPurple", "#673ab7" },
            { "Green", "#4caf50" },
            { "Grey", "#9e9e9e" },
            { "Indigo", "#3f51b5" },
            { "LightBlue", "#03a9f4" },
            { "LightGreen", "#8bc34a" },
            { "Lime", "#cddc39" },
            { "Orange", "#ff9800" },
            { "Pink", "#e91e63" },
            { "Purple", "#9c27b0" },
            { "Red", "#f44336" },
            { "Transparent", "Transparent" },
            { "Teal", "#009688" },
            { "White", "#ffffff" },
            { "Yellow", "#ffeb3b" },
        };

        public static string GetColorCode(string name)
        {
            if (!string.IsNullOrEmpty(name) && ColorSchemes.HTMLColorSchemeDictionary.ContainsKey(name))
            {
                return ColorSchemes.HTMLColorSchemeDictionary[name];
            }

            var locLookup = HTMLColorSchemeDictionary.Select(kvp => kvp.Key).SingleOrDefault(key => MixItUp.Base.Resources.ResourceManager.GetSafeString(key) == name);
            if (locLookup != null)
            {
                return ColorSchemes.HTMLColorSchemeDictionary[locLookup];
            }

            return name;
        }

        public static string GetColorName(string code)
        {
            if (!string.IsNullOrEmpty(code) && ColorSchemes.HTMLColorSchemeDictionary.ContainsValue(code))
            {
                var key = ColorSchemes.HTMLColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(code)).Key;
                return MixItUp.Base.Resources.ResourceManager.GetSafeString(key);
            }
            return code;
        }
    }
}
