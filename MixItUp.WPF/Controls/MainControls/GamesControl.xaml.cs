using MixItUp.Base;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.WPF.Controls.Commands;
using MixItUp.WPF.Windows.Commands;
using System;
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
            GameCommandModelBase command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<GameCommandModelBase>();
            if (command != null)
            {
                GameCommandEditorWindow window = new GameCommandEditorWindow(command);
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                GameCommandModelBase command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<GameCommandModelBase>();
                if (command != null)
                {
                    ChannelSession.GameCommands.Remove(command);
                    ChannelSession.Settings.RemoveCommand(command);
                    await ChannelSession.SaveSettings();
                    this.viewModel.Refresh();
                }
            });
        }

        private void CommandButtons_EnableDisableToggled(object sender, RoutedEventArgs e)
        {
            ChannelSession.Services.Chat.RebuildCommandTriggers();
        }

        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            GameCommandEditorWindow window = new GameCommandEditorWindow(GameCommandTypeEnum.Roulette, this.viewModel.PrimaryCurrency);
            window.Closed += Window_Closed;
            window.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.viewModel.Refresh();
        }
    }
}
