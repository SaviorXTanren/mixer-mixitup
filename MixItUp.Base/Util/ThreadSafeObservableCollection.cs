using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MixItUp.Base.Util
{
    public class ThreadSafeObservableCollection<T> : ObservableCollection<T>
    {
        public ThreadSafeObservableCollection() { }

        public new T this[int index]
        {
            get { return base[index]; }
            set
            {
                DispatcherHelper.Dispatcher.Invoke(() =>
                {
                    base[index] = value;
                });
            }
        }

        public new void Add(T item)
        {
            DispatcherHelper.Dispatcher.Invoke(() =>
            {
                this.AddInternal(item);
            });
        }

        public new void Clear()
        {
            DispatcherHelper.Dispatcher.Invoke(base.Clear);
        }

        public new IEnumerator<T> GetEnumerator()
        {
            IEnumerator<T> result = null;
            DispatcherHelper.Dispatcher.Invoke(() =>
            {
                result = this.ToList().GetEnumerator();
            });
            return result;
        }

        public new int IndexOf(T item)
        {
            int index = -1;
            DispatcherHelper.Dispatcher.Invoke(() =>
            {
                index = base.IndexOf(item);
            });
            return index;
        }

        public new void Insert(int index, T item)
        {
            DispatcherHelper.Dispatcher.Invoke(() =>
            {
                this.InsertInternal(index, item);
            });
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

        public void AddRange(IEnumerable<T> range)
        {
            DispatcherHelper.Dispatcher.Invoke(() =>
            {
                this.AddRangeInternal(range);
            });
        }

        public void ClearAndAddRange(IEnumerable<T> range)
        {
            DispatcherHelper.Dispatcher.Invoke(() =>
            {
                base.Clear();
                this.AddRangeInternal(range);
            });
        }

        protected void AddInternal(T item) { base.Add(item); }

        protected void InsertInternal(int index, T item) { base.Insert(index, item); }

        private void AddRangeInternal(IEnumerable<T> range)
        {
            if (range != null)
            {
                foreach (var item in range.ToList())
                {
                    base.Add(item);
                }
            }

            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        }
    }
}
