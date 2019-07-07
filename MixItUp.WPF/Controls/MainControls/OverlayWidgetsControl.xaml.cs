using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Overlay;
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
        private OverlayWidgetsMainControlViewModel viewModel;

        public OverlayWidgetsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new OverlayWidgetsMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnLoaded();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }

        private async void Window_Closed(object sender, System.EventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                await this.viewModel.OnVisible();
                return Task.FromResult(0);
            });
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                Button button = (Button)sender;
                OverlayWidgetModel widget = (OverlayWidgetModel)button.DataContext;
                await this.viewModel.PlayWidget(widget);
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
                    await this.viewModel.DeleteWidget(widget);
                    await this.viewModel.OnVisible();
                }
            });
        }

        private async void EnableDisableToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            OverlayWidgetModel widget = (OverlayWidgetModel)button.DataContext;
            await this.viewModel.EnableWidget(widget);
        }

        private async void EnableDisableToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            OverlayWidgetModel widget = (OverlayWidgetModel)button.DataContext;
            await this.viewModel.DisableWidget(widget);
        }

        private void AddOverlayWidgetButton_Click(object sender, RoutedEventArgs e)
        {
            OverlayWidgetEditorWindow window = new OverlayWidgetEditorWindow();
            window.Closed += Window_Closed;
            window.Show();
        }
    }
}
