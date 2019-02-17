using MixItUp.Base.Remote.Models.Items;
using MixItUp.Base.Util;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public class RemoteFolderItemViewModel : RemoteButtonItemViewModelBase
    {
        public const string NewRemoteFolderEventName = "NewRemoteFolder";
        public const string RemoteFolderDetailsEventName = "RemoteFolderDetails";
        public const string RemoteFolderNavigationEventName = "RemoteFolderNavigation";

        private new RemoteFolderItemModel model;

        public RemoteFolderItemViewModel(string name, int xPosition, int yPosition)
            : this(new RemoteFolderItemModel(xPosition, yPosition))
        {
            this.Name = name;
        }

        public RemoteFolderItemViewModel(RemoteFolderItemModel model)
            : base(model)
        {
            this.model = model;

            this.FolderSelectedCommand = this.CreateCommand((parameter) =>
            {
                MessageCenter.Send<RemoteFolderItemViewModel>(RemoteFolderItemViewModel.RemoteFolderDetailsEventName, this);
                return Task.FromResult(0);
            });

            this.FolderNavigationCommand = this.CreateCommand((parameter) =>
            {
                MessageCenter.Send<RemoteFolderItemViewModel>(RemoteFolderItemViewModel.RemoteFolderNavigationEventName, this);
                return Task.FromResult(0);
            });
        }

        public RemoteBoardViewModel Board { get { return new RemoteBoardViewModel(this.model.Board); } }

        public override bool IsFolder { get { return true; } }

        public ICommand FolderSelectedCommand { get; private set; }
        public ICommand FolderNavigationCommand { get; private set; }
    }
}
