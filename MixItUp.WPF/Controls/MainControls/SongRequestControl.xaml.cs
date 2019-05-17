using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
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
        private SongRequestsMainControlViewModel viewModel;

        public SongRequestControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new SongRequestsMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnLoaded();

            this.SongAddedCommand.DataContext = ChannelSession.Settings.SongAddedCommand;
            this.SongPlayedCommand.DataContext = ChannelSession.Settings.SongPlayedCommand;

            await base.InitializeInternal();
        }

        private void SongCommand_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.Show();
            }
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

        private async void BanQueueButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                Button button = (Button)sender;
                SongRequestModel songRequest = (SongRequestModel)button.DataContext;
                this.viewModel.BanCommand.Execute(songRequest);
                return Task.FromResult(0);
            });
        }
    }
}
