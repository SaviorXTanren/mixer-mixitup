using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for DuelGameEditorControl.xaml
    /// </summary>
    public partial class DuelGameEditorControl : GameEditorControlBase
    {
        private DuelGameCommand existingCommand;

        private CustomCommand startedCommand;
        private CustomCommand successOutcomeCommand;
        private CustomCommand failOutcomeCommand;

        public DuelGameEditorControl()
        {
            InitializeComponent();
        }

        public DuelGameEditorControl(DuelGameCommand command)
            : this()
        {
            this.existingCommand = command;
        }

        public override async Task<bool> Validate()
        {
            if (!await this.CommandDetailsControl.Validate())
            {
                return false;
            }

            if (!int.TryParse(this.TimeLimitTextBox.Text, out int timeLimit) || timeLimit < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Time Limit is not a valid number greater than 0");
                return false;
            }

            if (!int.TryParse(this.UserPercentageTextBox.Text, out int userChance) || userChance < 0 || userChance > 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The User Chance %'s is not a valid number between 0 - 100");
                return false;
            }

            if (!int.TryParse(this.SubscriberPercentageTextBox.Text, out int subscriberChance) || subscriberChance < 0 || subscriberChance > 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The Sub Chance %'s is not a valid number between 0 - 100");
                return false;
            }

            if (!int.TryParse(this.ModPercentageTextBox.Text, out int modChance) || modChance < 0 || modChance > 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The Mod Chance %'s is not a valid number between 0 - 100");
                return false;
            }

            return true;
        }

        public override void SaveGameCommand()
        {
            int.TryParse(this.TimeLimitTextBox.Text, out int timeLimit);
            int.TryParse(this.UserPercentageTextBox.Text, out int userChance);
            int.TryParse(this.SubscriberPercentageTextBox.Text, out int subscriberChance);
            int.TryParse(this.ModPercentageTextBox.Text, out int modChance);

            Dictionary<MixerRoleEnum, int> successRoleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, userChance }, { MixerRoleEnum.Subscriber, subscriberChance }, { MixerRoleEnum.Mod, modChance } };
            Dictionary<MixerRoleEnum, int> failRoleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 100 - userChance }, { MixerRoleEnum.Subscriber, 100 - subscriberChance }, { MixerRoleEnum.Mod, 100 - modChance } };

            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
            }
            ChannelSession.Settings.GameCommands.Add(new DuelGameCommand(this.CommandDetailsControl.GameName, this.CommandDetailsControl.ChatTriggers,
                this.CommandDetailsControl.GetRequirements(), new GameOutcome("Success", 1, successRoleProbabilities, this.successOutcomeCommand),
                new GameOutcome("Failure", 0, failRoleProbabilities, this.failOutcomeCommand), this.startedCommand, timeLimit));
        }

        protected override Task OnLoaded()
        {
            if (this.existingCommand != null)
            {
                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);
                this.startedCommand = this.existingCommand.StartedCommand;
                this.TimeLimitTextBox.Text = this.existingCommand.TimeLimit.ToString();
                this.UserPercentageTextBox.Text = this.existingCommand.SuccessfulOutcome.RoleProbabilities[MixerRoleEnum.User].ToString();
                this.SubscriberPercentageTextBox.Text = this.existingCommand.SuccessfulOutcome.RoleProbabilities[MixerRoleEnum.Subscriber].ToString();
                this.ModPercentageTextBox.Text = this.existingCommand.SuccessfulOutcome.RoleProbabilities[MixerRoleEnum.Mod].ToString();
                this.successOutcomeCommand = this.existingCommand.SuccessfulOutcome.Command;
                this.failOutcomeCommand = this.existingCommand.FailedOutcome.Command;
            }
            else
            {
                this.CommandDetailsControl.SetDefaultValues("Duel", "duel", CurrencyRequirementTypeEnum.MinimumAndMaximum, 10, 1000);
                UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault();
                this.startedCommand = this.CreateBasicChatCommand("@$username has challenged @$targetusername to a duel for $gamebet " + currency.Name + "! Type !duel in chat to accept!");
                this.TimeLimitTextBox.Text = "30";
                this.UserPercentageTextBox.Text = "50";
                this.SubscriberPercentageTextBox.Text = "50";
                this.ModPercentageTextBox.Text = "50";
                this.successOutcomeCommand = this.CreateBasicChatCommand("@$username won the duel against @$targetusername, winning $gamepayout " + currency.Name + "!");
                this.failOutcomeCommand = this.CreateBasicChatCommand("@$targetusername defeated @$username at his own game, winning $gamepayout " + currency.Name + "!");
            }

            this.DuelStartedCommandButtonsControl.DataContext = this.startedCommand;
            this.SuccessOutcomeCommandButtonsControl.DataContext = this.successOutcomeCommand;
            this.FailOutcomeCommandButtonsControl.DataContext = this.failOutcomeCommand;

            return base.OnLoaded();
        }

        private void OutcomeCommandButtonsControl_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.Show();
            }
        }

        private CustomCommand CreateBasicChatCommand(string message)
        {
            CustomCommand command = new CustomCommand("Game Outcome");
            command.Actions.Add(new ChatAction(message));
            return command;
        }
    }
}
