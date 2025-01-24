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

        public static bool TryGetJObject(this JObject jobj, string key, out JObject result)
        {
            result = null;
            if (jobj.TryGetValue(key, out JToken value) && value is JObject)
            {
                result = (JObject)value;
                return true;
            }
            return false;
        }

        public static bool TryGetJArray(this JObject jobj, string key, out JArray result)
        {
            result = null;
            if (jobj.TryGetValue(key, out JToken value) && value is JArray)
            {
                result = (JArray)value;
                return true;
            }
            return false;
        }

        public static bool TryGetJObject(this JArray jarr, int index, out JObject result)
        {
            result = null;
            if (jarr.TryGetValue(index, out JToken value) && value is JObject)
            {
                result = (JObject)value;
                return true;
            }
            return false;
        }

        public static bool TryGetJArray(this JArray jarr, int index, out JArray result)
        {
            result = null;
            if (jarr.TryGetValue(index, out JToken value) && value is JArray)
            {
                result = (JArray)value;
                return true;
            }
            return false;
        }
    }
}
