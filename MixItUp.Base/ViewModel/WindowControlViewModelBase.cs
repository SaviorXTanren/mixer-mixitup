using MixItUp.Base.ViewModels;

namespace MixItUp.Base.ViewModel
{
    public class WindowControlViewModelBase : ControlViewModelBase
    {
        public UIViewModelBase WindowViewModel { get; private set; }

        public WindowControlViewModelBase(UIViewModelBase windowViewModel)
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
