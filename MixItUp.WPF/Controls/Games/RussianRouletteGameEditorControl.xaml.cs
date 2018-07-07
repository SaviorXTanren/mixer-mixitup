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
    /// Interaction logic for RussianRouletteGameEditorControl.xaml
    /// </summary>
    public partial class RussianRouletteGameEditorControl : GameEditorControlBase
    {
        private RussianRouletteGameCommand existingCommand;

        private CustomCommand startedCommand { get; set; }

        private CustomCommand userJoinCommand { get; set; }

        private CustomCommand userSuccessCommand { get; set; }
        private CustomCommand userFailCommand { get; set; }

        private CustomCommand gameCompleteCommand { get; set; }

        public RussianRouletteGameEditorControl()
        {
            InitializeComponent();
        }

        public RussianRouletteGameEditorControl(RussianRouletteGameCommand command)
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

            if (!int.TryParse(this.TimeLimitTextBox.Text, out int timeLimit) || timeLimit <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Time Limit is not a valid number greater than 0");
                return false;
            }

            if (!int.TryParse(this.MinimumParticipantsTextBox.Text, out int minimumParticipants) || minimumParticipants <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Minimum Participants is not a valid number greater than 0");
                return false;
            }

            if (!int.TryParse(this.MaxWinnersTextBox.Text, out int maxWinners) || maxWinners <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Max Winners is not a valid number greater than 0");
                return false;
            }

            if (maxWinners >= minimumParticipants)
            {
                await MessageBoxHelper.ShowMessageDialog("Max Winners must be less than Minimum Participants");
                return false;
            }

            return true;
        }

        public override void SaveGameCommand()
        {
            int.TryParse(this.MinimumParticipantsTextBox.Text, out int minimumParticipants);
            int.TryParse(this.TimeLimitTextBox.Text, out int timeLimit);
            int.TryParse(this.MaxWinnersTextBox.Text, out int maxWinners);

            Dictionary<MixerRoleEnum, int> roleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 0 }, { MixerRoleEnum.Subscriber, 0 }, { MixerRoleEnum.Mod, 0 } };

            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
            }
            ChannelSession.Settings.GameCommands.Add(new RussianRouletteGameCommand(this.CommandDetailsControl.GameName, this.CommandDetailsControl.ChatTriggers,
                this.CommandDetailsControl.GetRequirements(), minimumParticipants, timeLimit, this.startedCommand, this.userJoinCommand,
                new GameOutcome("Success", 0, roleProbabilities, this.userSuccessCommand), new GameOutcome("Failure", 0, roleProbabilities, this.userFailCommand), maxWinners, this.gameCompleteCommand));
        }

        protected override Task OnLoaded()
        {
            if (this.existingCommand != null)
            {
                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);
                this.MinimumParticipantsTextBox.Text = this.existingCommand.MinimumParticipants.ToString();
                this.TimeLimitTextBox.Text = this.existingCommand.TimeLimit.ToString();
                this.MaxWinnersTextBox.Text = this.existingCommand.MaxWinners.ToString();

                this.startedCommand = this.existingCommand.StartedCommand;

                this.userJoinCommand = this.existingCommand.UserJoinCommand;

                this.userSuccessCommand = this.existingCommand.UserSuccessOutcome.Command;
                this.userFailCommand = this.existingCommand.UserFailOutcome.Command;

                this.gameCompleteCommand = this.existingCommand.GameCompleteCommand;
            }
            else
            {
                this.CommandDetailsControl.SetDefaultValues("Heist", "rr russian", CurrencyRequirementTypeEnum.MinimumAndMaximum, 10, 1000);
                UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault();
                this.MinimumParticipantsTextBox.Text = "2";
                this.TimeLimitTextBox.Text = "30";
                this.MaxWinnersTextBox.Text = "1";

                this.startedCommand = this.CreateBasicChatCommand("@$username has started a game of Russian Roulette with a $gamebet " + currency.Name + " entry fee! Type !rr to join in!");

                this.userJoinCommand = this.CreateBasicChatCommand("You've joined in the russian roulette match! Let's see who walks away the winner...", whisper: true);

                this.userSuccessCommand = this.CreateBasicChatCommand("You survived and walked away with $gamepayout " + currency.Name + "!", whisper: true);
                this.userFailCommand = this.CreateBasicChatCommand("Looks like luck was not on your side. Better luck next time...", whisper: true);

                this.gameCompleteCommand = this.CreateBasicChatCommand("The dust settles after a grueling match-up and...It's $gamewinners! Total Amount Per Winner: $gameallpayout " + currency.Name + "!");
            }

            this.GameStartCommandButtonsControl.DataContext = this.startedCommand;
            this.UserJoinedCommandButtonsControl.DataContext = this.userJoinCommand;
            this.SuccessOutcomeCommandButtonsControl.DataContext = this.userSuccessCommand;
            this.FailOutcomeCommandButtonsControl.DataContext = this.userFailCommand;
            this.GameCompleteCommandButtonsControl.DataContext = this.gameCompleteCommand;

            return base.OnLoaded();
        }
    }
}
