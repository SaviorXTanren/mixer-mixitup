using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class CommunityCommandsMainControlViewModel : WindowControlViewModelBase
    {
        public CommunityCommandsMainControlViewModel(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        protected override async Task OnLoadedInternal()
        {
            await base.OnVisibleInternal();
        }

        protected override async Task OnVisibleInternal()
        {
            await base.OnVisibleInternal();
        }
    }
}
