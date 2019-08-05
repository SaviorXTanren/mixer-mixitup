using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class BetOutcome : UIViewModelBase
    {
        public string Name { get; set; }

        public CustomCommand Command { get; set; }

        public int Payout { get; set; }

        public BetOutcome(string name, CustomCommand command, int payout = 0)
        {
            this.Name = name;
            this.Command = command;
            this.Payout = payout;
        }

        public BetOutcome(GameOutcome outcome) : this(outcome.Name, outcome.Command)
        {
            this.Payout = Convert.ToInt32(outcome.Payout * 100.0);
        }

        public string PayoutString
        {
            get { return this.Payout.ToString(); }
            set { this.Payout = this.GetPositiveIntFromString(value); }
        }

        public GameOutcome GetGameOutcome()
        {
            return new GameOutcome(this.Name, Convert.ToDouble(this.Payout) / 100.0, new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 0 }, { MixerRoleEnum.Subscriber, 0 }, { MixerRoleEnum.Mod, 0 } }, this.Command);
        }
    }

    public class BetGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public ObservableCollection<BetOutcome> Options { get; set; } = new ObservableCollection<BetOutcome>();

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

        public CustomCommand StartedCommand { get; set; }
        public CustomCommand UserJoinedCommand { get; set; }

        public CustomCommand NotEnoughPlayersCommand { get; set; }
        public CustomCommand BetsClosedCommand { get; set; }

        public CustomCommand UserFailCommand { get; set; }
        public CustomCommand GameCompleteCommand { get; set; }

        public ICommand AddOutcomeCommand { get; set; }
        public ICommand DeleteOutcomeCommand { get; set; }

        private BetGameCommand existingCommand;

        public BetGameEditorControlViewModel(UserCurrencyViewModel currency)
            : this()
        {
            this.StartedCommand = this.CreateBasic2ChatCommand("@$username has started a bet on...SOMETHING! Type !bet <OPTION #> <AMOUNT> in chat to participate!", "Options: $gamebetoptions");
            this.UserJoinedCommand = this.CreateBasicChatCommand("Your bet option has been selected!", whisper: true);

            this.NotEnoughPlayersCommand = this.CreateBasicChatCommand("@$username couldn't get enough users to join in...");
            this.BetsClosedCommand = this.CreateBasicChatCommand("All bets are now closed! Let's wait and see what the result is...");
            this.UserFailCommand = this.CreateBasicChatCommand("Lady luck wasn't with you today, better luck next time...", whisper: true);
            this.GameCompleteCommand = this.CreateBasicChatCommand("$gamebetwinningoption was the winning choice!");

            this.Options.Add(new BetOutcome("Win Match", this.CreateBasicChatCommand("We both won! Which mean you won $gamepayout " + currency.Name + "!", whisper: true), 200));
            this.Options.Add(new BetOutcome("Lose Match", this.CreateBasicChatCommand("Well, I lose and you won $gamepayout " + currency.Name + ", so there's something at least...", whisper: true), 200));
        }

        public BetGameEditorControlViewModel(BetGameCommand command)
            : this()
        {
            this.existingCommand = command;

            this.WhoCanStart = this.existingCommand.GameStarterRequirement.MixerRole;
            this.MinimumParticipants = this.existingCommand.MinimumParticipants;
            this.TimeLimit = this.existingCommand.TimeLimit;

            this.StartedCommand = this.existingCommand.StartedCommand;
            this.UserJoinedCommand = this.existingCommand.UserJoinCommand;

            this.NotEnoughPlayersCommand = this.existingCommand.NotEnoughPlayersCommand;
            this.BetsClosedCommand = this.existingCommand.BetsClosedCommand;
            this.UserFailCommand = this.existingCommand.UserFailOutcome.Command;
            this.GameCompleteCommand = this.existingCommand.GameCompleteCommand;

            foreach (GameOutcome outcome in this.existingCommand.BetOptions)
            {
                this.Options.Add(new BetOutcome(outcome));
            }
        }

        private BetGameEditorControlViewModel()
        {
            this.AddOutcomeCommand = this.CreateCommand((parameter) =>
            {
                this.Options.Add(new BetOutcome("", this.CreateBasicChatCommand("@$username")));
                return Task.FromResult(0);
            });

            this.DeleteOutcomeCommand = this.CreateCommand((parameter) =>
            {
                this.Options.Remove((BetOutcome)parameter);
                return Task.FromResult(0);
            });
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            RoleRequirementViewModel starterRequirement = new RoleRequirementViewModel(this.WhoCanStart);
            GameCommandBase newCommand = new BetGameCommand(name, triggers, requirements, this.MinimumParticipants, this.TimeLimit, starterRequirement,
                this.Options.Select(o => o.GetGameOutcome()), this.StartedCommand, this.UserJoinedCommand, this.BetsClosedCommand,
                new GameOutcome("Failure", 0, new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 0 }, { MixerRoleEnum.Subscriber, 0 }, { MixerRoleEnum.Mod, 0 } }, this.UserFailCommand),
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

            if (this.Options.Count() < 2)
            {
                await DialogHelper.ShowMessage("You must specify at least 2 different bet types");
                return false;
            }

            foreach (BetOutcome outcome in this.Options)
            {
                if (string.IsNullOrEmpty(outcome.Name))
                {
                    await DialogHelper.ShowMessage("An outcome is missing a name");
                    return false;
                }

                if (outcome.Command == null)
                {
                    await DialogHelper.ShowMessage("An outcome is missing a command");
                    return false;
                }
            }

            return true;
        }
    }
}
