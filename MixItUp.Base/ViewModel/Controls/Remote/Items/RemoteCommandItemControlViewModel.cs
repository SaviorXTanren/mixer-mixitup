using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Remote.Items;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Remote.Items
{
    public class RemoteCommandItemControlViewModel : RemoteItemControlViewModelBase
    {
        public const string RemoteCommandDetailsEventName = "RemoteCommandDetails";

        public RemoteCommandItemControlViewModel(RemoteCommandItemViewModel item)
            : base(item)
        {
            this.CommandSelectedCommand = this.CreateCommand((parameter) =>
            {
                MessageCenter.Send<RemoteCommandItemViewModel>(RemoteCommandItemControlViewModel.RemoteCommandDetailsEventName, this.GetTypedItem<RemoteCommandItemViewModel>());
                return Task.FromResult(0);
            });
        }

        public ICommand CommandSelectedCommand { get; private set; }
    }
}
