using MixItUp.Base.Remote.Models.Items;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public class RemoteFolderItemViewModel : RemoteButtonItemViewModelBase
    {
        public const string NewRemoteFolderEventName = "NewRemoteFolder";
        public const string RemoteFolderDetailsEventName = "RemoteFolderDetails";

        private new RemoteFolderItemModel model;

        public RemoteFolderItemViewModel(string name, int xPosition, int yPosition)
            : this(new RemoteFolderItemModel(xPosition, yPosition))
        {
            this.Name = name;
        }

        public RemoteFolderItemViewModel(RemoteFolderItemModel model) : base(model) { this.model = model; }

        public RemoteBoardViewModel Board { get { return new RemoteBoardViewModel(this.model.Board); } }

        public override bool IsFolder { get { return true; } }
    }
}
