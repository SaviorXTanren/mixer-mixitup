using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace MixItUp.Base.Util
{
    /// <summary>
    /// Extension methods for the JArray class
    /// </summary>
    public static class JArrayExtensions
    {
        /// <summary>
        /// Converts a JArray to a typed list of objects.
        /// </summary>
        /// <typeparam name="T">The type of objects in the array</typeparam>
        /// <param name="array">The JArray to convert</param>
        /// <returns>The converted list of objects</returns>
        public static List<T> ToTypedArray<T>(this JArray array)
        {
            List<T> results = new List<T>();
            if (array != null)
            {
                foreach (JToken token in array)
                {
                    results.Add(token.ToObject<T>());
                }
            }
            return results;
        }
    }
}