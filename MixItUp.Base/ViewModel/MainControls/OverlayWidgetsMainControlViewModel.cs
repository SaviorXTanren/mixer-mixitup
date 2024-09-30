using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class OverlayWidgetViewModel : UIViewModelBase
    {
        public OverlayWidgetV3Model Widget { get; set; }

        public OverlayItemV3ModelBase Item { get { return this.Widget.Item; } }

        public Guid ID { get { return this.Widget.ID; } }

        public string Name { get { return this.Widget.Name; } }

        public bool IsSingleWidgetURL { get { return this.Widget.Item.DisplayOption == OverlayItemV3DisplayOptionsType.SingleWidgetURL; } }

        public string SingleWidgetURL { get { return this.Widget.SingleWidgetURL; } }

        public bool IsResettable { get { return this.Widget.IsResettable; } }

        public bool IsEnabled
        {
            get { return this.Widget.IsEnabled; }
            set
            {
                if (this.Widget.IsEnabled)
                {
                    this.Widget.Disable().Wait();
                }
                else
                {
                    this.Widget.Enable().Wait();
                }
                this.NotifyPropertyChanged();
            }
        }

        public OverlayWidgetViewModel(OverlayWidgetV3Model widget)
        {
            this.Widget = widget;
        }

        public async Task Reset() { await this.Widget.Reset(); }
    }

    public class OverlayWidgetsMainControlViewModel : WindowControlViewModelBase
    {
        public bool OverlayEnabled { get { return ChannelSession.Settings.EnableOverlay; } }
        public bool OverlayNotEnabled { get { return !this.OverlayEnabled; } }

        public ThreadSafeObservableCollection<OverlayWidgetViewModel> OverlayWidgets { get; private set; } = new ThreadSafeObservableCollection<OverlayWidgetViewModel>();

        public OverlayWidgetsMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        { }

        public void Refresh()
        {
            this.OverlayWidgets.ClearAndAddRange(ServiceManager.Get<OverlayV3Service>().GetWidgets().Select(w => new OverlayWidgetViewModel(w)).OrderBy(w => w.Name));
            this.NotifyPropertyChanges();
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

        protected override async Task OnOpenInternal()
        {
            this.Refresh();
            await base.OnVisibleInternal();
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
