using System.Collections.Generic;

namespace MixItUp.Base.Util
{
    /// <summary>
    /// Extension methods for the Dictionary class
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the value at the specified key or the default of the value class type.
        /// </summary>
        /// <param name="dictionary">The dictionary to search through</param>
        /// <param name="key">The key to look up</param>
        /// <returns>The value at the specified key or the default of the value class type</returns>
        public static V GetOrDefault<K, V>(this Dictionary<K, V> dictionary, K key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            return default(V);
        }
    }
}
