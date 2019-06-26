using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
using MixItUp.WPF.Util;
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
        private ObservableCollection<OverlayWidgetModel> widgets = new ObservableCollection<OverlayWidgetModel>();

        public OverlayWidgetsControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.OverlayWidgetsListView.ItemsSource = this.widgets;

            this.RefreshTimeTextBox.Text = ChannelSession.Settings.OverlayWidgetRefreshTime.ToString();

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
            foreach (OverlayWidgetModel widget in ChannelSession.Settings.OverlayWidgets.OrderBy(c => c.OverlayName).ThenBy(c => c.Name))
            {
                this.widgets.Add(widget);
            }
        }

        private async Task HideWidget(OverlayWidgetModel widget)
        {
            await widget.HideItem();
        }

        private async void Window_Closed(object sender, System.EventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                this.RefreshList();
                return Task.FromResult(0);
            });
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                Button button = (Button)sender;
                OverlayWidgetModel widget = (OverlayWidgetModel)button.DataContext;
                if (widget != null && widget.SupportsTestData)
                {
                    await this.HideWidget(widget);

                    await widget.LoadTestData();

                    await Task.Delay(3000);

                    await this.HideWidget(widget);
                }
            });
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            OverlayWidgetModel widget = (OverlayWidgetModel)button.DataContext;
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
                if (await MessageBoxHelper.ShowConfirmationDialog("Are you sure you want to delete this widget?"))
                {
                    Button button = (Button)sender;
                    OverlayWidgetModel widget = (OverlayWidgetModel)button.DataContext;
                    if (widget != null)
                    {
                        ChannelSession.Settings.OverlayWidgets.Remove(widget);
                        await this.HideWidget(widget);

                        await ChannelSession.SaveSettings();
                        this.RefreshList();
                    }
                }
            });
        }

        private async void EnableDisableToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            OverlayWidgetModel widget = (OverlayWidgetModel)button.DataContext;
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

        private void EnableDisableToggleSwitch_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            OverlayWidgetModel widget = (OverlayWidgetModel)button.DataContext;
            if (widget != null && widget.IsEnabled)
            {
                button.IsChecked = true;
            }
        }

        private async void RefreshTimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int value;
            if (int.TryParse(this.RefreshTimeTextBox.Text, out value) && value > 0)
            {
                ChannelSession.Settings.OverlayWidgetRefreshTime = value;
            }
            else
            {
                await MessageBoxHelper.ShowMessageDialog("Refresh Interval must be greater than 0");
                this.RefreshTimeTextBox.Text = ChannelSession.Settings.OverlayWidgetRefreshTime.ToString();
            }
        }

        private void AddOverlayWidgetButton_Click(object sender, RoutedEventArgs e)
        {
            OverlayWidgetEditorWindow window = new OverlayWidgetEditorWindow();
            window.Closed += Window_Closed;
            window.Show();
        }
    }
}
