using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for HeistGameEditorControl.xaml
    /// </summary>
    public partial class HeistGameEditorControl : GameEditorControlBase
    {
        private HeistGameCommand existingCommand;

        private CustomCommand startedCommand { get; set; }

        private CustomCommand userJoinCommand { get; set; }

        private CustomCommand userSuccessCommand { get; set; }
        private CustomCommand userFailCommand { get; set; }

        private CustomCommand allSucceedCommand { get; set; }
        private CustomCommand topThirdsSucceedCommand { get; set; }
        private CustomCommand middleThirdsSucceedCommand { get; set; }
        private CustomCommand lowThirdsSucceedCommand { get; set; }
        private CustomCommand noneSucceedCommand { get; set; }

        public HeistGameEditorControl()
        {
            InitializeComponent();
        }

        public HeistGameEditorControl(HeistGameCommand command)
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

            if (!int.TryParse(this.UserPercentageTextBox.Text, out int userChance) || userChance < 0 || userChance > 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The User Chance %'s is not a valid number between 0 - 100");
                return false;
            }

            if (!int.TryParse(this.SubscriberPercentageTextBox.Text, out int subscriberChance) || subscriberChance < 0 || subscriberChance > 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The Sub Chance %'s is not a valid number between 0 - 100");
                return false;
            }

            if (!int.TryParse(this.ModPercentageTextBox.Text, out int modChance) || modChance < 0 || modChance > 100)
            {
                await MessageBoxHelper.ShowMessageDialog("The Mod Chance %'s is not a valid number between 0 - 100");
                return false;
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

            int.TryParse(this.UserPercentageTextBox.Text, out int userChance);
            int.TryParse(this.SubscriberPercentageTextBox.Text, out int subscriberChance);
            int.TryParse(this.ModPercentageTextBox.Text, out int modChance);

            userPayout = userPayout / 100;
            subscriberPayout = subscriberPayout / 100;
            modPayout = modPayout / 100;

            Dictionary<MixerRoleEnum, double> successRolePayouts = new Dictionary<MixerRoleEnum, double>() { { MixerRoleEnum.User, userPayout }, { MixerRoleEnum.Subscriber, subscriberPayout }, { MixerRoleEnum.Mod, modPayout } };
            Dictionary<MixerRoleEnum, int> successRoleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, userChance }, { MixerRoleEnum.Subscriber, subscriberChance }, { MixerRoleEnum.Mod, modChance } };
            Dictionary<MixerRoleEnum, int> failRoleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 100 - userChance }, { MixerRoleEnum.Subscriber, 100 - subscriberChance }, { MixerRoleEnum.Mod, 100 - modChance } };

            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
            }
            ChannelSession.Settings.GameCommands.Add(new HeistGameCommand(this.CommandDetailsControl.GameName, this.CommandDetailsControl.ChatTriggers,
                this.CommandDetailsControl.GetRequirements(), minimumParticipants, timeLimit, this.startedCommand, this.userJoinCommand,
                new GameOutcome("Success", successRolePayouts, successRoleProbabilities, this.userSuccessCommand), new GameOutcome("Failure", 0, failRoleProbabilities, this.userFailCommand),
                this.allSucceedCommand, this.topThirdsSucceedCommand, this.middleThirdsSucceedCommand, this.lowThirdsSucceedCommand, this.noneSucceedCommand));
        }

        protected override Task OnLoaded()
        {
            if (this.existingCommand != null)
            {
                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);

                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);
                this.MinimumParticipantsTextBox.Text = this.existingCommand.MinimumParticipants.ToString();
                this.TimeLimitTextBox.Text = this.existingCommand.TimeLimit.ToString();
                this.UserPayoutTextBox.Text = (this.existingCommand.UserSuccessOutcome.RolePayouts[MixerRoleEnum.User] * 100).ToString();
                this.SubscriberPayoutTextBox.Text = (this.existingCommand.UserSuccessOutcome.RolePayouts[MixerRoleEnum.Subscriber] * 100).ToString();
                this.ModPayoutTextBox.Text = (this.existingCommand.UserSuccessOutcome.RolePayouts[MixerRoleEnum.Mod] * 100).ToString();
                this.UserPercentageTextBox.Text = this.existingCommand.UserSuccessOutcome.RoleProbabilities[MixerRoleEnum.User].ToString();
                this.SubscriberPercentageTextBox.Text = this.existingCommand.UserSuccessOutcome.RoleProbabilities[MixerRoleEnum.Subscriber].ToString();
                this.ModPercentageTextBox.Text = this.existingCommand.UserSuccessOutcome.RoleProbabilities[MixerRoleEnum.Mod].ToString();

                this.startedCommand = this.existingCommand.StartedCommand;

                this.userJoinCommand = this.existingCommand.UserJoinCommand;

                this.userSuccessCommand = this.existingCommand.UserSuccessOutcome.Command;
                this.userFailCommand = this.existingCommand.UserFailOutcome.Command;

                this.allSucceedCommand = this.existingCommand.AllSucceedCommand;
                this.topThirdsSucceedCommand = this.existingCommand.TopThirdsSucceedCommand;
                this.middleThirdsSucceedCommand = this.existingCommand.MiddleThirdsSucceedCommand;
                this.lowThirdsSucceedCommand = this.existingCommand.LowThirdsSucceedCommand;
                this.noneSucceedCommand = this.existingCommand.NoneSucceedCommand;
            }
            else
            {
                this.CommandDetailsControl.SetDefaultValues("Heist", "heist", CurrencyRequirementTypeEnum.MinimumAndMaximum, 10, 1000);
                UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault();
                this.MinimumParticipantsTextBox.Text = "2";
                this.TimeLimitTextBox.Text = "30";
                this.UserPayoutTextBox.Text = "200";
                this.SubscriberPayoutTextBox.Text = "200";
                this.ModPayoutTextBox.Text = "200";
                this.UserPercentageTextBox.Text = "60";
                this.SubscriberPercentageTextBox.Text = "60";
                this.ModPercentageTextBox.Text = "60";

                this.startedCommand = this.CreateBasicChatCommand("@$username has started a game of Heist! Type !heist <AMOUNT> to join in!");

                this.userJoinCommand = this.CreateBasicChatCommand("You've joined in the heist! Let's see how it turns out...", whisper: true);

                this.userSuccessCommand = this.CreateBasicChatCommand("Congrats, you made out with $gamepayout " + currency.Name + "!", whisper: true);
                this.userFailCommand = this.CreateBasicChatCommand("The cops caught you before you could make it out! Better luck next time...", whisper: true);

                this.allSucceedCommand = this.CreateBasic2ChatCommand("What a steal! Everyone made it out and cleaned the bank out dry! Total Amount: $gameallpayout " + currency.Name + "!", "Winners: $gamewinners");
                this.topThirdsSucceedCommand = this.CreateBasic2ChatCommand("The cops showed up at the last second and snagged a few of you, but most made it out with the good! Total Amount: $gameallpayout " + currency.Name + "!", "Winners: $gamewinners");
                this.middleThirdsSucceedCommand = this.CreateBasic2ChatCommand("As you started to leave the bank, the cops were ready for you and got almost half of you! Total Amount: $gameallpayout " + currency.Name + "!", "Winners: $gamewinners");
                this.lowThirdsSucceedCommand = this.CreateBasic2ChatCommand("A heated battle took place inside the bank and almost everyone got caught by the cops! Total Amount: $gameallpayout " + currency.Name + "!", "Winners: $gamewinners");
                this.noneSucceedCommand = this.CreateBasicChatCommand("Someone was a spy! The cops were waiting for you as soon as you showed up and got everyone!");
            }

            this.GameStartCommandButtonsControl.DataContext = this.startedCommand;
            this.UserJoinedCommandButtonsControl.DataContext = this.userJoinCommand;
            this.SuccessOutcomeCommandButtonsControl.DataContext = this.userSuccessCommand;
            this.FailOutcomeCommandButtonsControl.DataContext = this.userFailCommand;
            this.AllSucceedCommandButtonsControl.DataContext = this.allSucceedCommand;
            this.TopThirdsSucceedCommandButtonsControl.DataContext = this.topThirdsSucceedCommand;
            this.MiddleThirdsSucceedCommandButtonsControl.DataContext = this.middleThirdsSucceedCommand;
            this.BottomThirdsSucceedCommandButtonsControl.DataContext = this.lowThirdsSucceedCommand;
            this.NoneSucceedCommandButtonsControl.DataContext = this.noneSucceedCommand;

            return base.OnLoaded();
        }
    }
}
