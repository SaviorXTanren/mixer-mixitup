using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Remote.Items;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Remote.Items
{
    public class RemoteEmptyItemControlViewModel : RemoteItemControlViewModelBase
    {
        public const string NewRemoteCommandEventName = "NewRemoteCommand";
        public const string NewRemoteFolderEventName = "NewRemoteFolder";

        public RemoteEmptyItemControlViewModel(RemoteEmptyItemViewModel item)
            : base(item)
        {
            this.AddCommandCommand = this.CreateCommand(async (x) =>
            {
                string name = await DialogHelper.ShowTextEntry("Name of Button:");
                if (!string.IsNullOrEmpty(name))
                {
                    MessageCenter.Send(RemoteEmptyItemControlViewModel.NewRemoteCommandEventName, new RemoteCommandItemViewModel(name, this.GetTypedItem<RemoteEmptyItemViewModel>().XPosition, this.GetTypedItem<RemoteEmptyItemViewModel>().YPosition));
                }
            });

            this.AddFolderCommand = this.CreateCommand(async (x) =>
            {
                string name = await DialogHelper.ShowTextEntry("Name of Folder:");
                if (!string.IsNullOrEmpty(name))
                {
                    MessageCenter.Send(RemoteEmptyItemControlViewModel.NewRemoteFolderEventName, new RemoteFolderItemViewModel(name, this.GetTypedItem<RemoteEmptyItemViewModel>().XPosition, this.GetTypedItem<RemoteEmptyItemViewModel>().YPosition));
                }
            });
        }

        public ICommand AddCommandCommand { get; private set; }
        public ICommand AddFolderCommand { get; private set; }
    }
}
