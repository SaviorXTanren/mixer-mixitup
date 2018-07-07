using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for StealGameEditorControl.xaml
    /// </summary>
    public partial class StealGameEditorControl : GameEditorControlBase
    {
        private StealGameCommand existingCommand;

        private CustomCommand successOutcomeCommand;
        private CustomCommand failOutcomeCommand;

        public StealGameEditorControl()
        {
            InitializeComponent();
        }

        public StealGameEditorControl(StealGameCommand command)
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
            int.TryParse(this.UserPercentageTextBox.Text, out int userChance);
            int.TryParse(this.SubscriberPercentageTextBox.Text, out int subscriberChance);
            int.TryParse(this.ModPercentageTextBox.Text, out int modChance);

            Dictionary<MixerRoleEnum, int> successRoleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, userChance }, { MixerRoleEnum.Subscriber, subscriberChance }, { MixerRoleEnum.Mod, modChance } };
            Dictionary<MixerRoleEnum, int> failRoleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 100 - userChance }, { MixerRoleEnum.Subscriber, 100 - subscriberChance }, { MixerRoleEnum.Mod, 100 - modChance } };

            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
            }
            ChannelSession.Settings.GameCommands.Add(new StealGameCommand(this.CommandDetailsControl.GameName, this.CommandDetailsControl.ChatTriggers,
                this.CommandDetailsControl.GetRequirements(), new GameOutcome("Success", 1, successRoleProbabilities, this.successOutcomeCommand),
                new GameOutcome("Failure", 0, failRoleProbabilities, this.failOutcomeCommand)));
        }

        protected override Task OnLoaded()
        {
            if (this.existingCommand != null)
            {
                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);
                this.UserPercentageTextBox.Text = this.existingCommand.SuccessfulOutcome.RoleProbabilities[MixerRoleEnum.User].ToString();
                this.SubscriberPercentageTextBox.Text = this.existingCommand.SuccessfulOutcome.RoleProbabilities[MixerRoleEnum.Subscriber].ToString();
                this.ModPercentageTextBox.Text = this.existingCommand.SuccessfulOutcome.RoleProbabilities[MixerRoleEnum.Mod].ToString();
                this.successOutcomeCommand = this.existingCommand.SuccessfulOutcome.Command;
                this.failOutcomeCommand = this.existingCommand.FailedOutcome.Command;
            }
            else
            {
                this.CommandDetailsControl.SetDefaultValues("Steal", "steal", CurrencyRequirementTypeEnum.MinimumAndMaximum, 10, 1000);
                UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault();
                this.UserPercentageTextBox.Text = "60";
                this.SubscriberPercentageTextBox.Text = "60";
                this.ModPercentageTextBox.Text = "60";
                this.successOutcomeCommand = this.CreateBasicChatCommand("@$username stole $gamepayout " + currency.Name + " from @$targetusername!");
                this.failOutcomeCommand = this.CreateBasicChatCommand("@$username was unable to steal from anyone...");
            }

            this.SuccessOutcomeCommandButtonsControl.DataContext = this.successOutcomeCommand;
            this.FailOutcomeCommandButtonsControl.DataContext = this.failOutcomeCommand;

            return base.OnLoaded();
        }
    }
}
