using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;
using MixItUp.Base.Util;
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
            await this.viewModel.OnOpen();
        }

        private void UserJoinedCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((GameQueueMainControlViewModel)this.DataContext).GameQueueUserJoinedCommand = command; };
            window.ForceShow();
        }

        private void UserSelectedCommand_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.CommandSaved += (object s, CommandModelBase command) => { ((GameQueueMainControlViewModel)this.DataContext).GameQueueUserSelectedCommand = command; };
            window.ForceShow();
        }

        private async void MoveUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                try
                {
                    QueueUser queueUser = FrameworkElementHelpers.GetDataContext<QueueUser>(sender);
                    this.viewModel.MoveUpCommand.Execute(queueUser);
                }
                catch (Exception ex) { Logger.Log(ex); }
                return Task.CompletedTask;
            });
        }

        private async void MoveDownButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                try
                {
                    QueueUser queueUser = FrameworkElementHelpers.GetDataContext<QueueUser>(sender);
                    this.viewModel.MoveDownCommand.Execute(queueUser);
                }
                catch (Exception ex) { Logger.Log(ex); }
                return Task.CompletedTask;
            });
        }

        private async void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                try
                {
                    QueueUser queueUser = FrameworkElementHelpers.GetDataContext<QueueUser>(sender);
                    this.viewModel.DeleteCommand.Execute(queueUser);
                }
                catch (Exception ex) { Logger.Log(ex); }
                return Task.CompletedTask;
            });
        }
    }
}
