using MixItUp.Base.ViewModel.Window;

namespace MixItUp.Base.ViewModel.Controls
{
    public class WindowControlViewModelBase : ControlViewModelBase
    {
        public WindowViewModelBase WindowViewModel { get; private set; }

        public WindowControlViewModelBase(WindowViewModelBase windowViewModel)
        {
            this.WindowViewModel = windowViewModel;
        }

        public override void StartLoadingOperation()
        {
            base.StartLoadingOperation();
            this.WindowViewModel.StartLoadingOperation();
        }

        public override void EndLoadingOperation()
        {
            base.EndLoadingOperation();
            this.WindowViewModel.EndLoadingOperation();
        }
    }
}
