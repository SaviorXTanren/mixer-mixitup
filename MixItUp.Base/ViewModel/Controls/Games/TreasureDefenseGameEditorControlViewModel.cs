using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class TreasureDefenseGameEditorControlViewModel : GameEditorControlViewModelBase
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
        public int MinimumParticipants { get; set; } = 3;

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

        public string ThiefPlayerPercentageString
        {
            get { return this.ThiefPlayerPercentage.ToString(); }
            set
            {
                this.ThiefPlayerPercentage = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int ThiefPlayerPercentage { get; set; } = 45;

        public CustomCommand StartedCommand { get; set; }

        public CustomCommand UserJoinCommand { get; set; }
        public CustomCommand NotEnoughPlayersCommand { get; set; }

        public CustomCommand KnightUserCommand { get; set; }
        public CustomCommand ThiefUserCommand { get; set; }
        public CustomCommand KingUserCommand { get; set; }

        public CustomCommand KnightSelectedCommand { get; set; }
        public CustomCommand ThiefSelectedCommand { get; set; }

        private TreasureDefenseGameCommand existingCommand;

        public TreasureDefenseGameEditorControlViewModel(UserCurrencyViewModel currency)
        {
            this.StartedCommand = this.CreateBasicChatCommand("@$username has started a game of Treasure Defense with a $gamebet " + currency.Name + " entry fee! Type !treasure to join in!");

            this.UserJoinCommand = this.CreateBasicChatCommand("You've joined in to the defense of the treasure! Let's see what role you're assigned...", whisper: true);
            this.NotEnoughPlayersCommand = this.CreateBasicChatCommand("@$username couldn't get enough users to join in...");

            this.KnightUserCommand = this.CreateBasicChatCommand("You've been selected as a Knight; convince the King to pick you so can protect the treasure!", whisper: true);
            this.ThiefUserCommand = this.CreateBasicChatCommand("You've been selected as a Thief; convince the King you are a Knight so you can steal the treasure!", whisper: true);
            this.KingUserCommand = this.CreateBasicChatCommand("King @$username has discovered the treasure! They must type \"!treasure <PLAYER>\" in chat to pick who will defend the treasure");

            this.KnightSelectedCommand = this.CreateBasicChatCommand("King @$username picked @$targetusername to defend the treasure...and they were a Knight! They defended the treasure, giving the King & Knights $gamepayout " + currency.Name + " each!");
            this.ThiefSelectedCommand = this.CreateBasicChatCommand("King @$username picked @$targetusername to defend the treasure...and they were a Thief! They stole the treasure, giving all Thieves $gamepayout " + currency.Name + " each!");
        }

        public TreasureDefenseGameEditorControlViewModel(TreasureDefenseGameCommand command)
        {
            this.existingCommand = command;

            this.MinimumParticipants = this.existingCommand.MinimumParticipants;
            this.TimeLimit = this.existingCommand.TimeLimit;
            this.ThiefPlayerPercentage = this.existingCommand.ThiefPlayerPercentage;

            this.StartedCommand = this.existingCommand.StartedCommand;

            this.UserJoinCommand = this.existingCommand.UserJoinCommand;
            this.NotEnoughPlayersCommand = this.existingCommand.NotEnoughPlayersCommand;

            this.KnightUserCommand = this.existingCommand.KnightUserCommand;
            this.ThiefUserCommand = this.existingCommand.ThiefUserCommand;
            this.KingUserCommand = this.existingCommand.KingUserCommand;

            this.KnightSelectedCommand = this.existingCommand.KnightSelectedCommand;
            this.ThiefSelectedCommand = this.existingCommand.ThiefSelectedCommand;
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            GameCommandBase newCommand = new TreasureDefenseGameCommand(name, triggers, requirements, this.MinimumParticipants, this.TimeLimit, this.ThiefPlayerPercentage, this.StartedCommand,
                this.UserJoinCommand, this.KnightUserCommand, this.ThiefUserCommand, this.KingUserCommand, this.KnightSelectedCommand, this.ThiefSelectedCommand, this.NotEnoughPlayersCommand);
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

            if (this.MinimumParticipants <= 2)
            {
                await DialogHelper.ShowMessage("The Minimum Participants is not a valid number greater than 2");
                return false;
            }

            if (this.ThiefPlayerPercentage <= 0 || this.ThiefPlayerPercentage >= 100)
            {
                await DialogHelper.ShowMessage("The Thief Player Percentage is not a valid number between 1 - 99");
                return false;
            }

            return true;
        }
    }
}
