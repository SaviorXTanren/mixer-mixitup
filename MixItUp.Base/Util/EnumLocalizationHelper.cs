using StreamingClient.Base.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.Util
{
    public static class EnumLocalizationHelper
    {
        public static string GetLocalizedName<T>(T value)
        {
            string key = EnumHelper.GetEnumName<T>(value);
            return Resources.ResourceManager.GetString(key) ?? key;
        }
    }
}
