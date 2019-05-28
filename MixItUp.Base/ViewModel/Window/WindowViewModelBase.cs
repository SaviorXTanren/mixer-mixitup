using MixItUp.Base.ViewModels;
using System;

namespace MixItUp.Base.ViewModel.Window
{
    public class WindowViewModelBase : UIViewModelBase
    {
        public event EventHandler StartLoadingOperationOccurred;
        public event EventHandler EndLoadingOperationOccurred;

        public override void StartLoadingOperation()
        {
            base.StartLoadingOperation();
            this.StartLoadingOperationOccurred?.Invoke(this, new EventArgs());
        }

        public override void EndLoadingOperation()
        {
            base.EndLoadingOperation();
            this.EndLoadingOperationOccurred?.Invoke(this, new EventArgs());
        }
    }
}
