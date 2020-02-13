using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class DuelGameEditorControlViewModel : GameEditorControlViewModelBase
    {
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

        public string UserPercentageString
        {
            get { return this.UserPercentage.ToString(); }
            set
            {
                this.UserPercentage = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int UserPercentage { get; set; } = 50;

        public string SubscriberPercentageString
        {
            get { return this.SubscriberPercentage.ToString(); }
            set
            {
                this.SubscriberPercentage = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int SubscriberPercentage { get; set; } = 50;

        public string ModPercentageString
        {
            get { return this.ModPercentage.ToString(); }
            set
            {
                this.ModPercentage = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int ModPercentage { get; set; } = 50;

        public CustomCommand StartedCommand { get; set; }
        public CustomCommand NotAcceptedCommand { get; set; }
        public CustomCommand SuccessOutcomeCommand { get; set; }
        public CustomCommand FailOutcomeCommand { get; set; }

        private DuelGameCommand existingCommand;

        public DuelGameEditorControlViewModel(UserCurrencyModel currency)
        {
            this.StartedCommand = this.CreateBasicChatCommand("@$username has challenged @$targetusername to a duel for $gamebet " + currency.Name + "! Type !duel in chat to accept!");
            this.NotAcceptedCommand = this.CreateBasicChatCommand("@$targetusername did not respond in time...");
            this.SuccessOutcomeCommand = this.CreateBasicChatCommand("@$username won the duel against @$targetusername, winning $gamepayout " + currency.Name + "!");
            this.FailOutcomeCommand = this.CreateBasicChatCommand("@$targetusername defeated @$username, winning $gamepayout " + currency.Name + "!");
        }

        public DuelGameEditorControlViewModel(DuelGameCommand command)
        {
            this.existingCommand = command;

            this.TimeLimit = this.existingCommand.TimeLimit;
            this.UserPercentage = this.existingCommand.SuccessfulOutcome.RoleProbabilities[UserRoleEnum.User];
            this.SubscriberPercentage = this.existingCommand.SuccessfulOutcome.RoleProbabilities[UserRoleEnum.Subscriber];
            this.ModPercentage = this.existingCommand.SuccessfulOutcome.RoleProbabilities[UserRoleEnum.Mod];

            this.StartedCommand = this.existingCommand.StartedCommand;
            this.NotAcceptedCommand = this.existingCommand.NotAcceptedCommand;
            this.SuccessOutcomeCommand = this.existingCommand.SuccessfulOutcome.Command;
            this.FailOutcomeCommand = this.existingCommand.FailedOutcome.Command;
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            Dictionary<UserRoleEnum, int> successRoleProbabilities = new Dictionary<UserRoleEnum, int>() { { UserRoleEnum.User, this.UserPercentage }, { UserRoleEnum.Subscriber, this.SubscriberPercentage }, { UserRoleEnum.Mod, this.ModPercentage } };
            Dictionary<UserRoleEnum, int> failRoleProbabilities = new Dictionary<UserRoleEnum, int>() { { UserRoleEnum.User, 100 - this.UserPercentage }, { UserRoleEnum.Subscriber, 100 - this.SubscriberPercentage }, { UserRoleEnum.Mod, 100 - this.ModPercentage } };

            GameCommandBase newCommand = new DuelGameCommand(name, triggers, requirements, new GameOutcome("Success", 1, successRoleProbabilities, this.SuccessOutcomeCommand),
                new GameOutcome("Failure", 0, failRoleProbabilities, this.FailOutcomeCommand), this.StartedCommand, this.TimeLimit, this.NotAcceptedCommand);
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        public override async Task<bool> Validate()
        {
            if (this.TimeLimit < 0)
            {
                await DialogHelper.ShowMessage("The Time Limit is not a valid number greater than 0");
                return false;
            }

            if (this.UserPercentage < 0 || this.UserPercentage > 100)
            {
                await DialogHelper.ShowMessage("The User Chance %'s is not a valid number between 0 - 100");
                return false;
            }

            if (this.SubscriberPercentage < 0 || this.SubscriberPercentage > 100)
            {
                await DialogHelper.ShowMessage("The Sub Chance %'s is not a valid number between 0 - 100");
                return false;
            }

            if (this.ModPercentage < 0 || ModPercentage > 100)
            {
                await DialogHelper.ShowMessage("The Mod Chance %'s is not a valid number between 0 - 100");
                return false;
            }

            return true;
        }
    }
}
