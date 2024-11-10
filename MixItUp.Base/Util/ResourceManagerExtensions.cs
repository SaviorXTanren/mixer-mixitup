using MixItUp.Base.Util;
using System;
using System.Resources;

namespace MixItUp.Base.Util
{
    public static class ResourceManagerExtensions
    {
        public static string GetSafeString(this ResourceManager resourceManager, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                try
                {
                    return resourceManager.GetString(name) ?? name;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }
            return name;
        }
    }
}
