using System;

namespace MixItUp.Base.Util
{
    public class SortableObservableCollection<T> : ThreadSafeObservableCollection<T> where T : IComparable<T>
    {
        public void SortedInsert(T newItem)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].CompareTo(newItem) >= 0)
                {
                    this.Insert(i, newItem);
                    return;
                }
            }
            this.Add(newItem);
        }
    }
}
