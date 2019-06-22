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
    }
}
