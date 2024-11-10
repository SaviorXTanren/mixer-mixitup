using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.Util
{
    public static class EnumLocalizationHelper
    {
        public static string GetLocalizedName<T>(T value)
        {
            return Resources.ResourceManager.GetSafeString(EnumHelper.GetEnumName<T>(value));
        }

        public static IEnumerable<T> GetSortedEnumList<T>()
        {
            return EnumHelper.GetEnumList<T>().OrderBy(s => EnumLocalizationHelper.GetLocalizedName(s));
        }
    }
}
