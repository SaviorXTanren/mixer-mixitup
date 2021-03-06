using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public class ThreadSafeObservableCollection<T> : ObservableCollection<T>
    {
        public ThreadSafeObservableCollection() { }

        public ThreadSafeObservableCollection(IEnumerable<T> collection) : base(collection) { }

        public ThreadSafeObservableCollection(List<T> list) : base(list) { }

        public new void Add(T item) { base.Add(item); }

        public async Task AddAsync(T item)
        {
            await DispatcherHelper.InvokeDispatcher(() =>
            {
                base.Add(item);
                return Task.FromResult(0);
            });
        }

        public new void Clear() { base.Clear(); }

        public async Task ClearAsync()
        {
            await DispatcherHelper.InvokeDispatcher(() =>
            {
                base.Clear();
                return Task.FromResult(0);
            });
        }

        public new IEnumerator<T> GetEnumerator() { return base.GetEnumerator(); }

        public async Task<IEnumerator<T>> GetEnumeratorAsync()
        {
            IEnumerator<T> result = null;
            await DispatcherHelper.InvokeDispatcher(() =>
            {
                result = base.GetEnumerator();
                return Task.FromResult(0);
            });
            return result;
        }

        public new int IndexOf(T item) { return base.IndexOf(item); }

        public async Task<int> IndexOfAsync(T item)
        {
            int index = -1;
            await DispatcherHelper.InvokeDispatcher(() =>
            {
                index = base.IndexOf(item);
                return Task.FromResult(0);
            });
            return index;
        }

        public new void Insert(int index, T item) { base.Insert(index, item); }

        public async Task InsertAsync(int index, T item)
        {
            await DispatcherHelper.InvokeDispatcher(() =>
            {
                base.Insert(index, item);
                return Task.FromResult(0);
            });
        }

        public new bool Remove(T item) { return base.Remove(item); }

        public async Task<bool> RemoveAsync(T item)
        {
            bool result = false;
            await DispatcherHelper.InvokeDispatcher(() =>
            {
                result = base.Remove(item);
                return Task.FromResult(0);
            });
            return result;
        }

        public void AddRange(IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                base.Add(item);
            }
        }

        public async Task AddRangeAsync(IEnumerable<T> range)
        {
            await DispatcherHelper.InvokeDispatcher(() =>
            {
                foreach (var item in range)
                {
                    base.Add(item);
                }

                this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                return Task.FromResult(0);
            });
        }
    }
}
