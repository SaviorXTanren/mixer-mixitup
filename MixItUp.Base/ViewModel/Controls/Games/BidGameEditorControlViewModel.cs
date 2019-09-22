using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class BidGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public IEnumerable<string> WhoCanStartRoles { get { return RoleRequirementViewModel.AdvancedUserRoleAllowedValues; } }

        public string WhoCanStartString
        {
            get { return EnumHelper.GetEnumName(this.WhoCanStart); }
            set
            {
                this.WhoCanStart = EnumHelper.GetEnumValueFromString<MixerRoleEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        public MixerRoleEnum WhoCanStart { get; set; } = MixerRoleEnum.Mod;

        public string MinimumParticipantsString
        {
            get { return this.MinimumParticipants.ToString(); }
            set
            {
                this.MinimumParticipants = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int MinimumParticipants { get; set; } = 1;

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

        public CustomCommand StartedCommand { get; set; }
        public CustomCommand UserJoinedCommand { get; set; }
        public CustomCommand NotEnoughPlayersCommand { get; set; }
        public CustomCommand GameCompleteCommand { get; set; }

        private BidGameCommand existingCommand;

        public BidGameEditorControlViewModel(UserCurrencyViewModel currency)
        {
            this.StartedCommand = this.CreateBasicChatCommand("@$username has started a bidding war starting at $gamebet " + currency.Name + " for...SOMETHING! Type !bid <AMOUNT> in chat to outbid them!");
            this.UserJoinedCommand = this.CreateBasicChatCommand("@$username has become the top bidder with $gamebet " + currency.Name + "! Type !bid <AMOUNT> in chat to outbid them!");
            this.NotEnoughPlayersCommand = this.CreateBasicChatCommand("@$username couldn't get enough users to join in...");
            this.GameCompleteCommand = this.CreateBasicChatCommand("$gamewinners won the bidding war with a bid of $gamebet " + currency.Name + "! Listen closely for how to claim your prize...");
        }

        public BidGameEditorControlViewModel(BidGameCommand command)
        {
            this.existingCommand = command;

            this.WhoCanStart = this.existingCommand.GameStarterRequirement.MixerRole;
            this.MinimumParticipants = this.existingCommand.MinimumParticipants;
            this.TimeLimit = this.existingCommand.TimeLimit;

            this.StartedCommand = this.existingCommand.StartedCommand;
            this.UserJoinedCommand = this.existingCommand.UserJoinCommand;
            this.NotEnoughPlayersCommand = this.existingCommand.NotEnoughPlayersCommand;
            this.GameCompleteCommand = this.existingCommand.GameCompleteCommand;
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            RoleRequirementViewModel starterRequirement = new RoleRequirementViewModel(this.WhoCanStart);
            GameCommandBase newCommand = new BidGameCommand(name, triggers, requirements, this.MinimumParticipants, this.TimeLimit, starterRequirement,
                this.StartedCommand, this.UserJoinedCommand, this.GameCompleteCommand, this.NotEnoughPlayersCommand);
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        public override async Task<bool> Validate()
        {
            if (this.WhoCanStart < 0)
            {
                await DialogHelper.ShowMessage("The Who Can Start Game must have a valid User Role selection");
                return false;
            }

            if (this.MinimumParticipants <= 0)
            {
                await DialogHelper.ShowMessage("The Minimum Users is not a valid number greater than 0");
                return false;
            }

            if (this.TimeLimit <= 0)
            {
                await DialogHelper.ShowMessage("The Time Limit is not a valid number greater than 0");
                return false;
            }

            return true;
        }
    }
}
