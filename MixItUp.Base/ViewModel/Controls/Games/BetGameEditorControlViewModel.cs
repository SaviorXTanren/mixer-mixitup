using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class BetGameEditorControlViewModel : GameEditorControlViewModelBase
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

        public string UserPayoutString
        {
            get { return this.UserPayout.ToString(); }
            set
            {
                this.UserPayout = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public double UserPayout { get; set; } = 200;

        public string SubscriberPayoutString
        {
            get { return this.SubscriberPayout.ToString(); }
            set
            {
                this.SubscriberPayout = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public double SubscriberPayout { get; set; } = 200;

        public string ModPayoutString
        {
            get { return this.ModPayout.ToString(); }
            set
            {
                this.ModPayout = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public double ModPayout { get; set; } = 200;

        public string SelectableBetTypes
        {
            get { return this.selectableBetTypes; }
            set
            {
                this.selectableBetTypes = value;
                this.NotifyPropertyChanged();
            }
        }
        private string selectableBetTypes { get; set; }

        public CustomCommand StartedCommand { get; set; }
        public CustomCommand UserJoinedCommand { get; set; }

        public CustomCommand NotEnoughPlayersCommand { get; set; }
        public CustomCommand BetsClosedCommand { get; set; }

        public CustomCommand UserSuccessCommand { get; set; }
        public CustomCommand UserFailCommand { get; set; }

        public CustomCommand GameCompleteCommand { get; set; }

        private BetGameCommand existingCommand;

        public BetGameEditorControlViewModel(UserCurrencyViewModel currency)
        {
            this.StartedCommand = this.CreateBasic2ChatCommand("@$username has started a bet on...SOMETHING! Type !bet <OPTION #> <AMOUNT> in chat to participate!", "Options: $gamebetoptions");
            this.UserJoinedCommand = this.CreateBasicChatCommand("Your bet option has been selected!", whisper: true);

            this.NotEnoughPlayersCommand = this.CreateBasicChatCommand("@$username couldn't get enough users to join in...");
            this.BetsClosedCommand = this.CreateBasicChatCommand("All bets are now closed! Let's wait and see what the result is...");

            this.UserSuccessCommand = this.CreateBasicChatCommand("Congrats, you made out with $gamepayout " + currency.Name + "!", whisper: true);
            this.UserFailCommand = this.CreateBasicChatCommand("Lady luck wasn't with you today, better luck next time...", whisper: true);
            this.GameCompleteCommand = this.CreateBasicChatCommand("$gamebetwinningoption was the winning choice!");
        }

        public BetGameEditorControlViewModel(BetGameCommand command)
        {
            this.existingCommand = command;

            this.WhoCanStart = this.existingCommand.GameStarterRequirement.MixerRole;
            this.MinimumParticipants = this.existingCommand.MinimumParticipants;
            this.TimeLimit = this.existingCommand.TimeLimit;
            this.SelectableBetTypes = string.Join(Environment.NewLine, this.existingCommand.Options);

            this.StartedCommand = this.existingCommand.StartedCommand;
            this.UserJoinedCommand = this.existingCommand.UserJoinCommand;

            this.NotEnoughPlayersCommand = this.existingCommand.NotEnoughPlayersCommand;
            this.BetsClosedCommand = this.existingCommand.BetsClosedCommand;

            this.UserSuccessCommand = this.existingCommand.UserSuccessOutcome.Command;
            this.UserFailCommand = this.existingCommand.UserFailOutcome.Command;
            this.GameCompleteCommand = this.existingCommand.GameCompleteCommand;
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            List<string> validBetTypes = new List<string>();
            foreach (string betType in this.SelectableBetTypes.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                validBetTypes.Add(betType.ToLower());
            }

            RoleRequirementViewModel starterRequirement = new RoleRequirementViewModel(this.WhoCanStart);
            Dictionary<MixerRoleEnum, double> successRolePayouts = new Dictionary<MixerRoleEnum, double>() { { MixerRoleEnum.User, this.UserPayout }, { MixerRoleEnum.Subscriber, this.SubscriberPayout }, { MixerRoleEnum.Mod, this.ModPayout = this.ModPayout } };
            Dictionary<MixerRoleEnum, int> roleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 0 }, { MixerRoleEnum.Subscriber, 0 }, { MixerRoleEnum.Mod, 0 } };

            GameCommandBase newCommand = new BetGameCommand(name, triggers, requirements, this.MinimumParticipants, this.TimeLimit, starterRequirement,
                this.StartedCommand, validBetTypes, this.UserJoinedCommand, this.BetsClosedCommand, new GameOutcome("Success", successRolePayouts, roleProbabilities, this.UserSuccessCommand),
                new GameOutcome("Failure", 0, roleProbabilities, this.UserFailCommand), this.GameCompleteCommand, this.NotEnoughPlayersCommand);
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

            if (this.UserPayout < 0)
            {
                await DialogHelper.ShowMessage("The User Payout %'s is not a valid number greater than or equal to 0");
                return false;
            }

            if (this.SubscriberPayout < 0)
            {
                await DialogHelper.ShowMessage("The Subscriber Payout %'s is not a valid number greater than or equal to 0");
                return false;
            }

            if (this.ModPayout < 0)
            {
                await DialogHelper.ShowMessage("The Mod Payout %'s is not a valid number greater than or equal to 0");
                return false;
            }

            if (string.IsNullOrEmpty(this.SelectableBetTypes))
            {
                await DialogHelper.ShowMessage("The Valid Bet Types does not have a value");
                return false;
            }

            HashSet<string> validBetTypes = new HashSet<string>();
            foreach (string betType in this.SelectableBetTypes.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                validBetTypes.Add(betType.ToLower());
            }

            if (validBetTypes.Count() < 2)
            {
                await DialogHelper.ShowMessage("You must specify at least 2 different bet types");
                return false;
            }

            return true;
        }
    }
}
