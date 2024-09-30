using System.Collections.Generic;
using System.Globalization;

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
        PortugueseBrazil = 7,
        Russian = 8,
        Ukrainian = 9,
        ChineseTraditional = 10,
        PortuguesePortugal = 11,
        Italian = 12,
        Polish = 13,

        Pseudo = 99999,
    }

    public static class Languages
    {
        public static readonly Dictionary<LanguageOptions, string> LanguageMaps = new Dictionary<LanguageOptions, string>
        {
            { LanguageOptions.Default, "en-US" },

            { LanguageOptions.ChineseTraditional, "zh-TW" },
            { LanguageOptions.Dutch, "nl-NL" },
            { LanguageOptions.English, "en-US" },
            { LanguageOptions.German, "de-DE" },
            { LanguageOptions.Spanish, "es-ES" },
            { LanguageOptions.Japanese, "ja-JP" },
            { LanguageOptions.French, "fr-FR" },
            { LanguageOptions.Italian, "it-IT" },
            { LanguageOptions.Polish, "pl-PL" },
            { LanguageOptions.PortugueseBrazil, "pt-BR" },
            { LanguageOptions.PortuguesePortugal, "pt-PT" },
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

        public static CultureInfo GetLanguageLocaleCultureInfo()
        {
            return new CultureInfo(Languages.GetLanguageLocale());
        }
    }
}
