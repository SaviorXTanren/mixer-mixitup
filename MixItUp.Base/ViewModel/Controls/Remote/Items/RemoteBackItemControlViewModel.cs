using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Remote;
using MixItUp.Base.ViewModel.Remote.Items;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Remote.Items
{
    public class RemoteBackItemControlViewModel : RemoteItemControlViewModelBase
    {
        public const string RemoteBackNavigationEventName = "RemoteBackNavigation";

        public RemoteBackItemControlViewModel(RemoteBackItemViewModel item, RemoteBoardViewModel parentBoard)
            : base(item)
        {
            this.ParentBoard = parentBoard;

            this.BackNavigationCommand = this.CreateCommand((parameter) =>
            {
                MessageCenter.Send<RemoteBoardViewModel>(RemoteBackItemControlViewModel.RemoteBackNavigationEventName, this.ParentBoard);
                return Task.FromResult(0);
            });
        }

        public RemoteBoardViewModel ParentBoard { get; private set; }

        public ICommand BackNavigationCommand { get; private set; }
    }
}
