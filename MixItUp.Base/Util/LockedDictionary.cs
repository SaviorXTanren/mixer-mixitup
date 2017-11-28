using System.Collections.Generic;

namespace MixItUp.Base.Util
{
    public class LockedDictionary<K, V>
    {
        private Dictionary<K, V> items = new Dictionary<K, V>();
        private object objLock = new object();

        public LockedDictionary() { }

        public LockedDictionary(IDictionary<K,V> items)
        {
            foreach (var kvp in items)
            {
                this.items.Add(kvp.Key, kvp.Value);
            }
        }

        public V this[K key]
        {
            get { lock (objLock) { return this.items[key]; } }
            set { lock (objLock) { this.items[key] = value; } }
        }

        public int Count { get { lock (objLock) { return this.items.Count; } } }

        public bool IsReadOnly { get { return false; } }

        public ICollection<K> Keys { get { lock (objLock) { return this.items.Keys; } } }

        public ICollection<V> Values { get { lock (objLock) { return this.items.Values; } } }

        public void Add(KeyValuePair<K, V> item) { this.Add(item.Key, item.Value); }

        public void Add(K key, V value) { lock (objLock) { this.items.Add(key, value); } }

        public void Clear() { lock (objLock) { this.items.Clear(); } }

        public bool ContainsKey(K key) { lock (objLock) { return this.items.ContainsKey(key); } }

        public bool ContainsValue(V value) { lock (objLock) { return this.items.ContainsValue(value); } }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() { lock (objLock) { return this.items.GetEnumerator(); } }

        public bool Remove(K key) { lock (objLock) { return this.items.Remove(key); } }

        public bool TryGetValue(K key, out V value) { lock (objLock) { return this.items.TryGetValue(key, out value); } }

        public Dictionary<K, V> ToDictionary() { return new Dictionary<K, V>(this.items); }
    }
}
