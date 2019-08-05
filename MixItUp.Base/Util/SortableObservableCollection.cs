using System;
using System.Collections.ObjectModel;

namespace MixItUp.Base.Util
{
    public class SortableObservableCollection<T> : ObservableCollection<T> where T : IComparable<T>
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
