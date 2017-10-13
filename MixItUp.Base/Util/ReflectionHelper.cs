using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MixItUp.Base.Util
{
    public static class ReflectionHelper
    {
        public static IEnumerable<T> CreateInstancesOfImplementingType<T>()
        {
            return (from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.BaseType == (typeof(T)) && t.GetConstructor(Type.EmptyTypes) != null
                    select (T)Activator.CreateInstance(t)).ToList();
        }

        public static T CreateInstanceOf<T>()
        {
            Type type = typeof(T);
            if (type.GetConstructor(Type.EmptyTypes) != null)
            {
                return (T)Activator.CreateInstance(type);
            }
            else
            {
                return default(T);
            }
        }
    }
}
