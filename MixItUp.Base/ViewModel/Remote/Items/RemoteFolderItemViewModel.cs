using MixItUp.Base.Remote.Models.Items;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public class RemoteFolderItemViewModel : RemoteItemViewModelBase
    {
        private new RemoteFolderItemModel model;

        public RemoteFolderItemViewModel(RemoteFolderItemModel model) : base(model) { this.model = model; }

        public RemoteBoardViewModel Board { get { return new RemoteBoardViewModel(this.model.Board); } }

        public override bool IsFolder { get { return true; } }
    }
}
