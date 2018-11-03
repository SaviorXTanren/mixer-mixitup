using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
using MixItUp.WPF.Windows.Overlay;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

        private void RefreshList()
        {
            this.OverlayWidgetsListView.SelectedIndex = -1;

            this.widgets.Clear();
            foreach (OverlayWidget widget in ChannelSession.Settings.OverlayWidgets.OrderBy(c => c.OverlayName))
            {
                this.widgets.Add(widget);
            }
        }

        private async Task RefreshWidgets()
        {
            await Task.Delay(1);
        }

        private async Task RefreshWidget(OverlayWidget widget)
        {
            await Task.Delay(1);
        }

        private async void Window_Closed(object sender, System.EventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                this.RefreshList();

                OverlayWidgetEditorWindow window = (OverlayWidgetEditorWindow)sender;
                if (window != null && window.Widget != null)
                {
                    await this.RefreshWidget(window.Widget);
                }
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
                    await ChannelSession.SaveSettings();
                    this.RefreshList();
                    await this.RefreshWidget(widget);
                }
            });
        }

        private async void EnableDisableToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                Button button = (Button)sender;
                OverlayWidget widget = (OverlayWidget)button.DataContext;
                if (widget != null)
                {
                    widget.IsEnabled = !widget.IsEnabled;
                    await ChannelSession.SaveSettings();
                    this.RefreshList();
                    await this.RefreshWidget(widget);
                }
            });
        }

        private void AddOverlayWidgetButton_Click(object sender, RoutedEventArgs e)
        {
            OverlayWidgetEditorWindow window = new OverlayWidgetEditorWindow();
            window.Closed += Window_Closed;
            window.Show();
        }
    }
}
