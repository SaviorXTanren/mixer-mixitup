using MixItUp.Base.Util;
using System;
using System.Resources;

namespace MixItUp.Base.Localization
{
    public static class LocalizationHandler
    {
        private static ResourceManager ResourceManager = null;

        public static void SetCurrentLanguage(string languageName)
        {
            switch (languageName)
            {
                case "jp":
                    LocalizationHandler.ResourceManager = MixItUp.Base.Localization.Japanese.ResourceManager;
                    break;
                case "sp":
                    LocalizationHandler.ResourceManager = MixItUp.Base.Localization.Spanish.ResourceManager;
                    break;
                case "en":
                default:
                    LocalizationHandler.ResourceManager = MixItUp.Base.Localization.English.ResourceManager;
                    break;
            }
        }

        public static string GetLocalizationString(string key)
        {
            return LocalizationHandler.GetLocalizationString(LocalizationHandler.ResourceManager, key);
        }

        public static string GetLocalizationString(ResourceManager resourceManager, string key)
        {
            string value = null;
            try
            {
                value = resourceManager.GetString(key);
            }
            catch (Exception ex) { Logger.Log(ex); }

            if (string.IsNullOrEmpty(value))
            {
                return LocalizationHandler.GetLocalizationString(MixItUp.Base.Localization.English.ResourceManager, key);
            }
            return value;
        }
    }
}
