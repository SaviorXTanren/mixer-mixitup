using MixItUp.Base.Remote.Models.Items;
using MixItUp.Base.Util;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public class RemoteEmptyItemViewModel : RemoteButtonItemViewModelBase
    {
        public class RemoteEmptyItemModel : RemoteButtonItemModelBase
        {
            public RemoteEmptyItemModel(int xPosition, int yPosition) : base(xPosition, yPosition) { }
        }

        public ICommand AddCommandCommand { get; private set; }
        public ICommand AddFolderCommand { get; private set; }

        public RemoteEmptyItemViewModel(int xPosition, int yPosition)
            : base(new RemoteEmptyItemModel(xPosition, yPosition))
        {
            this.AddCommandCommand = this.CreateCommand(async (x) =>
            {
                string name = await DialogHelper.ShowTextEntry("Name of Button:");
                if (!string.IsNullOrEmpty(name))
                {
                    MessageCenter.Send(RemoteCommandItemViewModel.NewRemoteCommandEventName, new RemoteCommandItemViewModel(name, this.model.XPosition, this.model.YPosition));
                }
            });

            this.AddFolderCommand = this.CreateCommand(async (x) =>
            {
                string name = await DialogHelper.ShowTextEntry("Name of Folder:");
                if (!string.IsNullOrEmpty(name))
                {
                    MessageCenter.Send(RemoteFolderItemViewModel.NewRemoteFolderEventName, new RemoteFolderItemViewModel(name, this.model.XPosition, this.model.YPosition));
                }
            });
        }

        public override bool IsEmpty { get { return true; } }
    }
}
