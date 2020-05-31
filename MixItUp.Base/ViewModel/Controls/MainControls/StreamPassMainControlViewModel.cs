using MixItUp.Base.ViewModel.Window;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class StreamPassMainControlViewModel : WindowControlViewModelBase
    {
        public StreamPassMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {

        }

        public void Refresh()
        {

        }

        protected override Task OnLoadedInternal()
        {
            this.Refresh();
            return base.OnVisibleInternal();
        }

        protected override Task OnVisibleInternal()
        {
            this.Refresh();
            return base.OnVisibleInternal();
        }
    }
}
