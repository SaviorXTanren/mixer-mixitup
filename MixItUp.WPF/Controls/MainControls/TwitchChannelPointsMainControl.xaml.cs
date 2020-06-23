using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for TwitchChannelPointsMainControl.xaml
    /// </summary>
    public partial class TwitchChannelPointsMainControl : MainControlBase
    {
        private TwitchChannelPointsMainControlViewModel viewModel;

        public TwitchChannelPointsMainControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new TwitchChannelPointsMainControlViewModel(this.Window.ViewModel);
            await this.viewModel.OnLoaded();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            TwitchChannelPointsCommand command = commandButtonsControl.GetCommandFromCommandButtons<TwitchChannelPointsCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new TwitchChannelPointsCommandDetailsControl(command));
                window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                TwitchChannelPointsCommand command = commandButtonsControl.GetCommandFromCommandButtons<TwitchChannelPointsCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.TwitchChannelPointsCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.viewModel.Refresh();
                }
            });
        }

        private void AddRewardCommand_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new TwitchChannelPointsCommandDetailsControl());
            window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
            window.Show();
        }

        private void Window_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            this.viewModel.Refresh();
        }
    }
}
