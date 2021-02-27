using System;
using System.Collections.Generic;

namespace MixItUp.Base.Services
{
    public static class ServiceContainer
    {
        private static Dictionary<Type, object> serviceContainer = new Dictionary<Type, object>();

        public static void Add<T>(T service) { ServiceContainer.serviceContainer[service.GetType()] = service; }

        public static bool Has<T>() { return ServiceContainer.serviceContainer.ContainsKey(typeof(T)); }

        public static T Get<T>()
        {
            if (ServiceContainer.Has<T>())
            {
                return (T)ServiceContainer.serviceContainer[typeof(T)];
            }
            return default(T);
        }

        public static void Remove<T>(T service) { ServiceContainer.serviceContainer.Remove(service.GetType()); }
    }
}
