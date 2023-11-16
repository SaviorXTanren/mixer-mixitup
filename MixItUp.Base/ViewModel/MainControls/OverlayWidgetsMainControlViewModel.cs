using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class OverlayWidgetsMainControlViewModel : WindowControlViewModelBase
    {
        public bool OverlayEnabled { get { return ChannelSession.Settings.EnableOverlay; } }
        public bool OverlayNotEnabled { get { return !this.OverlayEnabled; } }

        public ObservableCollection<OverlayWidgetModel> OverlayWidgets { get; private set; } = new ObservableCollection<OverlayWidgetModel>();

        public OverlayWidgetsMainControlViewModel(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        public void Refresh()
        {
            List<OverlayWidgetModel> toBeRemoved = new List<OverlayWidgetModel>();
            List<OverlayWidgetModel> widgets = new List<OverlayWidgetModel>();
            foreach (OverlayWidgetModel widget in ChannelSession.Settings.OverlayWidgets.OrderBy(c => c.OverlayName).ThenBy(c => c.Name))
            {
                if (EnumHelper.IsObsolete(widget.Item.ItemType))
                {
                    toBeRemoved.Add(widget);
                }
                else
                {
                    widgets.Add(widget);
                }
            }

            this.OverlayWidgets.ClearAndAddRange(widgets);

            foreach (OverlayWidgetModel widget in toBeRemoved)
            {
                ChannelSession.Settings.OverlayWidgets.Remove(widget);
            }

            this.NotifyPropertyChanges();
        }

        public async Task PlayWidget(OverlayWidgetModel widget)
        {
            if (widget != null && widget.SupportsTestData)
            {
                await widget.HideItem();

                await widget.LoadTestData();

                await Task.Delay(5000);

                await widget.HideItem();
            }
        }

        public async Task DeleteWidget(OverlayWidgetModel widget)
        {
            if (widget != null)
            {
                await widget.Disable();
                ChannelSession.Settings.OverlayWidgets.Remove(widget);
                await ChannelSession.SaveSettings();
            }
        }

        public async Task EnableWidget(OverlayWidgetModel widget)
        {
            if (widget != null && !widget.IsEnabled)
            {
                await widget.Enable();
            }
        }

        public async Task DisableWidget(OverlayWidgetModel widget)
        {
            if (widget != null && widget.IsEnabled)
            {
                await widget.Disable();
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
