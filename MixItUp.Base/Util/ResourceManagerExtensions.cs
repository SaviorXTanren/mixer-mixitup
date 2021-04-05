using System.Resources;

namespace MixItUp.Base.Util
{
    public static class ResourceManagerExtensions
    {
        public static string GetSafeString(this ResourceManager resourceManager, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return resourceManager.GetString(name) ?? name;
            }
            return name;
        }
    }
}
