using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MixItUp.Base.Util
{
    public static class ReflectionHelper
    {
        public static IEnumerable<T> GetInstancesImplementingType<T>()
        {
            return (from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.BaseType == (typeof(T)) && t.GetConstructor(Type.EmptyTypes) != null
                    select (T)Activator.CreateInstance(t)).ToList();
        }
    }
}
