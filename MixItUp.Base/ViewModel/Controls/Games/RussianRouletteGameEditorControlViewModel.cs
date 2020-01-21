using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class RussianRouletteGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public string MinimumParticipantsString
        {
            get { return this.MinimumParticipants.ToString(); }
            set
            {
                this.MinimumParticipants = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int MinimumParticipants { get; set; } = 2;

        public string TimeLimitString
        {
            get { return this.TimeLimit.ToString(); }
            set
            {
                this.TimeLimit = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int TimeLimit { get; set; } = 60;

        public string MaxWinnersString
        {
            get { return this.MaxWinners.ToString(); }
            set
            {
                this.MaxWinners = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int MaxWinners { get; set; } = 1;

        public CustomCommand StartedCommand { get; set; }

        public CustomCommand UserJoinCommand { get; set; }
        public CustomCommand NotEnoughPlayersCommand { get; set; }

        public CustomCommand UserSuccessCommand { get; set; }
        public CustomCommand UserFailCommand { get; set; }
        public CustomCommand GameCompleteCommand { get; set; }

        private RussianRouletteGameCommand existingCommand;

        public RussianRouletteGameEditorControlViewModel(UserCurrencyModel currency)
        {
            this.StartedCommand = this.CreateBasicChatCommand("@$username has started a game of Russian Roulette with a $gamebet " + currency.Name + " entry fee! Type !rr to join in!");

            this.UserJoinCommand = this.CreateBasicChatCommand("You've joined in the russian roulette match! Let's see who walks away the winner...", whisper: true);
            this.NotEnoughPlayersCommand = this.CreateBasicChatCommand("@$username couldn't get enough users to join in...");

            this.UserSuccessCommand = this.CreateBasicChatCommand("You survived and walked away with $gamepayout " + currency.Name + "!", whisper: true);
            this.UserFailCommand = this.CreateBasicChatCommand("Looks like luck was not on your side. Better luck next time...", whisper: true);
            this.GameCompleteCommand = this.CreateBasicChatCommand("The dust settles after a grueling match-up and...It's $gamewinners! Total Amount Per Winner: $gameallpayout " + currency.Name + "!");
        }

        public RussianRouletteGameEditorControlViewModel(RussianRouletteGameCommand command)
        {
            this.existingCommand = command;

            this.MinimumParticipants = this.existingCommand.MinimumParticipants;
            this.TimeLimit = this.existingCommand.TimeLimit;
            this.MaxWinners = this.existingCommand.MaxWinners;

            this.StartedCommand = this.existingCommand.StartedCommand;

            this.UserJoinCommand = this.existingCommand.UserJoinCommand;
            this.NotEnoughPlayersCommand = this.existingCommand.NotEnoughPlayersCommand;

            this.UserSuccessCommand = this.existingCommand.UserSuccessOutcome.Command;
            this.UserFailCommand = this.existingCommand.UserFailOutcome.Command;
            this.GameCompleteCommand = this.existingCommand.GameCompleteCommand;
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            Dictionary<MixerRoleEnum, int> roleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 0 }, { MixerRoleEnum.Subscriber, 0 }, { MixerRoleEnum.Mod, 0 } };

            GameCommandBase newCommand = new RussianRouletteGameCommand(name, triggers, requirements, this.MinimumParticipants, this.TimeLimit, this.StartedCommand, this.UserJoinCommand,
                new GameOutcome("Success", 0, roleProbabilities, this.UserSuccessCommand), new GameOutcome("Failure", 0, roleProbabilities, this.UserFailCommand), this.MaxWinners,
                this.GameCompleteCommand, this.NotEnoughPlayersCommand);
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        public override async Task<bool> Validate()
        {
            if (this.TimeLimit <= 0)
            {
                await DialogHelper.ShowMessage("The Time Limit is not a valid number greater than 0");
                return false;
            }

            if (this.MinimumParticipants <= 0)
            {
                await DialogHelper.ShowMessage("The Minimum Participants is not a valid number greater than 0");
                return false;
            }

            if (this.MaxWinners <= 0)
            {
                await DialogHelper.ShowMessage("The Max Winners is not a valid number greater than 0");
                return false;
            }

            if (this.MaxWinners >= this.MinimumParticipants)
            {
                await DialogHelper.ShowMessage("Max Winners must be less than Minimum Participants");
                return false;
            }

            return true;
        }
    }
}
