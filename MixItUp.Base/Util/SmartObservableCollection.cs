using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public class SmartObservableCollection<T> : ObservableCollection<T>
    {
        public SmartObservableCollection()
            : base()
        {
        }

        public SmartObservableCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public SmartObservableCollection(List<T> list)
            : base(list)
        {
        }

        public async Task AddRange(IEnumerable<T> range)
        {
            lock (this.Items)
            {
                foreach (var item in range)
                {
                    this.Items.Add(item);
                }
            }

            await DispatcherHelper.InvokeDispatcher(() =>
            {
                this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                return Task.FromResult(0);
            });
        }

        public async Task Reset(IEnumerable<T> range)
        {
            lock (this.Items)
            {
                this.Items.Clear();
            }

            await AddRange(range);
        }
    }
}
