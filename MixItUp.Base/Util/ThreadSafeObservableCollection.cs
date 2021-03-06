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

        public async Task AddAsync(T item)
        {
            await DispatcherHelper.InvokeDispatcher(() =>
            {
                base.Add(item);
                return Task.FromResult(0);
            });
        }

        public async Task ClearAsync()
        {
            await DispatcherHelper.InvokeDispatcher(() =>
            {
                base.Clear();
                return Task.FromResult(0);
            });
        }

        public async Task InsertAsync(int index, T item)
        {
            await DispatcherHelper.InvokeDispatcher(() =>
            {
                base.Insert(index, item);
                return Task.FromResult(0);
            });
        }

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
