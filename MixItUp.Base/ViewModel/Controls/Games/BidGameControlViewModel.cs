using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class BidGameControlViewModel : GamesControlViewModelBase
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

        public string MinUsersString
        {
            get { return this.MinUsers.ToString(); }
            set
            {
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int intValue) && intValue > 0)
                {
                    this.MinUsers = intValue;
                }
                else
                {
                    this.MinUsers = 0;
                }
                this.NotifyPropertyChanged();
            }
        }
        public int MinUsers { get; set; } = 1;

        public string TimeLimitString
        {
            get { return this.TimeLimit.ToString(); }
            set
            {
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int intValue) && intValue > 0)
                {
                    this.TimeLimit = intValue;
                }
                else
                {
                    this.TimeLimit = 0;
                }
                this.NotifyPropertyChanged();
            }
        }
        public int TimeLimit { get; set; } = 60;

        public CustomCommand StartedCommand { get; private set; }
        public CustomCommand UserJoinedCommand { get; private set; }
        public CustomCommand GameCompleteCommand { get; private set; }

        private BidGameCommand existingCommand;

        public BidGameControlViewModel(UserCurrencyViewModel currency)
        {
            this.StartedCommand = this.CreateBasicChatCommand("@$username has started a bidding war starting at $gamebet " + currency.Name + " for...SOMETHING! Type !bid <AMOUNT> in chat to outbid them!");
            this.UserJoinedCommand = this.CreateBasicChatCommand("@$username has become the top bidder with $gamebet " + currency.Name + "! Type !bid <AMOUNT> in chat to outbid them!");
            this.GameCompleteCommand = this.CreateBasicChatCommand("$gamewinners won the bidding war with a bid of $gamebet " + currency.Name + "! Listen closely for how to claim your prize...");
        }

        public BidGameControlViewModel(BidGameCommand command)
        {
            this.existingCommand = command;

            this.WhoCanStart = this.existingCommand.GameStarterRequirement.MixerRole;
            this.MinUsers = this.existingCommand.MinimumParticipants;
            this.TimeLimit = this.existingCommand.TimeLimit;

            this.StartedCommand = this.existingCommand.StartedCommand;
            this.UserJoinedCommand = this.existingCommand.UserJoinCommand;
            this.GameCompleteCommand = this.existingCommand.GameCompleteCommand;
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            RoleRequirementViewModel starterRequirement = new RoleRequirementViewModel(this.WhoCanStart);
            GameCommandBase newCommand = new BidGameCommand(name, triggers, requirements, this.MinUsers, this.TimeLimit, starterRequirement,
                this.StartedCommand, this.UserJoinedCommand, this.GameCompleteCommand);
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

            if (this.MinUsers <= 0)
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
