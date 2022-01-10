using MixItUp.Base;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.Dialogs;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.WPF.Controls.Commands;
using MixItUp.WPF.Controls.Dialogs;
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
            await this.viewModel.OnOpen();
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
                    ServiceManager.Get<CommandService>().GameCommands.Remove(command);
                    ChannelSession.Settings.RemoveCommand(command);
                    ServiceManager.Get<ChatService>().RebuildCommandTriggers();
                    this.viewModel.Refresh();
                    await ChannelSession.SaveSettings();
                }
            });
        }

        private void CommandButtons_EnableDisableToggled(object sender, RoutedEventArgs e)
        {
            ServiceManager.Get<ChatService>().RebuildCommandTriggers();
        }

        private async void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            GameTypeSelectorDialogControl gameTypeSelectorDialogControl = new GameTypeSelectorDialogControl();
            GameTypeSelectorDialogControlViewModel viewModel = new GameTypeSelectorDialogControlViewModel();
            gameTypeSelectorDialogControl.DataContext = viewModel;
            if (bool.Equals(await DialogHelper.ShowCustom(gameTypeSelectorDialogControl), true))
            {
                await Task.Delay(300);
                GameCommandEditorWindow window = new GameCommandEditorWindow(viewModel.SelectedGameType, this.viewModel.PrimaryCurrency);
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.viewModel.Refresh();
        }
    }
}
