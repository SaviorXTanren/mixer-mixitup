using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for GiveawayControl.xaml
    /// </summary>
    public partial class GiveawayControl : MainControlBase
    {
        private GiveawayMainControlViewModel viewModel;

        public GiveawayControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new GiveawayMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);

            this.Requirements.HideThresholdRequirement();
            this.Requirements.SetRequirements(ChannelSession.Settings.GiveawayRequirements);

            this.GiveawayStartReminderCommand.DataContext = ChannelSession.Settings.GiveawayStartedReminderCommand;
            this.GiveawayUserJoinedCommand.DataContext = ChannelSession.Settings.GiveawayUserJoinedCommand;
            this.GiveawayWinnerSelectedCommand.DataContext = ChannelSession.Settings.GiveawayWinnerSelectedCommand;

            return base.InitializeInternal();
        }

        private void GiveawayCommand_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.Show();
            }
        }

        private async void StartGiveawayButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.EntryType == GiveawayEntryTypeEnum.Command)
            {
                if (!await this.Requirements.Validate())
                {
                    return;
                }
            }
            this.viewModel.StartGiveawayCommand.Execute(null);
        }
    }
}