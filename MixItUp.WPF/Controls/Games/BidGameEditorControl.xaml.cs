using Mixer.Base.Util;
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
    /// Interaction logic for BidGameEditorControl.xaml
    /// </summary>
    public partial class BidGameEditorControl : GameEditorControlBase
    {
        private BidGameCommand existingCommand;

        private CustomCommand startedCommand { get; set; }

        private CustomCommand userJoinCommand { get; set; }

        private CustomCommand gameCompleteCommand { get; set; }

        public BidGameEditorControl()
        {
            InitializeComponent();
        }

        public BidGameEditorControl(BidGameCommand command)
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

            if (this.GameStartRoleComboBox.SelectedIndex < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Who Can Start Game must have a valid User Role selection");
                return false;
            }

            return true;
        }

        public override void SaveGameCommand()
        {
            int.TryParse(this.MinimumParticipantsTextBox.Text, out int minimumParticipants);
            int.TryParse(this.TimeLimitTextBox.Text, out int timeLimit);
            RoleRequirementViewModel starterRequirement = new RoleRequirementViewModel((string)this.GameStartRoleComboBox.SelectedItem);

            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
            }
            ChannelSession.Settings.GameCommands.Add(new BidGameCommand(this.CommandDetailsControl.GameName, this.CommandDetailsControl.ChatTriggers,
                this.CommandDetailsControl.GetRequirements(), minimumParticipants, timeLimit, starterRequirement, this.startedCommand, this.userJoinCommand, this.gameCompleteCommand));
        }

        protected override Task OnLoaded()
        {
            this.CommandDetailsControl.SetAsMinimumOnly();

            this.GameStartRoleComboBox.ItemsSource = RoleRequirementViewModel.AdvancedUserRoleAllowedValues;
            this.GameStartRoleComboBox.SelectedItem = EnumHelper.GetEnumName(MixerRoleEnum.Mod);

            if (this.existingCommand != null)
            {
                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);
                this.MinimumParticipantsTextBox.Text = this.existingCommand.MinimumParticipants.ToString();
                this.TimeLimitTextBox.Text = this.existingCommand.TimeLimit.ToString();
                this.GameStartRoleComboBox.SelectedItem = this.existingCommand.GameStarterRequirement.RoleNameString;

                this.startedCommand = this.existingCommand.StartedCommand;

                this.userJoinCommand = this.existingCommand.UserJoinCommand;

                this.gameCompleteCommand = this.existingCommand.GameCompleteCommand;
            }
            else
            {
                this.CommandDetailsControl.SetDefaultValues("Bid", "bid", CurrencyRequirementTypeEnum.MinimumOnly, 10);
                UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault();
                this.MinimumParticipantsTextBox.Text = "2";
                this.TimeLimitTextBox.Text = "30";

                this.startedCommand = this.CreateBasicChatCommand("@$username has started a bidding war starting at $gamebet " + currency.Name + " for...SOMETHING! Type !bid <AMOUNT> in chat to outbid them!");

                this.userJoinCommand = this.CreateBasicChatCommand("@$username has become the top bidder with $gamebet " + currency.Name + "! Type !bid <AMOUNT> in chat to outbid them!");

                this.gameCompleteCommand = this.CreateBasicChatCommand("$gamewinners won the bidding war with a bid of $gamebet " + currency.Name + "! Listen closely for how to claim your prize...");
            }

            this.GameStartCommandButtonsControl.DataContext = this.startedCommand;
            this.UserJoinedCommandButtonsControl.DataContext = this.userJoinCommand;
            this.GameCompleteCommandButtonsControl.DataContext = this.gameCompleteCommand;

            return base.OnLoaded();
        }
    }
}
