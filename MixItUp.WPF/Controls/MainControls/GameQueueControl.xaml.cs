using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;
using StreamingClient.Base.Util;
using System;
using System.Threading.Tasks;

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
        }

        private void UserJoinedCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((GameQueueMainControlViewModel)this.DataContext).GameQueueUserJoinedCommand = command; };
            window.Show();
        }

        private void UserSelectedCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((GameQueueMainControlViewModel)this.DataContext).GameQueueUserSelectedCommand = command; };
            window.Show();
        }

        private async void MoveUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                try
                {
                    QueueUser queueUser = FrameworkElementHelpers.GetDataContext<QueueUser>(sender);
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
                    QueueUser queueUser = FrameworkElementHelpers.GetDataContext<QueueUser>(sender);
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
                    QueueUser queueUser = FrameworkElementHelpers.GetDataContext<QueueUser>(sender);
                    this.viewModel.DeleteCommand.Execute(queueUser.user);
                }
                catch (Exception ex) { Logger.Log(ex); }
                return Task.FromResult(0);
            });
        }
    }
}
