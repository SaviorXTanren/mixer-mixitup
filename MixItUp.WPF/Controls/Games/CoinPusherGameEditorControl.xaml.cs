using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for CoinPusherGameEditorControl.xaml
    /// </summary>
    public partial class CoinPusherGameEditorControl : GameEditorControlBase
    {
        private CoinPusherGameCommand existingCommand;

        private CustomCommand statusCommand { get; set; }
        private CustomCommand noPayoutCommand { get; set; }
        private CustomCommand payoutCommand { get; set; }

        public CoinPusherGameEditorControl()
        {
            InitializeComponent();
        }

        public CoinPusherGameEditorControl(CoinPusherGameCommand command)
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

            if (!int.TryParse(this.MinimumAmountForPayoutTextBox.Text, out int minAmountForPayout) || minAmountForPayout <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Min Amount for Payout is not a valid number greater than 0");
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

            if (string.IsNullOrEmpty(this.StatusArgumentTextBox.Text) && !this.StatusArgumentTextBox.Text.Any(c => char.IsLetterOrDigit(c)))
            {
                await MessageBoxHelper.ShowMessageDialog("The Status Argument must have a valid value");
                return false;
            }

            return true;
        }

        public override void SaveGameCommand()
        {
            int.TryParse(this.MinimumAmountForPayoutTextBox.Text, out int minAmountForPayout);
            int.TryParse(this.PayoutProbabilityTextBox.Text, out int payoutProbability);
            double.TryParse(this.PayoutPercentageMinimumLimitTextBox.Text, out double minPayoutPercentage);
            double.TryParse(this.PayoutPercentageMaximumLimitTextBox.Text, out double maxPayoutPercentage);

            minPayoutPercentage = minPayoutPercentage / 100.0;
            maxPayoutPercentage = maxPayoutPercentage / 100.0;

            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
            }
            ChannelSession.Settings.GameCommands.Add(new CoinPusherGameCommand(this.CommandDetailsControl.GameName, this.CommandDetailsControl.ChatTriggers, this.CommandDetailsControl.GetRequirements(),
                this.StatusArgumentTextBox.Text.ToLower(), minAmountForPayout, payoutProbability, minPayoutPercentage, maxPayoutPercentage, this.statusCommand, this.noPayoutCommand, this.payoutCommand));
        }

        protected override Task OnLoaded()
        {
            if (this.existingCommand != null)
            {
                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);
                this.MinimumAmountForPayoutTextBox.Text = this.existingCommand.MinimumAmountForPayout.ToString();
                this.PayoutProbabilityTextBox.Text = this.existingCommand.PayoutProbability.ToString();
                this.PayoutPercentageMinimumLimitTextBox.Text = (this.existingCommand.PayoutPercentageMinimum * 100).ToString();
                this.PayoutPercentageMaximumLimitTextBox.Text = (this.existingCommand.PayoutPercentageMaximum * 100).ToString();

                this.noPayoutCommand = this.existingCommand.NoPayoutCommand;
                this.payoutCommand = this.existingCommand.PayoutCommand;

                this.StatusArgumentTextBox.Text = this.existingCommand.StatusArgument;
                this.statusCommand = this.existingCommand.StatusCommand;
            }
            else
            {
                this.CommandDetailsControl.SetDefaultValues("Coin Pusher", "pusher", CurrencyRequirementTypeEnum.MinimumAndMaximum, 10, 1000);
                UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault();
                this.MinimumAmountForPayoutTextBox.Text = "2500";
                this.PayoutProbabilityTextBox.Text = "10";
                this.PayoutPercentageMinimumLimitTextBox.Text = "30";
                this.PayoutPercentageMaximumLimitTextBox.Text = "70";

                this.noPayoutCommand = this.CreateBasicChatCommand("@$username drops their coins into the machine...and nothing happens. All $gametotalamount " + currency.Name + " stares back at you.");
                this.payoutCommand = this.CreateBasicChatCommand("@$username drops their coins into the machine...and hits the jackpot, walking away with $gamepayout " + currency.Name + "!");

                this.StatusArgumentTextBox.Text = "status";
                this.statusCommand = this.CreateBasicChatCommand("After spending a few minutes, you count $gametotalamount " + currency.Name + " inside the machine.");
            }

            this.NoPayoutCommandButtonsControl.DataContext = this.noPayoutCommand;
            this.PayoutCommandButtonsControl.DataContext = this.payoutCommand;
            this.StatusCommandButtonsControl.DataContext = this.statusCommand;

            return base.OnLoaded();
        }
    }
}
