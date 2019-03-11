using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Remote.Items;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Remote.Items
{
    public abstract class RemoteItemControlViewModelBase : UIViewModelBase
    {
        public const string RemoteDeleteItemEventName = "RemoteDeleteItem";

        public RemoteItemControlViewModelBase(RemoteItemViewModelBase item)
        {
            this.item = item;

            this.DeleteCommand = this.CreateCommand((parameter) =>
            {
                MessageCenter.Send<RemoteItemViewModelBase>(RemoteItemControlViewModelBase.RemoteDeleteItemEventName, this.Item);
                return Task.FromResult(0);
            });
        }

        public ICommand DeleteCommand { get; private set; }

        public RemoteItemViewModelBase Item
        {
            get { return this.item; }
            set
            {
                this.item = value;
                this.NotifyPropertyChanged();
            }
        }
        private RemoteItemViewModelBase item;

        public IEnumerable<string> PreDefinedColors { get { return ColorSchemes.WPFColorSchemeDictionary; } }

        public T GetTypedItem<T>() where T : RemoteItemViewModelBase { return (T)this.Item; }
    }
}
