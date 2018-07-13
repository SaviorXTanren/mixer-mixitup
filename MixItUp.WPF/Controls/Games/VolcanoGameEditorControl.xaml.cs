using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for VolcanoGameEditorControl.xaml
    /// </summary>
    public partial class VolcanoGameEditorControl : GameEditorControlBase
    {
        private VolcanoGameCommand existingCommand;

        private CustomCommand stage1DepositCommand { get; set; }
        private CustomCommand stage1StatusCommand { get; set; }
        private CustomCommand stage2DepositCommand { get; set; }
        private CustomCommand stage2StatusCommand { get; set; }
        private CustomCommand stage3DepositCommand { get; set; }
        private CustomCommand stage3StatusCommand { get; set; }
        private CustomCommand payoutCommand { get; set; }
        private CustomCommand collectCommand { get; set; }

        public VolcanoGameEditorControl()
        {
            InitializeComponent();
        }

        public VolcanoGameEditorControl(VolcanoGameCommand command)
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

            if (string.IsNullOrEmpty(this.StatusArgumentTextBox.Text) && !this.StatusArgumentTextBox.Text.Any(c => char.IsLetterOrDigit(c)))
            {
                await MessageBoxHelper.ShowMessageDialog("The Status Argument must have a valid value");
                return false;
            }

            if (!int.TryParse(this.Stage2MinimumAmountTextBox.Text, out int stage2Minimum) || stage2Minimum <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Stage 2 Min Amount is not a valid number greater than 0");
                return false;
            }

            if (!int.TryParse(this.Stage3MinimumAmountTextBox.Text, out int stage3Minimum) || stage3Minimum <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Stage 3 Min Amount is not a valid number greater than 0");
                return false;
            }

            if (!int.TryParse(this.PayoutProbabilityTextBox.Text, out int payoutProbability) || payoutProbability <= 0 || payoutProbability > 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The Payout Probability is not a valid number between 0 - 100");
                return false;
            }

            if (!int.TryParse(this.PayoutPercentageMinimumLimitTextBox.Text, out int minPayoutPercentage) || minPayoutPercentage < 0 || minPayoutPercentage > 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The Min Payout % is not a valid number between 0 - 100");
                return false;
            }

            if (!int.TryParse(this.PayoutPercentageMaximumLimitTextBox.Text, out int maxPayoutPercentage) || maxPayoutPercentage < 0 || maxPayoutPercentage > 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The Max Payout % is not a valid number between 0 - 100");
                return false;
            }

            if (maxPayoutPercentage < minPayoutPercentage)
            {
                await MessageBoxHelper.ShowMessageDialog("The Max Payout % can not be less than Min Payout %");
                return false;
            }

            if (string.IsNullOrEmpty(this.CollectArgumentTextBox.Text) && !this.CollectArgumentTextBox.Text.Any(c => char.IsLetterOrDigit(c)))
            {
                await MessageBoxHelper.ShowMessageDialog("The Collect Argument must have a valid value");
                return false;
            }

            if (!int.TryParse(this.CollectTimeLimitTextBox.Text, out int collectTime) || collectTime < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Collect Time Out must be greater than 0");
                return false;
            }

            if (!int.TryParse(this.CollectPayoutPercentageMinimumLimitTextBox.Text, out int collectMinPayoutPercentage) || collectMinPayoutPercentage < 0 || collectMinPayoutPercentage > 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The Collect Min Payout % is not a valid number between 0 - 100");
                return false;
            }

            if (!int.TryParse(this.CollectPayoutPercentageMaximumLimitTextBox.Text, out int collectMaxPayoutPercentage) || collectMaxPayoutPercentage < 0 || collectMaxPayoutPercentage > 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The Collect Max Payout % is not a valid number between 0 - 100");
                return false;
            }

            return true;
        }

        public override void SaveGameCommand()
        {
            int.TryParse(this.Stage2MinimumAmountTextBox.Text, out int stage2Minimum);
            int.TryParse(this.Stage3MinimumAmountTextBox.Text, out int stage3Minimum);
            int.TryParse(this.PayoutProbabilityTextBox.Text, out int payoutProbability);
            double.TryParse(this.PayoutPercentageMinimumLimitTextBox.Text, out double minPayoutPercentage);
            double.TryParse(this.PayoutPercentageMaximumLimitTextBox.Text, out double maxPayoutPercentage);
            int.TryParse(this.CollectTimeLimitTextBox.Text, out int collectTime);
            double.TryParse(this.CollectPayoutPercentageMinimumLimitTextBox.Text, out double collectMinPayoutPercentage);
            double.TryParse(this.CollectPayoutPercentageMaximumLimitTextBox.Text, out double collectMaxPayoutPercentage);

            minPayoutPercentage = minPayoutPercentage / 100.0;
            maxPayoutPercentage = maxPayoutPercentage / 100.0;
            collectMinPayoutPercentage = collectMinPayoutPercentage / 100.0;
            collectMaxPayoutPercentage = collectMaxPayoutPercentage / 100.0;

            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
            }
            ChannelSession.Settings.GameCommands.Add(new VolcanoGameCommand(this.CommandDetailsControl.GameName, this.CommandDetailsControl.ChatTriggers, this.CommandDetailsControl.GetRequirements(),
                this.StatusArgumentTextBox.Text.ToLower(), this.stage1DepositCommand, this.stage1StatusCommand, stage2Minimum, this.stage2DepositCommand, this.stage2StatusCommand, stage3Minimum,
                this.stage3DepositCommand, this.stage3StatusCommand, payoutProbability, minPayoutPercentage, maxPayoutPercentage, this.payoutCommand, this.CollectArgumentTextBox.Text,
                collectTime, collectMinPayoutPercentage, collectMaxPayoutPercentage, this.collectCommand));
        }

        protected override Task OnLoaded()
        {
            if (this.existingCommand != null)
            {
                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);
                this.StatusArgumentTextBox.Text = this.existingCommand.StatusArgument;
                this.stage1DepositCommand = this.existingCommand.Stage1DepositCommand;
                this.stage1StatusCommand = this.existingCommand.Stage1StatusCommand;
                this.Stage2MinimumAmountTextBox.Text = this.existingCommand.Stage2MinimumAmount.ToString();
                this.stage2DepositCommand = this.existingCommand.Stage2DepositCommand;
                this.stage2StatusCommand = this.existingCommand.Stage2StatusCommand;
                this.Stage3MinimumAmountTextBox.Text = this.existingCommand.Stage3MinimumAmount.ToString();
                this.stage3DepositCommand = this.existingCommand.Stage3DepositCommand;
                this.stage3StatusCommand = this.existingCommand.Stage3StatusCommand;

                this.PayoutProbabilityTextBox.Text = this.existingCommand.PayoutProbability.ToString();
                this.PayoutPercentageMinimumLimitTextBox.Text = (this.existingCommand.PayoutPercentageMinimum * 100).ToString();
                this.PayoutPercentageMaximumLimitTextBox.Text = (this.existingCommand.PayoutPercentageMaximum * 100).ToString();
                this.payoutCommand = this.existingCommand.PayoutCommand;

                this.CollectArgumentTextBox.Text = this.existingCommand.CollectArgument;
                this.CollectTimeLimitTextBox.Text = this.existingCommand.CollectTimeLimit.ToString();
                this.CollectPayoutPercentageMinimumLimitTextBox.Text = (this.existingCommand.CollectPayoutPercentageMinimum * 100).ToString();
                this.CollectPayoutPercentageMaximumLimitTextBox.Text = (this.existingCommand.CollectPayoutPercentageMaximum * 100).ToString();
                this.collectCommand = this.existingCommand.CollectCommand;
            }
            else
            {
                this.CommandDetailsControl.SetDefaultValues("Volcano", "volcano", CurrencyRequirementTypeEnum.MinimumAndMaximum, 10, 1000);
                UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault();
                this.StatusArgumentTextBox.Text = "status";
                this.stage1DepositCommand = this.CreateBasicChatCommand("After a few seconds, @$username hears a faint clunk as their " + currency.Name + " hit the bottom of the volcano");
                this.stage1StatusCommand = this.CreateBasicChatCommand("Peering in, you can hardly see anything inside. Total Amount: $gametotalamount");
                this.Stage2MinimumAmountTextBox.Text = "5000";
                this.stage2DepositCommand = this.CreateBasicChatCommand("@$username hears a loud shuffling of " + currency.Name + " as their deposit goes in to the volcano");
                this.stage2StatusCommand = this.CreateBasicChatCommand("Peering in, you see the opening filled up over halfway inside the Volcano. Total Amount: $gametotalamount");
                this.Stage3MinimumAmountTextBox.Text = "10000";
                this.stage3DepositCommand = this.CreateBasicChatCommand("@$username carefully places their " + currency.Name + " into the volcano, trying not to knock over the overflowing amount already in it.");
                this.stage3StatusCommand = this.CreateBasicChatCommand("The  " + currency.Name + " are starting to overflow from the top of the Volcano. Total Amount: $gametotalamount");

                this.PayoutProbabilityTextBox.Text = "10";
                this.PayoutPercentageMinimumLimitTextBox.Text = "40";
                this.PayoutPercentageMaximumLimitTextBox.Text = "60";
                this.payoutCommand = this.CreateBasic2ChatCommand("As @$username drops their " + currency.Name + " into the Volcano, a loud eruption occurs and $gamepayout " + currency.Name + " land on top of them!",
                    "The Volcano is exploding out coins! Quick, type \"!volcano collect\" in chat in the next 30 seconds!");

                this.CollectArgumentTextBox.Text = "collect";
                this.CollectTimeLimitTextBox.Text = "30";
                this.CollectPayoutPercentageMinimumLimitTextBox.Text = "10";
                this.CollectPayoutPercentageMaximumLimitTextBox.Text = "20";
                this.collectCommand = this.CreateBasicChatCommand("@$username after scavenging the aftermath, you walk away with $gamepayout " + currency.Name + "!", whisper: true);
            }

            this.Stage1DepositCommandButtonsControl.DataContext = this.stage1DepositCommand;
            this.Stage1StatusCommandButtonsControl.DataContext = this.stage1StatusCommand;
            this.Stage2DepositCommandButtonsControl.DataContext = this.stage2DepositCommand;
            this.Stage2StatusCommandButtonsControl.DataContext = this.stage2StatusCommand;
            this.Stage3DepositCommandButtonsControl.DataContext = this.stage3DepositCommand;
            this.Stage3StatusCommandButtonsControl.DataContext = this.stage3StatusCommand;
            this.PayoutCommandButtonsControl.DataContext = this.payoutCommand;
            this.CollectPayoutCommandButtonsControl.DataContext = this.collectCommand;

            return base.OnLoaded();
        }
    }
}
