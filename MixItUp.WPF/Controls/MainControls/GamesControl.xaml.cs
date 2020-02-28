using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for GamesControl.xaml
    /// </summary>
    public partial class GamesControl : MainControlBase
    {
        private GamesMainControlViewModel viewModel;

        public GamesControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new GamesMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnLoaded();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonControl = (CommandButtonsControl)sender;
            GameCommandBase command = commandButtonControl.GetCommandFromCommandButtons<GameCommandBase>(sender);
            if (command != null)
            {
                GameCommandWindow window = new GameCommandWindow(command);
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonControl = (CommandButtonsControl)sender;
                GameCommandBase command = commandButtonControl.GetCommandFromCommandButtons<GameCommandBase>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.GameCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.viewModel.Refresh();
                    ChannelSession.Services.Chat.RebuildCommandTriggers();
                }
            });
        }

        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            GameCommandWindow window = new GameCommandWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.viewModel.Refresh();
            ChannelSession.Services.Chat.RebuildCommandTriggers();
        }
    }
}
