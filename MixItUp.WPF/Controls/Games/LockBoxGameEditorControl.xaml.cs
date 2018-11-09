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
    /// Interaction logic for LockBoxGameEditorControl.xaml
    /// </summary>
    public partial class LockBoxGameEditorControl : GameEditorControlBase
    {
        private LockBoxGameCommand existingCommand;

        private CustomCommand failedGuessCommand { get; set; }
        private CustomCommand successfulGuessCommand { get; set; }
        private CustomCommand statusCommand { get; set; }
        private CustomCommand inspectionCommand { get; set; }

        public LockBoxGameEditorControl()
        {
            InitializeComponent();
        }

        public LockBoxGameEditorControl(LockBoxGameCommand command)
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

            if (!int.TryParse(this.CombinationLengthTextBox.Text, out int comboLength) || comboLength <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Combo Length is not a valid number greater than 0");
                return false;
            }

            if (!int.TryParse(this.InitialAmountTextBox.Text, out int initialAmount) || initialAmount <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Initial Amount is not a valid number greater than 0");
                return false;
            }

            if (string.IsNullOrEmpty(this.StatusArgumentTextBox.Text) && !this.StatusArgumentTextBox.Text.Any(c => char.IsLetterOrDigit(c)))
            {
                await MessageBoxHelper.ShowMessageDialog("The Status Argument must have a valid value");
                return false;
            }

            if (string.IsNullOrEmpty(this.InspectionArgumentTextBox.Text) && !this.InspectionArgumentTextBox.Text.Any(c => char.IsLetterOrDigit(c)))
            {
                await MessageBoxHelper.ShowMessageDialog("The Inspection Argument must have a valid value");
                return false;
            }

            if (!int.TryParse(this.InspectionCostTextBox.Text, out int inspectionCost) || inspectionCost < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Inspection Cost is not a valid number greater than 0 or equal to 0");
                return false;
            }

            return true;
        }

        public override void SaveGameCommand()
        {
            int.TryParse(this.CombinationLengthTextBox.Text, out int comboLength);
            int.TryParse(this.InitialAmountTextBox.Text, out int initialAmount);
            int.TryParse(this.InspectionCostTextBox.Text, out int inspectionCost);

            GameCommandBase newCommand = new LockBoxGameCommand(this.CommandDetailsControl.GameName, this.CommandDetailsControl.ChatTriggers, this.CommandDetailsControl.GetRequirements(),
                this.StatusArgumentTextBox.Text.ToLower(), this.statusCommand, comboLength, initialAmount, this.successfulGuessCommand, this.failedGuessCommand,
                this.InspectionArgumentTextBox.Text.ToLower(), inspectionCost, this.inspectionCommand);
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        protected override Task OnLoaded()
        {
            if (this.existingCommand != null)
            {
                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);

                this.CombinationLengthTextBox.Text = this.existingCommand.CombinationLength.ToString();
                this.InitialAmountTextBox.Text = this.existingCommand.InitialAmount.ToString();
                this.failedGuessCommand = this.existingCommand.FailedGuessCommand;
                this.successfulGuessCommand = this.existingCommand.SuccessfulGuessCommand;

                this.StatusArgumentTextBox.Text = this.existingCommand.StatusArgument;
                this.statusCommand = this.existingCommand.StatusCommand;

                this.InspectionArgumentTextBox.Text = this.existingCommand.InspectionArgument;
                this.InspectionCostTextBox.Text = this.existingCommand.InspectionCost.ToString();
                this.inspectionCommand = this.existingCommand.InspectionCommand;
            }
            else
            {
                this.CommandDetailsControl.SetDefaultValues("Lock Box", "lockbox", CurrencyRequirementTypeEnum.RequiredAmount, 10);
                UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault();

                this.CombinationLengthTextBox.Text = "3";
                this.InitialAmountTextBox.Text = "500";
                this.failedGuessCommand = this.CreateBasicChatCommand("@$username drops their coins into and try their combo...but the box doesn't unlock. Their guess was too $gamelockboxhint.");
                this.successfulGuessCommand = this.CreateBasicChatCommand("@$username drops their coins into and try their combo...and unlocks the box! They quickly run off with the $gamepayout " + currency.Name + " inside it!");

                this.StatusArgumentTextBox.Text = "status";
                this.statusCommand = this.CreateBasicChatCommand("After shaking the box for a bit, you guess there's about $gametotalamount " + currency.Name + " inside it.");

                this.InspectionArgumentTextBox.Text = "inspect";
                this.InspectionCostTextBox.Text = "10";
                this.inspectionCommand = this.CreateBasicChatCommand("After inspecting the box for a bit, you surmise that one of the numbers is a $gamelockboxinspection.");
            }

            this.FailedGuessCommandButtonsControl.DataContext = this.failedGuessCommand;
            this.SuccessfulGuessCommandButtonsControl.DataContext = this.successfulGuessCommand;
            this.StatusCommandButtonsControl.DataContext = this.statusCommand;
            this.InspectionCommandButtonsControl.DataContext = this.inspectionCommand;

            return base.OnLoaded();
        }
    }
}
