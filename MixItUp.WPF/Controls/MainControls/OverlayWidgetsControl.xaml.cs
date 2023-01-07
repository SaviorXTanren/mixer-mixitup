using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Overlay;
using System.Threading.Tasks;
using System.Windows;

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
            await this.viewModel.OnOpen();
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
                return Task.CompletedTask;
            });
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                OverlayWidgetViewModel widget = FrameworkElementHelpers.GetDataContext<OverlayWidgetViewModel>(sender);
                await this.viewModel.PlayWidget(widget);
            });
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            OverlayWidgetViewModel widget = FrameworkElementHelpers.GetDataContext<OverlayWidgetViewModel>(sender);
            if (widget != null)
            {
                OverlayWidgetV3EditorWindow window = new OverlayWidgetV3EditorWindow(widget.Item);
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.DeleteWidgetPrompt))
                {
                    OverlayWidgetViewModel widget = FrameworkElementHelpers.GetDataContext<OverlayWidgetViewModel>(sender);
                    await this.viewModel.DeleteWidget(widget);
                    await this.viewModel.OnVisible();
                }
            });
        }

        private async void EnableDisableToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            await this.viewModel.EnableWidget(FrameworkElementHelpers.GetDataContext<OverlayWidgetViewModel>(sender));
        }

        private async void EnableDisableToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            await this.viewModel.DisableWidget(FrameworkElementHelpers.GetDataContext<OverlayWidgetViewModel>(sender));
        }

        private void AddOverlayWidgetButton_Click(object sender, RoutedEventArgs e)
        {
            OverlayWidgetV3EditorWindow window = new OverlayWidgetV3EditorWindow();
            window.Closed += Window_Closed;
            window.Show();
        }
    }
}
