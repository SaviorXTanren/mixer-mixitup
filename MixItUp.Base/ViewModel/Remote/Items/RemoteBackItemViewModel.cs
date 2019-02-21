using MixItUp.Base.Remote.Models.Items;
using MixItUp.Base.Util;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public class RemoteBackItemViewModel : RemoteButtonItemViewModelBase
    {
        public const string RemoteBackNavigationEventName = "RemoteBackNavigation";

        public RemoteBackItemViewModel(RemoteBoardViewModel parentBoard)
            : base(new RemoteBackItemModel())
        {
            this.ParentBoard = parentBoard;

            this.BackNavigationCommand = this.CreateCommand((parameter) =>
            {
                MessageCenter.Send<RemoteBoardViewModel>(RemoteBackItemViewModel.RemoteBackNavigationEventName, this.ParentBoard);
                return Task.FromResult(0);
            });
        }

        public RemoteBoardViewModel ParentBoard { get; private set; }

        public override bool IsBack { get { return true; } }

        public ICommand BackNavigationCommand { get; private set; }
    }
}
