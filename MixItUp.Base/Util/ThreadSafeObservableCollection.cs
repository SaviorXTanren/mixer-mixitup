using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace MixItUp.Base.Util
{
    public class ThreadSafeObservableCollection<T> : ObservableCollection<T>
    {
        private object updateLock = new object();

        public ThreadSafeObservableCollection()
        {
            BindingOperations.EnableCollectionSynchronization(this, updateLock);
        }

        public new void Add(T item)
        {
            lock (updateLock)
            {
                base.Add(item);
            }
        }

        public new void Clear()
        {
            lock (updateLock)
            {
                base.Clear();
            }
        }

        public new IEnumerator<T> GetEnumerator()
        {
            lock (updateLock)
            {
                return this.ToList().GetEnumerator();
            }
        }

        public new void Insert(int index, T item)
        {
            lock (updateLock)
            {
                base.Insert(index, item);
            }
        }

        public new bool Remove(T item)
        {
            bool result = false;
            DispatcherHelper.Dispatcher.Invoke(() =>
            {
                result = base.Remove(item);
            });
            return result;
        }
    }
}
