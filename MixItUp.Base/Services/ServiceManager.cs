using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.Services
{
    public static class ServiceManager
    {
        public static event EventHandler<string> OnServiceDisconnect = delegate { };
        public static void ServiceDisconnect(string serviceName) { OnServiceDisconnect(null, serviceName); }

        public static event EventHandler<string> OnServiceReconnect = delegate { };
        public static void ServiceReconnect(string serviceName) { OnServiceReconnect(null, serviceName); }

        private static Dictionary<Type, object> services = new Dictionary<Type, object>();

        public static void Add<T>(T service) { ServiceManager.services[typeof(T)] = service; }

        public static bool Has<T>() { return ServiceManager.services.ContainsKey(typeof(T)); }

        public static T Get<T>()
        {
            if (ServiceManager.Has<T>())
            {
                return (T)ServiceManager.services[typeof(T)];
            }
            return default(T);
        }

        public static IEnumerable<T> GetAll<T>()
        {
            List<T> results = new List<T>();
            foreach (object service in ServiceManager.services.Values.ToList())
            {
                if (service is T)
                {
                    results.Add((T)service);
                }
            }
            return results;
        }

        public static void Remove<T>() { ServiceManager.services.Remove(typeof(T)); }
    }
}
