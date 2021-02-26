using System;
using System.Collections.Generic;

namespace MixItUp.Base.Services
{
    public static class ServiceContainer
    {
        private static Dictionary<Type, object> serviceContainer = new Dictionary<Type, object>();

        public static void Add<T>(T service) { ServiceContainer.serviceContainer[service.GetType()] = service; }

        public static T Get<T>()
        {
            ServiceContainer.serviceContainer.TryGetValue(typeof(T), out object service);
            return (T)service;
        }

        public static void Remove<T>(T service) { ServiceContainer.serviceContainer.Remove(service.GetType()); }
    }
}
