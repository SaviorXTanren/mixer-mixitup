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
        public OverlayItemV3ModelBase Item { get; set; }

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
            get { return this.Item.IsEnabled; }
            set
            {
                this.Item.IsEnabled = value;
                this.NotifyPropertyChanged();
            }
        }

        public OverlayWidgetViewModel(OverlayItemV3ModelBase item)
        {
            this.Item = item;
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
            this.OverlayWidgets.ClearAndAddRange(ChannelSession.Settings.OverlayWidgetsV3.Select(w => new OverlayWidgetViewModel(w)));

            this.NotifyPropertyChanges();
        }

        public async Task PlayWidget(OverlayWidgetViewModel widget)
        {
            //if (widget != null && widget.SupportsTestData)
            //{
            //    await widget.HideItem();

            //    await widget.LoadTestData();

            //    await Task.Delay(5000);

            //    await widget.HideItem();
            //}
        }

        public async Task DeleteWidget(OverlayWidgetViewModel widget)
        {
            if (widget != null)
            {
                this.OverlayWidgets.Remove(widget);
                //await ServiceManager.Get<OverlayV3Service>().RemoveOverlayWidget(widget.Item);
                await ChannelSession.SaveSettings();
            }
        }

        public async Task EnableWidget(OverlayWidgetViewModel widget)
        {
            if (widget != null && !widget.Item.IsEnabled)
            {
                await widget.Item.Enable();
            }
        }

        public async Task DisableWidget(OverlayWidgetViewModel widget)
        {
            if (widget != null && widget.Item.IsEnabled)
            {
                await widget.Item.Disable();
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
