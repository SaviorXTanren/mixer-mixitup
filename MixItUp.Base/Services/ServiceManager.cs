using System;
using System.Collections.Generic;

namespace MixItUp.Base.Services
{
    public static class ServiceManager
    {
        private static Dictionary<Type, object> serviceContainer = new Dictionary<Type, object>();

        public static void Add<T>(T service) { ServiceManager.serviceContainer[service.GetType()] = service; }

        public static bool Has<T>() { return ServiceManager.serviceContainer.ContainsKey(typeof(T)); }

        public static T Get<T>()
        {
            if (ServiceManager.Has<T>())
            {
                return (T)ServiceManager.serviceContainer[typeof(T)];
            }
            return default(T);
        }

        public static void Remove<T>(T service) { ServiceManager.serviceContainer.Remove(service.GetType()); }
    }
}
