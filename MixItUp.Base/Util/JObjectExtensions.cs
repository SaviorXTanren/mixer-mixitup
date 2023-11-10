using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.Util
{
    public static class JObjectExtensions
    {
        public static IEnumerable<string> GetKeys(this JObject jobj)
        {
            return jobj.Properties().Select(p => p.Name).ToList();
        }

        public static T GetValueOrDefault<T>(this JObject jobj, string key, T defaultValue)
        {
            if (jobj.TryGetValue(key, out JToken value) && value != null)
            {
                return value.ToObject<T>();
            }
            return defaultValue;
        }
    }
}
