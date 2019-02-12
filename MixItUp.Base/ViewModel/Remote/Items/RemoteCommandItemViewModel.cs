using MixItUp.Base.Remote.Models.Items;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public class RemoteCommandItemViewModel : RemoteButtonItemViewModelBase
    {
        private new RemoteCommandItemModel model;

        public RemoteCommandItemViewModel(RemoteCommandItemModel model) : base(model) { this.model = model; }

        public override bool IsCommand { get { return true; } }
    }
}
