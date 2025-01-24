using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.Util
{
    public class DatabaseDictionary<K, V> : LockedDictionary<K, V>
    {
        private HashSet<K> addedValues = new HashSet<K>();
        private HashSet<K> changedValues = new HashSet<K>();
        private HashSet<K> removedValues = new HashSet<K>();

        private object valuesUpdateLock = new object();

        public DatabaseDictionary() { }

        public override V this[K key]
        {
            get
            {
                if (key != null)
                {
                    this.ValueChanged(key);
                    return base[key];
                }
                return default(V);
            }
            set
            {
                if (key != null)
                {
                    if (!this.ContainsKey(key))
                    {
                        this.ValueAdded(key);
                    }
                    base[key] = value;
                    this.ValueChanged(key);
                }
            }
        }

        public override void Add(K key, V value)
        {
            if (key != null)
            {
                base.Add(key, value);
                this.ValueAdded(key);
            }
        }

        public override bool Remove(K key)
        {
            if (key != null)
            {
                this.ValueRemoved(key);
                return base.Remove(key);
            }
            return false;
        }

        public override void Clear()
        {
            base.Clear();
            this.ClearTracking();
        }

        public IEnumerable<K> GetAddedKeys() { return this.GetKeyValues(this.addedValues).Keys; }

        public IEnumerable<V> GetAddedValues() { return this.GetKeyValues(this.addedValues).Values; }

        public IEnumerable<K> GetChangedKeys() { return this.GetKeyValues(this.changedValues).Keys; }

        public IEnumerable<V> GetChangedValues() { return this.GetKeyValues(this.changedValues).Values; }

        public IEnumerable<V> GetAddedChangedValues()
        {
            List<V> values = new List<V>();
            values.AddRange(this.GetAddedValues());
            values.AddRange(this.GetChangedValues());
            return values;
        }

        public IEnumerable<K> GetRemovedValues()
        {
            lock (valuesUpdateLock)
            {
                IEnumerable<K> values = this.removedValues.ToList();
                this.removedValues.Clear();
                return values;
            }
        }

        public void ManualValueChanged(K key)
        {
            if (key != null)
            {
                this.ValueChanged(key);
            }
        }

        public void ManualValueDeleted(K key)
        {
            if (key != null)
            {
                this.ValueRemoved(key);
            }
        }

        public void ClearTracking()
        {
            this.addedValues.Clear();
            this.changedValues.Clear();
            this.removedValues.Clear();
        }

        public void ClearTracking(K id)
        {
            this.addedValues.Remove(id);
            this.changedValues.Remove(id);
            this.removedValues.Remove(id);
        }

        private Dictionary<K, V> GetKeyValues(HashSet<K> keys)
        {
            lock (valuesUpdateLock)
            {
                Dictionary<K, V> values = new Dictionary<K, V>();
                foreach (K key in keys)
                {
                    if (base.ContainsKey(key))
                    {
                        values[key] = base[key];
                    }
                }
                keys.Clear();
                return values;
            }
        }

        private void ValueAdded(K key)
        {
            try
            {
                lock (valuesUpdateLock)
                {
                    this.addedValues.Add(key);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private void ValueChanged(K key)
        {
            try
            {
                lock (valuesUpdateLock)
                {
                    if (this.ContainsKey(key))
                    {
                        this.changedValues.Add(key);
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private void ValueRemoved(K key)
        {
            try
            {
                lock (valuesUpdateLock)
                {
                    this.removedValues.Add(key);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }
    }
}
