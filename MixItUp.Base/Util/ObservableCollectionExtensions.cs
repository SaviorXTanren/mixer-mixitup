using System.Collections.ObjectModel;
using System.Windows.Data;

namespace MixItUp.Base.Util
{
    public static class ObservableCollectionExtensions
    {
        public static ObservableCollection<T> EnableSync<T>(this ObservableCollection<T> collection)
        {
            BindingOperations.EnableCollectionSynchronization(collection, collection);
            return collection;
        }
    }
}
