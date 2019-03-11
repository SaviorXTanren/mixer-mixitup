using MixItUp.Base.Remote.Models.Items;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public class RemoteEmptyItemViewModel : RemoteButtonItemViewModelBase
    {
        public class RemoteEmptyItemModel : RemoteButtonItemModelBase
        {
            public RemoteEmptyItemModel(int xPosition, int yPosition) : base(xPosition, yPosition) { }
        }

        public RemoteEmptyItemViewModel(int xPosition, int yPosition) : base(new RemoteEmptyItemModel(xPosition, yPosition)) { }

        public override bool IsEmpty { get { return true; } }
    }
}
