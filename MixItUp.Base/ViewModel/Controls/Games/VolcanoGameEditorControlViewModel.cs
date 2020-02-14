using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class VolcanoGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public string StatusArgument { get; set; } = "status";

        public CustomCommand Stage1DepositCommand { get; set; }
        public CustomCommand Stage1StatusCommand { get; set; }

        public string Stage2MinimumAmountString
        {
            get { return this.Stage2MinimumAmount.ToString(); }
            set
            {
                this.Stage2MinimumAmount = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int Stage2MinimumAmount { get; set; } = 5000;
        public CustomCommand Stage2DepositCommand { get; set; }
        public CustomCommand Stage2StatusCommand { get; set; }

        public string Stage3MinimumAmountString
        {
            get { return this.Stage3MinimumAmount.ToString(); }
            set
            {
                this.Stage3MinimumAmount = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int Stage3MinimumAmount { get; set; } = 10000;
        public CustomCommand Stage3DepositCommand { get; set; }
        public CustomCommand Stage3StatusCommand { get; set; }

        public string PayoutProbabilityString
        {
            get { return this.PayoutProbability.ToString(); }
            set
            {
                this.PayoutProbability = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int PayoutProbability { get; set; } = 10;

        public string PayoutPercentageMinimumString
        {
            get { return this.PayoutPercentageMinimum.ToString(); }
            set
            {
                this.PayoutPercentageMinimum = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public double PayoutPercentageMinimum { get; set; } = 40;

        public string PayoutPercentageMaximumString
        {
            get { return this.PayoutPercentageMaximum.ToString(); }
            set
            {
                this.PayoutPercentageMaximum = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public double PayoutPercentageMaximum { get; set; } = 60;
        public CustomCommand PayoutCommand { get; set; }

        public string CollectArgument { get; set; } = "collect";
        public string CollectTimeLimitString
        {
            get { return this.CollectTimeLimit.ToString(); }
            set
            {
                this.CollectTimeLimit = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int CollectTimeLimit { get; set; } = 30;

        public string CollectPayoutPercentageMinimumString
        {
            get { return this.CollectPayoutPercentageMinimum.ToString(); }
            set
            {
                this.CollectPayoutPercentageMinimum = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public double CollectPayoutPercentageMinimum { get; set; } = 10;

        public string CollectPayoutPercentageMaximumString
        {
            get { return this.CollectPayoutPercentageMaximum.ToString(); }
            set
            {
                this.CollectPayoutPercentageMaximum = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public double CollectPayoutPercentageMaximum { get; set; } = 20;
        public CustomCommand CollectCommand { get; set; }

        private VolcanoGameCommand existingCommand;

        public VolcanoGameEditorControlViewModel(UserCurrencyModel currency)
        {
            this.Stage1DepositCommand = this.CreateBasicChatCommand("After a few seconds, @$username hears a faint clunk as their " + currency.Name + " hit the bottom of the volcano");
            this.Stage1StatusCommand = this.CreateBasicChatCommand("Peering in, you can hardly see anything inside. Total Amount: $gametotalamount");
            this.Stage2DepositCommand = this.CreateBasicChatCommand("@$username hears a loud shuffling of " + currency.Name + " as their deposit goes in to the volcano");
            this.Stage2StatusCommand = this.CreateBasicChatCommand("Peering in, you see the opening filled up over halfway inside the Volcano. Total Amount: $gametotalamount");
            this.Stage3DepositCommand = this.CreateBasicChatCommand("@$username carefully places their " + currency.Name + " into the volcano, trying not to knock over the overflowing amount already in it.");
            this.Stage3StatusCommand = this.CreateBasicChatCommand("The  " + currency.Name + " are starting to overflow from the top of the Volcano. Total Amount: $gametotalamount");

            this.PayoutCommand = this.CreateBasic2ChatCommand("As @$username drops their " + currency.Name + " into the Volcano, a loud eruption occurs and $gamepayout " + currency.Name + " land on top of them!",
                "The Volcano is exploding out coins! Quick, type \"!volcano collect\" in chat in the next 30 seconds!");

            this.CollectCommand = this.CreateBasicChatCommand("@$username after scavenging the aftermath, you walk away with $gamepayout " + currency.Name + "!", whisper: true);
        }

        public VolcanoGameEditorControlViewModel(VolcanoGameCommand command)
        {
            this.existingCommand = command;

            this.StatusArgument = this.existingCommand.StatusArgument;
            this.Stage1DepositCommand = this.existingCommand.Stage1DepositCommand;
            this.Stage1StatusCommand = this.existingCommand.Stage1StatusCommand;
            this.Stage2MinimumAmount = this.existingCommand.Stage2MinimumAmount;
            this.Stage2DepositCommand = this.existingCommand.Stage2DepositCommand;
            this.Stage2StatusCommand = this.existingCommand.Stage2StatusCommand;
            this.Stage3MinimumAmount = this.existingCommand.Stage3MinimumAmount;
            this.Stage3DepositCommand = this.existingCommand.Stage3DepositCommand;
            this.Stage3StatusCommand = this.existingCommand.Stage3StatusCommand;

            this.PayoutProbability = this.existingCommand.PayoutProbability;
            this.PayoutPercentageMinimum = (this.existingCommand.PayoutPercentageMinimum * 100);
            this.PayoutPercentageMaximum = (this.existingCommand.PayoutPercentageMaximum * 100);
            this.PayoutCommand = this.existingCommand.PayoutCommand;

            this.CollectArgument = this.existingCommand.CollectArgument;
            this.CollectTimeLimit = this.existingCommand.CollectTimeLimit;
            this.CollectPayoutPercentageMinimum = (this.existingCommand.CollectPayoutPercentageMinimum * 100);
            this.CollectPayoutPercentageMaximum = (this.existingCommand.CollectPayoutPercentageMaximum * 100);
            this.CollectCommand = this.existingCommand.CollectCommand;
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            this.PayoutPercentageMinimum = this.PayoutPercentageMinimum / 100.0;
            this.PayoutPercentageMaximum = this.PayoutPercentageMaximum / 100.0;
            this.CollectPayoutPercentageMinimum = this.CollectPayoutPercentageMinimum / 100.0;
            this.CollectPayoutPercentageMaximum = this.CollectPayoutPercentageMaximum / 100.0;

            GameCommandBase newCommand = new VolcanoGameCommand(name, triggers, requirements,
                this.StatusArgument.ToLower(), this.Stage1DepositCommand, this.Stage1StatusCommand, this.Stage2MinimumAmount, this.Stage2DepositCommand, this.Stage2StatusCommand, this.Stage3MinimumAmount,
                this.Stage3DepositCommand, this.Stage3StatusCommand, this.PayoutProbability, this.PayoutPercentageMinimum, this.PayoutPercentageMaximum, this.PayoutCommand, this.CollectArgument,
                this.CollectTimeLimit, this.CollectPayoutPercentageMinimum, this.CollectPayoutPercentageMaximum, this.CollectCommand);
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        public override async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(this.StatusArgument) && !this.StatusArgument.Any(c => char.IsLetterOrDigit(c)))
            {
                await DialogHelper.ShowMessage("The Status Argument must have a valid value");
                return false;
            }

            if (this.Stage2MinimumAmount <= 0)
            {
                await DialogHelper.ShowMessage("The Stage 2 Min Amount is not a valid number greater than 0");
                return false;
            }

            if (this.Stage3MinimumAmount <= 0)
            {
                await DialogHelper.ShowMessage("The Stage 3 Min Amount is not a valid number greater than 0");
                return false;
            }

            if (this.PayoutProbability <= 0 || this.PayoutProbability > 100)
            {
                await DialogHelper.ShowMessage("The Payout Probability is not a valid number between 0 - 100");
                return false;
            }

            if (this.PayoutPercentageMinimum < 0 || PayoutPercentageMinimum > 100)
            {
                await DialogHelper.ShowMessage("The Min Payout % is not a valid number between 0 - 100");
                return false;
            }

            if (this.PayoutPercentageMaximum < 0 || this.PayoutPercentageMaximum > 100)
            {
                await DialogHelper.ShowMessage("The Max Payout % is not a valid number between 0 - 100");
                return false;
            }

            if (this.PayoutPercentageMaximum < this.PayoutPercentageMinimum)
            {
                await DialogHelper.ShowMessage("The Max Payout % can not be less than Min Payout %");
                return false;
            }

            if (string.IsNullOrEmpty(this.CollectArgument) && !this.CollectArgument.Any(c => char.IsLetterOrDigit(c)))
            {
                await DialogHelper.ShowMessage("The Collect Argument must have a valid value");
                return false;
            }

            if (this.CollectTimeLimit < 0)
            {
                await DialogHelper.ShowMessage("The Collect Time Out must be greater than 0");
                return false;
            }

            if (this.CollectPayoutPercentageMinimum < 0 || this.CollectPayoutPercentageMinimum > 100)
            {
                await DialogHelper.ShowMessage("The Collect Min Payout % is not a valid number between 0 - 100");
                return false;
            }

            if (this.CollectPayoutPercentageMaximum < 0 || this.CollectPayoutPercentageMaximum > 100)
            {
                await DialogHelper.ShowMessage("The Collect Max Payout % is not a valid number between 0 - 100");
                return false;
            }

            if (this.CollectPayoutPercentageMaximum < this.CollectPayoutPercentageMaximum)
            {
                await DialogHelper.ShowMessage("The Collect Max Payout % can not be less than Collect Min Payout %");
                return false;
            }

            return true;
        }
    }
}
