using System.Collections;
using System.Collections.Generic;

namespace MixItUp.Base.Util
{
    public class LockedHashSet<T> : ISet<T>
    {
        private HashSet<T> items = new HashSet<T>();
        private object objLock = new object();

        public LockedHashSet() { }

        public LockedHashSet(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                this.items.Add(item);
            }
        }

        public int Count { get { lock (objLock) { return this.items.Count; } } }

        public bool IsReadOnly { get { return false; } }

        public bool Add(T item) { lock (objLock) { return this.items.Add(item); } }

        public void Clear() { lock (objLock) { this.items.Clear(); } }

        public bool Contains(T item) { lock (objLock) { return this.items.Contains(item); } }

        public void CopyTo(T[] array, int arrayIndex) { lock (objLock) { this.items.CopyTo(array, arrayIndex); } }

        public void ExceptWith(IEnumerable<T> other) { lock (objLock) { this.items.ExceptWith(other); } }

        public IEnumerator<T> GetEnumerator() { lock (objLock) { return this.items.GetEnumerator(); } }

        public void IntersectWith(IEnumerable<T> other) { lock (objLock) { this.items.IntersectWith(other); } }

        public bool IsProperSubsetOf(IEnumerable<T> other) { lock (objLock) { return this.items.IsProperSubsetOf(other); } }

        public bool IsProperSupersetOf(IEnumerable<T> other) { lock (objLock) { return this.items.IsProperSupersetOf(other); } }

        public bool IsSubsetOf(IEnumerable<T> other) { lock (objLock) { return this.items.IsSubsetOf(other); } }

        public bool IsSupersetOf(IEnumerable<T> other) { lock (objLock) { return this.items.IsSupersetOf(other); } }

        public bool Overlaps(IEnumerable<T> other) { lock (objLock) { return this.items.Overlaps(other); } }

        public bool Remove(T item) { lock (objLock) { return this.items.Remove(item); } }

        public bool SetEquals(IEnumerable<T> other) { lock (objLock) { return this.items.SetEquals(other); } }

        public void SymmetricExceptWith(IEnumerable<T> other) { lock (objLock) { this.items.SymmetricExceptWith(other); } }

        public void UnionWith(IEnumerable<T> other) { lock (objLock) { this.items.UnionWith(other); } }

        void ICollection<T>.Add(T item) { this.Add(item); }

        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
    }
}
