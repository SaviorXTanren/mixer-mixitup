using MixItUp.Base;
using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for SongRequestControl.xaml
    /// </summary>
    public partial class SongRequestControl : MainControlBase
    {
        private static readonly string SpotifySetupTooltip = "To use Spotify with Song Requests, you must ensure Spotify is" + Environment.NewLine +
            "running on your computer and you have played at least one" + Environment.NewLine +
            "song in the Spotify app. This is required to be done everytime" + Environment.NewLine +
            "to let Spotify know that where to send our song requests to.";

        private SongRequestsMainControlViewModel viewModel;

        public SongRequestControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new SongRequestsMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);

            this.SpotifyTextBlock.ToolTip = SpotifySetupTooltip;
            this.SpotifyToggleButton.ToolTip = SpotifySetupTooltip;

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
                SongRequestModel songRequest = (SongRequestModel)button.DataContext;
                this.viewModel.MoveUpCommand.Execute(songRequest);
                return Task.FromResult(0);
            });
        }

        private async void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                Button button = (Button)sender;
                SongRequestModel songRequest = (SongRequestModel)button.DataContext;
                this.viewModel.MoveDownCommand.Execute(songRequest);
                return Task.FromResult(0);
            });
        }

        private async void DeleteQueueButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                Button button = (Button)sender;
                SongRequestModel songRequest = (SongRequestModel)button.DataContext;
                this.viewModel.DeleteCommand.Execute(songRequest);
                return Task.FromResult(0);
            });
        }
    }
}
