using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.WPF.Windows.Overlay;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for OverlayWidgetsControl.xaml
    /// </summary>
    public partial class OverlayWidgetsControl : MainControlBase
    {
        private ObservableCollection<OverlayWidget> widgets = new ObservableCollection<OverlayWidget>();

        public OverlayWidgetsControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.OverlayWidgetsListView.ItemsSource = this.widgets;

            this.RefreshList();

            return base.InitializeInternal();
        }

        protected override Task OnVisibilityChanged()
        {
            this.NoOverlayGrid.Visibility = (ChannelSession.Settings.EnableOverlay) ? Visibility.Collapsed : Visibility.Visible;
            this.MainGrid.Visibility = (ChannelSession.Settings.EnableOverlay) ? Visibility.Visible : Visibility.Collapsed;
            return Task.FromResult(0);
        }

        private void RefreshList()
        {
            this.OverlayWidgetsListView.SelectedIndex = -1;

            this.widgets.Clear();
            foreach (OverlayWidget widget in ChannelSession.Settings.OverlayWidgets.OrderBy(c => c.OverlayName).ThenBy(c => c.Name))
            {
                this.widgets.Add(widget);
            }
        }

        private async Task HideWidget(OverlayWidget widget)
        {
            IOverlayService overlay = ChannelSession.Services.OverlayServers.GetOverlay(widget.OverlayName);
            if (overlay != null)
            {
                await overlay.RemoveItem(widget.Item);
            }
        }

        private async void Window_Closed(object sender, System.EventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                this.RefreshList();
                return Task.FromResult(0);
            });
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            OverlayWidget widget = (OverlayWidget)button.DataContext;
            if (widget != null)
            {
                OverlayWidgetEditorWindow window = new OverlayWidgetEditorWindow(widget);
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                Button button = (Button)sender;
                OverlayWidget widget = (OverlayWidget)button.DataContext;
                if (widget != null)
                {
                    ChannelSession.Settings.OverlayWidgets.Remove(widget);
                    await this.HideWidget(widget);

                    await ChannelSession.SaveSettings();
                    this.RefreshList();
                }
            });
        }

        private async void EnableDisableToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            OverlayWidget widget = (OverlayWidget)button.DataContext;
            if (widget != null)
            {
                widget.IsEnabled = button.IsChecked.GetValueOrDefault();
                if (!widget.IsEnabled)
                {
                    await this.Window.RunAsyncOperation(async () =>
                    {
                        await this.HideWidget(widget);
                    });
                }
            }
        }

        private void AddOverlayWidgetButton_Click(object sender, RoutedEventArgs e)
        {
            OverlayWidgetEditorWindow window = new OverlayWidgetEditorWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private void EnableDisableToggleSwitch_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            OverlayWidget widget = (OverlayWidget)button.DataContext;
            if (widget != null && widget.IsEnabled)
            {
                button.IsChecked = true;
            }
        }
    }
}
