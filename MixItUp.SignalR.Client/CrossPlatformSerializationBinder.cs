using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;

namespace MixItUp.SignalR.Client
{
    public class CrossPlatformSerializationBinder : DefaultSerializationBinder
    {
        private static bool _isNetCore = Type.GetType("System.String, System.Private.CoreLib") != null;
        private ConcurrentDictionary<string, Type> _mappedTypes = new ConcurrentDictionary<string, Type>();

        public override Type BindToType(string assemblyName, string typeName)
        {
            this._mappedTypes.TryGetValue(typeName, out Type t);

            if (t != null)
                return t;

            var originalTypeName = typeName;

            if (CrossPlatformSerializationBinder._isNetCore)
            {
                typeName = typeName.Replace("mscorlib", "System.Private.CoreLib");
                assemblyName = assemblyName.Replace("mscorlib", "System.Private.CoreLib");
            }
            else
            {
                typeName = typeName.Replace("System.Private.CoreLib", "mscorlib");
                assemblyName = assemblyName.Replace("System.Private.CoreLib", "mscorlib");
            }

            t = base.BindToType(assemblyName, typeName);
            this._mappedTypes.TryAdd(originalTypeName, t);
            return t;
        }
    }
}
