using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class HeistGameEditorControlViewModel : GameEditorControlViewModelBase
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
        public int TimeLimit { get; set; } = 30;

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

        public string UserProbabilityString
        {
            get { return this.UserProbability.ToString(); }
            set
            {
                this.UserProbability = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int UserProbability { get; set; } = 60;

        public string SubscriberProbabilityString
        {
            get { return this.SubscriberProbability.ToString(); }
            set
            {
                this.SubscriberProbability = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int SubscriberProbability { get; set; } = 60;

        public string ModProbabilityString
        {
            get { return this.ModProbability.ToString(); }
            set
            {
                this.ModProbability = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int ModProbability { get; set; } = 60;

        public CustomCommand StartedCommand { get; set; }

        public CustomCommand UserJoinCommand { get; set; }
        public CustomCommand NotEnoughPlayersCommand { get; set; }

        public CustomCommand UserSuccessCommand { get; set; }
        public CustomCommand UserFailCommand { get; set; }

        public CustomCommand AllSucceedCommand { get; set; }
        public CustomCommand TopThirdsSucceedCommand { get; set; }
        public CustomCommand MiddleThirdsSucceedCommand { get; set; }
        public CustomCommand LowThirdsSucceedCommand { get; set; }
        public CustomCommand NoneSucceedCommand { get; set; }

        private HeistGameCommand existingCommand;

        public HeistGameEditorControlViewModel(UserCurrencyViewModel currency)
        {
            this.StartedCommand = this.CreateBasicChatCommand("@$username has started a game of Heist! Type !heist <AMOUNT> to join in!");

            this.UserJoinCommand = this.CreateBasicChatCommand("You've joined in the heist! Let's see how it turns out...", whisper: true);
            this.NotEnoughPlayersCommand = this.CreateBasicChatCommand("@$username couldn't get enough users to join in...");

            this.UserSuccessCommand = this.CreateBasicChatCommand("Congrats, you made out with $gamepayout " + currency.Name + "!", whisper: true);
            this.UserFailCommand = this.CreateBasicChatCommand("The cops caught you before you could make it out! Better luck next time...", whisper: true);

            this.AllSucceedCommand = this.CreateBasic2ChatCommand("What a steal! Everyone made it out and cleaned the bank out dry! Total Amount: $gameallpayout " + currency.Name + "!", "Winners: $gamewinners");
            this.TopThirdsSucceedCommand = this.CreateBasic2ChatCommand("The cops showed up at the last second and snagged a few of you, but most made it out with the good! Total Amount: $gameallpayout " + currency.Name + "!", "Winners: $gamewinners");
            this.MiddleThirdsSucceedCommand = this.CreateBasic2ChatCommand("As you started to leave the bank, the cops were ready for you and got almost half of you! Total Amount: $gameallpayout " + currency.Name + "!", "Winners: $gamewinners");
            this.LowThirdsSucceedCommand = this.CreateBasic2ChatCommand("A heated battle took place inside the bank and almost everyone got caught by the cops! Total Amount: $gameallpayout " + currency.Name + "!", "Winners: $gamewinners");
            this.NoneSucceedCommand = this.CreateBasicChatCommand("Someone was a spy! The cops were waiting for you as soon as you showed up and got everyone!");
        }

        public HeistGameEditorControlViewModel(HeistGameCommand command)
        {
            this.existingCommand = command;

            this.MinimumParticipants = this.existingCommand.MinimumParticipants;
            this.TimeLimit = this.existingCommand.TimeLimit;
            this.UserPayout = (this.existingCommand.UserSuccessOutcome.RolePayouts[MixerRoleEnum.User] * 100);
            this.SubscriberPayout = (this.existingCommand.UserSuccessOutcome.RolePayouts[MixerRoleEnum.Subscriber] * 100);
            this.ModPayout = (this.existingCommand.UserSuccessOutcome.RolePayouts[MixerRoleEnum.Mod] * 100);
            this.UserProbability = this.existingCommand.UserSuccessOutcome.RoleProbabilities[MixerRoleEnum.User];
            this.SubscriberProbability = this.existingCommand.UserSuccessOutcome.RoleProbabilities[MixerRoleEnum.Subscriber];
            this.ModProbability = this.existingCommand.UserSuccessOutcome.RoleProbabilities[MixerRoleEnum.Mod];

            this.StartedCommand = this.existingCommand.StartedCommand;

            this.UserJoinCommand = this.existingCommand.UserJoinCommand;

            this.UserSuccessCommand = this.existingCommand.UserSuccessOutcome.Command;
            this.UserFailCommand = this.existingCommand.UserFailOutcome.Command;

            this.AllSucceedCommand = this.existingCommand.AllSucceedCommand;
            this.TopThirdsSucceedCommand = this.existingCommand.TopThirdsSucceedCommand;
            this.MiddleThirdsSucceedCommand = this.existingCommand.MiddleThirdsSucceedCommand;
            this.LowThirdsSucceedCommand = this.existingCommand.LowThirdsSucceedCommand;
            this.NoneSucceedCommand = this.existingCommand.NoneSucceedCommand;
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            this.UserPayout = this.UserPayout / 100.0;
            this.SubscriberPayout = this.SubscriberPayout / 100.0;
            this.ModPayout = this.ModPayout / 100.0;

            Dictionary<MixerRoleEnum, double> successRolePayouts = new Dictionary<MixerRoleEnum, double>() { { MixerRoleEnum.User, this.UserPayout }, { MixerRoleEnum.Subscriber, this.SubscriberPayout }, { MixerRoleEnum.Mod, this.ModPayout } };
            Dictionary<MixerRoleEnum, int> successRoleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, this.UserProbability }, { MixerRoleEnum.Subscriber, this.SubscriberProbability }, { MixerRoleEnum.Mod, this.ModProbability } };
            Dictionary<MixerRoleEnum, int> failRoleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 100 - this.UserProbability }, { MixerRoleEnum.Subscriber, 100 - this.SubscriberProbability }, { MixerRoleEnum.Mod, 100 - this.ModProbability } };

            GameCommandBase newCommand = new HeistGameCommand(name, triggers, requirements, this.MinimumParticipants, this.TimeLimit, this.StartedCommand, this.UserJoinCommand,
                new GameOutcome("Success", successRolePayouts, successRoleProbabilities, this.UserSuccessCommand), new GameOutcome("Failure", 0, failRoleProbabilities, this.UserFailCommand),
                this.AllSucceedCommand, this.TopThirdsSucceedCommand, this.MiddleThirdsSucceedCommand, this.LowThirdsSucceedCommand, this.NoneSucceedCommand, this.NotEnoughPlayersCommand);
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

            if (this.UserProbability < 0 || this.UserProbability > 100)
            {
                await DialogHelper.ShowMessage("The User Chance %'s is not a valid number between 0 - 100");
                return false;
            }

            if (this.SubscriberProbability < 0 || this.SubscriberProbability > 100)
            {
                await DialogHelper.ShowMessage("The Sub Chance %'s is not a valid number between 0 - 100");
                return false;
            }

            if (this.ModProbability < 0 || this.ModProbability > 100)
            {
                await DialogHelper.ShowMessage("The Mod Chance %'s is not a valid number between 0 - 100");
                return false;
            }

            return true;
        }
    }
}
