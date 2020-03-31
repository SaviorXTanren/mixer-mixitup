using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using StreamingClient.Base.Util;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for GameQueueControl.xaml
    /// </summary>
    public partial class GameQueueControl : MainControlBase
    {
        private GameQueueMainControlViewModel viewModel;

        public GameQueueControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new GameQueueMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnLoaded();

            this.UserJoinedCommand.DataContext = ChannelSession.Settings.GameQueueUserJoinedCommand;
            this.UserSelectedCommand.DataContext = ChannelSession.Settings.GameQueueUserSelectedCommand;
        }

        private void GameQueueCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.Show();
            }
        }

        private async void MoveUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                try
                {
                    Button button = (Button)sender;
                    QueueUser queueUser = (QueueUser)button.DataContext;
                    this.viewModel.MoveUpCommand.Execute(queueUser.user);
                }
                catch (Exception ex) { Logger.Log(ex); }
                return Task.FromResult(0);
            });
        }

        private async void MoveDownButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                try
                {
                    Button button = (Button)sender;
                    QueueUser queueUser = (QueueUser)button.DataContext;
                    this.viewModel.MoveDownCommand.Execute(queueUser.user);
                }
                catch (Exception ex) { Logger.Log(ex); }
                return Task.FromResult(0);
            });
        }

        private async void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                try
                {
                    Button button = (Button)sender;
                    QueueUser queueUser = (QueueUser)button.DataContext;
                    this.viewModel.DeleteCommand.Execute(queueUser.user);
                }
                catch (Exception ex) { Logger.Log(ex); }
                return Task.FromResult(0);
            });
        }
    }
}
