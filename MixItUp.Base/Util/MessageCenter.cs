using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public static class MessageCenter
    {
        private class MessageCenterKey : IEquatable<MessageCenterKey>
        {
            public string Key { get; set; }
            public Type Type { get; set; }

            public MessageCenterKey(string key, Type type)
            {
                this.Key = key;
                this.Type = type;
            }

            public override bool Equals(object obj)
            {
                if (obj is MessageCenterKey)
                {
                    return this.Equals((MessageCenterKey)obj);
                }
                return false;
            }

            public bool Equals(MessageCenterKey other) { return this.Key.Equals(other.Key) && this.Type.Equals(other.Type); }

            public override int GetHashCode() { return this.Key.GetHashCode() + this.Type.GetHashCode(); }
        }

        private static Dictionary<MessageCenterKey, Dictionary<object, Action<object>>> registeredListeners = new Dictionary<MessageCenterKey, Dictionary<object, Action<object>>>();

        public static void Register<T>(string key, object owner, Action<T> listener)
        {
            MessageCenterKey mcKey = new MessageCenterKey(key, typeof(T));
            if (!registeredListeners.ContainsKey(mcKey))
            {
                registeredListeners[mcKey] = new Dictionary<object, Action<object>>();
            }
            registeredListeners[mcKey][owner] = (value) => { listener((T)value); };
        }

        public static void Unregister<T>(string key, object owner)
        {
            MessageCenterKey mcKey = new MessageCenterKey(key, typeof(T));
            if (registeredListeners.ContainsKey(mcKey))
            {
                registeredListeners[mcKey].Remove(owner);
            }
        }

        public static void Send<T>(string key, T data)
        {
            MessageCenterKey mcKey = new MessageCenterKey(key, typeof(T));
            if (registeredListeners.ContainsKey(mcKey))
            {
                foreach (var listener in registeredListeners[mcKey].Values)
                {
                    listener(data);
                }
            }
        }
    }
}
