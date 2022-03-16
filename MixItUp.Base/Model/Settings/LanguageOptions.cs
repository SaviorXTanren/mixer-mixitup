using System.Collections.Generic;

namespace MixItUp.Base.Model.Settings
{
    public enum LanguageOptions
    {
        Default = 0,    // OS Culture

        English = 1,
        German = 2,
        Dutch = 3,
        Spanish = 4,
        Japanese = 5,
        French = 6,
        Portuguese = 7,
        Russian = 8,
        Ukrainian = 9,

        Pseudo = 99999,
    }

    public static class Languages
    {
        public static readonly Dictionary<LanguageOptions, string> LanguageMaps = new Dictionary<LanguageOptions, string>
        {
            { LanguageOptions.Default, "en-US" },

            { LanguageOptions.Dutch, "nl-NL" },
            { LanguageOptions.English, "en-US" },
            { LanguageOptions.German, "de-DE" },
            { LanguageOptions.Spanish, "es-ES" },
            { LanguageOptions.Japanese, "ja-JP" },
            { LanguageOptions.French, "fr-FR" },
            { LanguageOptions.Portuguese, "pt-BR" },
            { LanguageOptions.Russian, "ru-RU" },
            { LanguageOptions.Ukrainian, "uk-UA" },

            { LanguageOptions.Pseudo, "qps-ploc" },
        };

        public static LanguageOptions GetLangauge()
        {
            if (ChannelSession.AppSettings != null)
            {
                return ChannelSession.AppSettings.LanguageOption;
            }
            return LanguageOptions.Default;
        }

        public static string GetLanguageLocale()
        {
            if (Languages.LanguageMaps.TryGetValue(Languages.GetLangauge(), out string locale))
            {
                return locale;
            }
            return Languages.LanguageMaps[LanguageOptions.Default];
        }

        public static string GetLanguageLocale(LanguageOptions language)
        {
            if (Languages.LanguageMaps.TryGetValue(language, out string locale))
            {
                return locale;
            }
            return null;
        }
    }
}
