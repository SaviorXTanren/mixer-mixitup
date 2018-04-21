using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.Util
{
    public class LockedList<T> : IList<T>
    {
        private List<T> items = new List<T>();
        private object objLock = new object();

        public LockedList() { }

        public LockedList(IEnumerable<T> items) { this.items.AddRange(items); }

        public T this[int index]
        {
            get { lock (objLock) { return this.items[index]; } }
            set { lock (objLock) { this.items[index] = value; } }
        }

        public int Count { get { lock (objLock) { return this.items.Count; } } }

        public bool IsReadOnly { get { return false; } }

        public void Add(T item) { lock (objLock) { this.items.Add(item); } }

        public void Clear() { lock (objLock) { this.items.Clear(); } }

        public bool Contains(T item) { lock (objLock) { return this.items.Contains(item); } }

        public void CopyTo(T[] array, int arrayIndex) { lock (objLock) { this.items.CopyTo(array, arrayIndex); } }

        public IEnumerator<T> GetEnumerator() { lock (objLock) { return this.ToList().GetEnumerator(); } }

        public int IndexOf(T item) { lock (objLock) { return this.items.IndexOf(item); } }

        public void Insert(int index, T item) { lock (objLock) { this.items.Insert(index, item); } }

        public bool Remove(T item) { lock (objLock) { return this.items.Remove(item); } }

        public void RemoveAt(int index) { lock (objLock) { this.items.RemoveAt(index); } }

        IEnumerator IEnumerable.GetEnumerator() { lock (objLock) { return this.ToList().GetEnumerator(); } }

        public List<T> ToList() { lock (objLock) { return this.items.ToList(); } }

        public T PickRandom()
        {
            Random random = new Random();
            int index = random.Next(this.Count);
            return this[index];
        }
    }
}
