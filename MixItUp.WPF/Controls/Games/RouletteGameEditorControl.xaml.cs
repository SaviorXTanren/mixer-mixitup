using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for RouletteGameEditorControl.xaml
    /// </summary>
    public partial class RouletteGameEditorControl : GameEditorControlBase
    {
        private RouletteGameCommand existingCommand;

        private CustomCommand startedCommand { get; set; }

        private CustomCommand userJoinCommand { get; set; }

        private CustomCommand userSuccessCommand { get; set; }
        private CustomCommand userFailCommand { get; set; }

        private CustomCommand gameCompleteCommand { get; set; }

        public RouletteGameEditorControl()
        {
            InitializeComponent();
        }

        public RouletteGameEditorControl(RouletteGameCommand command)
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

            if (!int.TryParse(this.TimeLimitTextBox.Text, out int timeLimit) || timeLimit <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Time Limit is not a valid number greater than 0");
                return false;
            }

            if (!int.TryParse(this.MinimumParticipantsTextBox.Text, out int minimumParticipants) || minimumParticipants <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Minimum Participants is not a valid number greater than 0");
                return false;
            }

            if (!int.TryParse(this.UserPayoutTextBox.Text, out int userPayout) || userPayout < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The User Payout %'s is not a valid number greater than or equal to 0");
                return false;
            }

            if (!int.TryParse(this.SubscriberPayoutTextBox.Text, out int subscriberPayout) || subscriberPayout < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Subscriber Payout %'s is not a valid number greater than or equal to 0");
                return false;
            }

            if (!int.TryParse(this.ModPayoutTextBox.Text, out int modPayout) || modPayout < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Mod Payout %'s is not a valid number greater than or equal to 0");
                return false;
            }

            if (this.IsNumberRangeToggleButton.IsChecked.GetValueOrDefault())
            {
                if (!int.TryParse(this.NumberRangeMinimumTextBox.Text, out int minNumber) || minNumber <= 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The Min Number is not a valid number greater than 0");
                    return false;
                }

                if (!int.TryParse(this.NumberRangeMaximumTextBox.Text, out int maxNumber) || maxNumber <= 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("The Max Number is not a valid number greater than 0");
                    return false;
                }

                if (maxNumber < minNumber)
                {
                    await MessageBoxHelper.ShowMessageDialog("The Max Number can not be less than the Min Number");
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(this.SelectableBetTypesTextBox.Text))
                {
                    await MessageBoxHelper.ShowMessageDialog("The Valid Bet Types does not have a value");
                    return false;
                }

                HashSet<string> validBetTypes = new HashSet<string>();
                foreach (string betType in this.SelectableBetTypesTextBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    validBetTypes.Add(betType.ToLower());
                }

                if (validBetTypes.Count() < 2)
                {
                    await MessageBoxHelper.ShowMessageDialog("You must specify at least 2 different bet types");
                    return false;
                }
            }

            return true;
        }

        public override void SaveGameCommand()
        {
            int.TryParse(this.MinimumParticipantsTextBox.Text, out int minimumParticipants);
            int.TryParse(this.TimeLimitTextBox.Text, out int timeLimit);

            double.TryParse(this.UserPayoutTextBox.Text, out double userPayout);
            double.TryParse(this.SubscriberPayoutTextBox.Text, out double subscriberPayout);
            double.TryParse(this.ModPayoutTextBox.Text, out double modPayout);

            HashSet<string> validBetTypes = new HashSet<string>();
            if (this.IsNumberRangeToggleButton.IsChecked.GetValueOrDefault())
            {
                int.TryParse(this.NumberRangeMinimumTextBox.Text, out int minNumber);
                int.TryParse(this.NumberRangeMaximumTextBox.Text, out int maxNumber);
                for (int i = minNumber; i <= maxNumber; i++)
                {
                    validBetTypes.Add(i.ToString());
                }
            }
            else
            {
                foreach (string betType in this.SelectableBetTypesTextBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    validBetTypes.Add(betType.ToLower());
                }
            }

            userPayout = userPayout / 100.0;
            subscriberPayout = subscriberPayout / 100.0;
            modPayout = modPayout / 100.0;

            Dictionary<MixerRoleEnum, double> successRolePayouts = new Dictionary<MixerRoleEnum, double>() { { MixerRoleEnum.User, userPayout }, { MixerRoleEnum.Subscriber, subscriberPayout }, { MixerRoleEnum.Mod, modPayout } };
            Dictionary<MixerRoleEnum, int> roleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 0 }, { MixerRoleEnum.Subscriber, 0 }, { MixerRoleEnum.Mod, 0 } };

            GameCommandBase newCommand = new RouletteGameCommand(this.CommandDetailsControl.GameName, this.CommandDetailsControl.ChatTriggers,
                this.CommandDetailsControl.GetRequirements(), minimumParticipants, timeLimit, this.IsNumberRangeToggleButton.IsChecked.GetValueOrDefault(), validBetTypes,
                this.startedCommand, this.userJoinCommand, new GameOutcome("Success", successRolePayouts, roleProbabilities, this.userSuccessCommand),
                new GameOutcome("Failure", 0, roleProbabilities, this.userFailCommand), this.gameCompleteCommand);
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        protected override Task OnLoaded()
        {
            this.IsNumberRangeToggleButton.IsChecked = true;

            if (this.existingCommand != null)
            {
                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);

                this.MinimumParticipantsTextBox.Text = this.existingCommand.MinimumParticipants.ToString();
                this.TimeLimitTextBox.Text = this.existingCommand.TimeLimit.ToString();
                this.UserPayoutTextBox.Text = (this.existingCommand.UserSuccessOutcome.RolePayouts[MixerRoleEnum.User] * 100).ToString();
                this.SubscriberPayoutTextBox.Text = (this.existingCommand.UserSuccessOutcome.RolePayouts[MixerRoleEnum.Subscriber] * 100).ToString();
                this.ModPayoutTextBox.Text = (this.existingCommand.UserSuccessOutcome.RolePayouts[MixerRoleEnum.Mod] * 100).ToString();

                this.startedCommand = this.existingCommand.StartedCommand;

                this.userJoinCommand = this.existingCommand.UserJoinCommand;

                this.IsNumberRangeToggleButton.IsChecked = this.existingCommand.IsNumberRange;
                if (this.existingCommand.IsNumberRange)
                {
                    IEnumerable<int> numberBetTypes = this.existingCommand.ValidBetTypes.Select(b => int.Parse(b));
                    this.NumberRangeMinimumTextBox.Text = numberBetTypes.Min().ToString();
                    this.NumberRangeMaximumTextBox.Text = numberBetTypes.Max().ToString();
                }
                else
                {
                    this.SelectableBetTypesTextBox.Text = string.Join(Environment.NewLine, this.existingCommand.ValidBetTypes);
                }

                this.userSuccessCommand = this.existingCommand.UserSuccessOutcome.Command;
                this.userFailCommand = this.existingCommand.UserFailOutcome.Command;

                this.gameCompleteCommand = this.existingCommand.GameCompleteCommand;
            }
            else
            {
                this.CommandDetailsControl.SetDefaultValues("Roulette", "roulette", CurrencyRequirementTypeEnum.MinimumAndMaximum, 10, 1000);
                UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault();
                this.MinimumParticipantsTextBox.Text = "2";
                this.TimeLimitTextBox.Text = "30";
                this.NumberRangeMinimumTextBox.Text = "1";
                this.NumberRangeMaximumTextBox.Text = "30";
                this.UserPayoutTextBox.Text = "200";
                this.SubscriberPayoutTextBox.Text = "200";
                this.ModPayoutTextBox.Text = "200";

                this.startedCommand = this.CreateBasic2ChatCommand("@$username has started a game of roulette! Type !roulette <BET TYPE> <AMOUNT> in chat to play!", "Valid Bet Types: $gamevalidbettypes");

                this.userJoinCommand = this.CreateBasicChatCommand("You slap you chips on the number $gamebettype as the ball starts to spin around the roulette wheel!", whisper: true);

                this.userSuccessCommand = this.CreateBasicChatCommand("Congrats, you made out with $gamepayout " + currency.Name + "!", whisper: true);
                this.userFailCommand = this.CreateBasicChatCommand("Lady luck wasn't with you today, better luck next time...", whisper: true);

                this.gameCompleteCommand = this.CreateBasicChatCommand("The wheel slows down, revealing $gamewinningbettype as the winning bet! Total Payout: $gameallpayout");
            }

            this.GameStartCommandButtonsControl.DataContext = this.startedCommand;
            this.UserJoinedCommandButtonsControl.DataContext = this.userJoinCommand;
            this.SuccessOutcomeCommandButtonsControl.DataContext = this.userSuccessCommand;
            this.FailOutcomeCommandButtonsControl.DataContext = this.userFailCommand;
            this.GameCompleteCommandButtonsControl.DataContext = this.gameCompleteCommand;

            return base.OnLoaded();
        }

        private void IsNumberRangeToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NumberRangeMinimumTextBox.Visibility = (this.IsNumberRangeToggleButton.IsChecked.GetValueOrDefault()) ? Visibility.Visible : Visibility.Collapsed;
            this.NumberRangeMaximumTextBox.Visibility = (this.IsNumberRangeToggleButton.IsChecked.GetValueOrDefault()) ? Visibility.Visible : Visibility.Collapsed;
            this.SelectableBetTypesTextBox.Visibility = (!this.IsNumberRangeToggleButton.IsChecked.GetValueOrDefault()) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
