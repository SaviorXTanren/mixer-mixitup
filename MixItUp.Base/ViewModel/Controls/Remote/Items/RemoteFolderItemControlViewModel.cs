using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Remote.Items;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Remote.Items
{
    public class RemoteFolderItemControlViewModel : RemoteItemControlViewModelBase
    {
        public const string RemoteFolderDetailsEventName = "RemoteFolderDetails";
        public const string RemoteFolderNavigationEventName = "RemoteFolderNavigation";

        public RemoteFolderItemControlViewModel(RemoteFolderItemViewModel item)
            : base(item)
        {
            this.FolderSelectedCommand = this.CreateCommand((parameter) =>
            {
                MessageCenter.Send<RemoteFolderItemViewModel>(RemoteFolderItemControlViewModel.RemoteFolderDetailsEventName, this.GetTypedItem<RemoteFolderItemViewModel>());
                return Task.FromResult(0);
            });

            this.FolderNavigationCommand = this.CreateCommand((parameter) =>
            {
                MessageCenter.Send<RemoteFolderItemViewModel>(RemoteFolderItemControlViewModel.RemoteFolderNavigationEventName, this.GetTypedItem<RemoteFolderItemViewModel>());
                return Task.FromResult(0);
            });
        }

        public ICommand FolderSelectedCommand { get; private set; }
        public ICommand FolderNavigationCommand { get; private set; }
    }
}
