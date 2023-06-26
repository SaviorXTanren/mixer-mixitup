using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class OverlayWidgetViewModel : UIViewModelBase
    {
        public OverlayWidgetV3Model Widget { get; set; }

        public OverlayItemV3ModelBase Item { get { return this.Widget.Item; } }

        public string Name { get { return this.Item.Name; } }

        public string OverlayName
        {
            get
            {
                OverlayEndpointV3Model endpointService = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoint(this.Item.OverlayEndpointID);
                if (endpointService != null)
                {
                    return endpointService.Name;
                }
                return null;
            }
        }

        public bool IsEnabled
        {
            get { return this.Widget.IsEnabled; }
            set
            {
                this.Widget.IsEnabled = value;
                this.NotifyPropertyChanged();
            }
        }

        public OverlayWidgetViewModel(OverlayWidgetV3Model widget)
        {
            this.Widget = widget;
        }
    }

    public class OverlayWidgetsMainControlViewModel : WindowControlViewModelBase
    {
        public bool OverlayEnabled { get { return ChannelSession.Settings.EnableOverlay; } }
        public bool OverlayNotEnabled { get { return !this.OverlayEnabled; } }

        public ThreadSafeObservableCollection<OverlayWidgetViewModel> OverlayWidgets { get; private set; } = new ThreadSafeObservableCollection<OverlayWidgetViewModel>();

        public OverlayWidgetsMainControlViewModel(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        public void Refresh()
        {
            this.OverlayWidgets.ClearAndAddRange(ServiceManager.Get<OverlayV3Service>().GetWidgets().Select(w => new OverlayWidgetViewModel(w)));
            this.NotifyPropertyChanges();
        }

        public async Task PlayWidget(OverlayWidgetViewModel widget)
        {
            await widget.Widget.Test();
        }

        public async Task DeleteWidget(OverlayWidgetViewModel widget)
        {
            if (widget != null)
            {
                this.OverlayWidgets.Remove(widget);
                await ServiceManager.Get<OverlayV3Service>().RemoveWidget(widget.Widget);
                await ChannelSession.SaveSettings();
            }
        }

        public async Task EnableWidget(OverlayWidgetViewModel widget)
        {
            if (widget != null && !widget.IsEnabled)
            {
                await widget.Widget.Enable();
            }
        }

        public async Task DisableWidget(OverlayWidgetViewModel widget)
        {
            if (widget != null && widget.IsEnabled)
            {
                await widget.Widget.Disable();
            }
        }

        protected override Task OnOpenInternal()
        {
            this.Refresh();
            return base.OnVisibleInternal();
        }

        protected override Task OnVisibleInternal()
        {
            this.Refresh();
            return base.OnVisibleInternal();
        }

        private void NotifyPropertyChanges()
        {
            this.NotifyPropertyChanged("OverlayEnabled");
            this.NotifyPropertyChanged("OverlayNotEnabled");
        }
    }
}
