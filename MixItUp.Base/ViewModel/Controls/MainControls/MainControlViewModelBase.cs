using MixItUp.Base.ViewModel.Window;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class MainControlViewModelBase : ControlViewModelBase
    {
        public MainWindowViewModel WindowViewModel { get; private set; }

        public MainControlViewModelBase(MainWindowViewModel windowViewModel)
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
