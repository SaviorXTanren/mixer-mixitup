using MixItUp.Base.Remote.Models.Items;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public class RemoteBackItemViewModel : RemoteButtonItemViewModelBase
    {
        public RemoteBackItemViewModel() : base(new RemoteBackItemModel()) { }

        public override bool IsFolder { get { return true; } }
    }
}
