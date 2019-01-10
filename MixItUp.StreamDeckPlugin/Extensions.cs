using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.StreamDeckPlugin
{
    public static class Extensions
    {
        public static bool Any(this SettingsPropertyCollection properties, string key)
        {
            foreach (SettingsProperty property in properties)
            {
                if (property.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
