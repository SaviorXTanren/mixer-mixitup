using MixItUp.Base;
using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.ViewModel.Controls.MainControls;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dashboard
{
    /// <summary>
    /// Interaction logic for SongRequestsDashboardControl.xaml
    /// </summary>
    public partial class SongRequestsDashboardControl : DashboardControlBase
    {
        private SongRequestsMainControlViewModel viewModel;

        public SongRequestsDashboardControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new SongRequestsMainControlViewModel(this.Window.ViewModel);
            await this.viewModel.OnLoaded();

            await base.InitializeInternal();
        }

        private async void VolumeSlider_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (ChannelSession.Services.SongRequestService != null)
                {
                    await ChannelSession.Services.SongRequestService.RefreshVolume();
                }
            });
        }

        private async void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                Button button = (Button)sender;
                if (button != null && button.DataContext != null && button.DataContext is SongRequestModel)
                {
                    SongRequestModel songRequest = (SongRequestModel)button.DataContext;
                    this.viewModel.MoveUpCommand.Execute(songRequest);
                }
                return Task.FromResult(0);
            });
        }

        private async void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                Button button = (Button)sender;
                if (button != null && button.DataContext != null && button.DataContext is SongRequestModel)
                {
                    SongRequestModel songRequest = (SongRequestModel)button.DataContext;
                    this.viewModel.MoveDownCommand.Execute(songRequest);
                }
                return Task.FromResult(0);
            });
        }

        private async void DeleteQueueButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                Button button = (Button)sender;
                if (button != null && button.DataContext != null && button.DataContext is SongRequestModel)
                {
                    SongRequestModel songRequest = (SongRequestModel)button.DataContext;
                    this.viewModel.DeleteCommand.Execute(songRequest);
                }
                return Task.FromResult(0);
            });
        }
    }
}
