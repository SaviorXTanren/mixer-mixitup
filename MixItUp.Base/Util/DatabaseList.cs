using MixItUp.Base.Util;
using System.Collections.Generic;

namespace MixItUp.Base.Util
{
    public class DatabaseList<T> : LockedList<T>
    {
        private HashSet<T> addedValues = new HashSet<T>();
        private Dictionary<T, int> changedValues = new Dictionary<T, int>();
        private HashSet<T> removedValues = new HashSet<T>();

        private object valuesUpdateLock = new object();

        public DatabaseList() { }

        public override void Add(T value)
        {
            if (value != null)
            {
                base.Add(value);
                this.ValueAdded(value);
            }
        }

        public override bool Remove(T value)
        {
            if (value != null)
            {
                this.ValueRemoved(value);
                return base.Remove(value);
            }
            return false;
        }

        public IEnumerable<T> GetAddedValues() { return this.GetValues(this.addedValues); }

        public IEnumerable<T> GetChangedValues()
        {
            Dictionary<T, int> currentHashes = new Dictionary<T, int>();
            foreach (T value in this.ToList())
            {
                currentHashes[value] = JSONSerializerHelper.SerializeToString(value).GetHashCode();
            }

            List<T> results = new List<T>();
            lock (valuesUpdateLock)
            {
                foreach (var current in currentHashes)
                {
                    if (this.changedValues.ContainsKey(current.Key) && this.changedValues[current.Key] != current.Value)
                    {
                        results.Add(current.Key);
                    }
                    this.changedValues[current.Key] = current.Value;
                }
            }
            return results;
        }

        public IEnumerable<T> GetAddedChangedValues()
        {
            List<T> results = new List<T>();
            results.AddRange(this.GetAddedValues());
            results.AddRange(this.GetChangedValues());
            return results;
        }

        public IEnumerable<T> GetRemovedValues() { return this.GetValues(this.removedValues); }

        public void ClearTracking()
        {
            this.addedValues.Clear();
            this.changedValues.Clear();
            this.removedValues.Clear();
        }

        private IEnumerable<T> GetValues(HashSet<T> values)
        {
            lock (valuesUpdateLock)
            {
                List<T> results = new List<T>();
                foreach (T value in values)
                {
                    results.Add(value);
                }
                values.Clear();
                return results;
            }
        }

        private void ValueAdded(T value)
        {
            lock (valuesUpdateLock)
            {
                this.addedValues.Add(value);
            }
        }

        private void ValueRemoved(T value)
        {
            lock (valuesUpdateLock)
            {
                this.removedValues.Add(value);
            }
        }
    }
}
