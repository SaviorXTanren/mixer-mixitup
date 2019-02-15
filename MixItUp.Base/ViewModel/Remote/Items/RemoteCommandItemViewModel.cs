using MixItUp.Base.Remote.Models.Items;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public class RemoteCommandItemViewModel : RemoteButtonItemViewModelBase
    {
        public const string NewRemoteCommandEventName = "NewRemoteCommand";
        public const string RemoteCommandDetailsEventName = "RemoteCommandDetails";

        private new RemoteCommandItemModel model;

        public RemoteCommandItemViewModel(string name, int xPosition, int yPosition)
            : this(new RemoteCommandItemModel(xPosition, yPosition))
        {
            this.Name = name;
        }

        public RemoteCommandItemViewModel(RemoteCommandItemModel model) : base(model) { this.model = model; }

        public override bool IsCommand { get { return true; } }
    }
}
