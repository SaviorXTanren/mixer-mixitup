using StreamingClient.Base.Util;

namespace MixItUp.Base.Util
{
    public static class EnumLocalizationHelper
    {
        public static string GetLocalizedName<T>(T value)
        {
            return Resources.ResourceManager.GetSafeString(EnumHelper.GetEnumName<T>(value));
        }
    }
}
