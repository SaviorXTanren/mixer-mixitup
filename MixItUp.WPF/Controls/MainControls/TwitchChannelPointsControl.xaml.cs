using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.WPF.Controls.Commands;
using MixItUp.WPF.Windows.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for TwitchChannelPointsControl.xaml
    /// </summary>
    public partial class TwitchChannelPointsControl : MainControlBase
    {
        private TwitchChannelPointsMainControlViewModel viewModel;

        public TwitchChannelPointsControl()
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
            TwitchChannelPointsCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<TwitchChannelPointsCommandModel>();
            if (command != null)
            {
                CommandEditorWindow window = new CommandEditorWindow(command);
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                TwitchChannelPointsCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<TwitchChannelPointsCommandModel>();
                if (command != null)
                {
                    ChannelSession.TwitchChannelPointsCommands.Remove(command);
                    ChannelSession.Settings.RemoveCommand(command);
                    await ChannelSession.SaveSettings();
                    this.viewModel.Refresh();
                }
            });
        }

        private void AddRewardCommand_Click(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(CommandTypeEnum.TwitchChannelPoints);
            window.Closed += Window_Closed;
            window.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.viewModel.Refresh();
        }
    }
}
