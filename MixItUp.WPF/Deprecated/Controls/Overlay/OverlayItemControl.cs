using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Overlay;
using System;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Overlay
{
    [Obsolete]
    public abstract class OverlayItemControl : LoadingControlBase
    {
        protected OverlayItemViewModelBase ViewModel { get; set; }

        public OverlayItemControl()
        {
            this.DataContextChanged += OverlayItemControl_DataContextChanged;
        }

        public OverlayItemViewModelBase GetViewModel() { return this.ViewModel; }

        public OverlayItemModelBase GetItem() { return (this.ViewModel != null) ? this.ViewModel.GetOverlayItem() : null; }

        protected void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ServiceManager.Get<IProcessService>().LaunchLink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        protected override async Task OnLoaded()
        {
            if (this.DataContext is OverlayItemViewModelBase)
            {
                this.ViewModel = (OverlayItemViewModelBase)this.DataContext;
            }
            else if (this.ViewModel != null)
            {
                this.DataContext = this.ViewModel;
            }

            if (this.ViewModel != null)
            {
                await this.ViewModel.OnOpen();
            }
        }

        private async void OverlayItemControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (this.IsLoaded && this.ViewModel != null)
            {
                await this.ViewModel.OnOpen();
            }
        }
    }
}
