using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class LockBoxGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public string CombinationLengthString
        {
            get { return this.CombinationLength.ToString(); }
            set
            {
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int intValue) && intValue > 0)
                {
                    this.CombinationLength = intValue;
                }
                else
                {
                    this.CombinationLength = 0;
                }
                this.NotifyPropertyChanged();
            }
        }
        public int CombinationLength { get; set; } = 3;

        public string InitialAmountString
        {
            get { return this.InitialAmount.ToString(); }
            set
            {
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int intValue) && intValue > 0)
                {
                    this.InitialAmount = intValue;
                }
                else
                {
                    this.InitialAmount = 0;
                }
                this.NotifyPropertyChanged();
            }
        }
        public int InitialAmount { get; set; } = 500;

        public string StatusArgument { get; set; } = "status";

        public string InspectionArgument { get; set; } = "inspect";

        public string InspectionCostString
        {
            get { return this.InspectionCost.ToString(); }
            set
            {
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int intValue) && intValue > 0)
                {
                    this.InspectionCost = intValue;
                }
                else
                {
                    this.InspectionCost = 0;
                }
                this.NotifyPropertyChanged();
            }
        }
        public int InspectionCost { get; set; } = 10;

        public CustomCommand FailedGuessCommand { get; set; }
        public CustomCommand SuccessfulGuessCommand { get; set; }
        public CustomCommand StatusCommand { get; set; }
        public CustomCommand InspectionCommand { get; set; }

        private LockBoxGameCommand existingCommand;

        public LockBoxGameEditorControlViewModel(UserCurrencyViewModel currency)
        {
            this.FailedGuessCommand = this.CreateBasicChatCommand("@$username drops their coins into and try their combo $arg1text...but the box doesn't unlock. Their guess was too $gamelockboxhint.");
            this.SuccessfulGuessCommand = this.CreateBasicChatCommand("@$username drops their coins into and try their combo $arg1text...and unlocks the box! They quickly run off with the $gamepayout " + currency.Name + " inside it!");
            this.StatusCommand = this.CreateBasicChatCommand("After shaking the box for a bit, you guess there's about $gametotalamount " + currency.Name + " inside it.");
            this.InspectionCommand = this.CreateBasicChatCommand("After inspecting the box for a bit, you surmise that one of the numbers is a $gamelockboxinspection.");
        }

        public LockBoxGameEditorControlViewModel(LockBoxGameCommand command)
        {
            this.existingCommand = command;

            this.CombinationLength = this.existingCommand.CombinationLength;
            this.InitialAmount = this.existingCommand.InitialAmount;
            this.StatusArgument = this.existingCommand.StatusArgument;
            this.InspectionArgument = this.existingCommand.InspectionArgument;
            this.InspectionCost = this.existingCommand.InspectionCost;

            this.FailedGuessCommand = this.existingCommand.FailedGuessCommand;
            this.SuccessfulGuessCommand = this.existingCommand.SuccessfulGuessCommand;
            this.StatusCommand = this.existingCommand.StatusCommand;
            this.InspectionCommand = this.existingCommand.InspectionCommand;
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            GameCommandBase newCommand = new LockBoxGameCommand(name, triggers, requirements, this.StatusArgument, this.StatusCommand, this.CombinationLength,
                this.InitialAmount, this.SuccessfulGuessCommand, this.FailedGuessCommand, this.InspectionArgument.ToLower(), this.InspectionCost, this.InspectionCommand);
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        public override async Task<bool> Validate()
        {
            if (this.CombinationLength <= 0)
            {
                await DialogHelper.ShowMessage("The Combo Length is not a valid number greater than 0");
                return false;
            }

            if (this.InitialAmount <= 0)
            {
                await DialogHelper.ShowMessage("The Initial Amount is not a valid number greater than 0");
                return false;
            }

            if (string.IsNullOrEmpty(this.StatusArgument) && !this.StatusArgument.Any(c => char.IsLetterOrDigit(c)))
            {
                await DialogHelper.ShowMessage("The Status Argument must have a valid value");
                return false;
            }

            if (string.IsNullOrEmpty(this.InspectionArgument) && !this.InspectionArgument.Any(c => char.IsLetterOrDigit(c)))
            {
                await DialogHelper.ShowMessage("The Inspection Argument must have a valid value");
                return false;
            }

            if (this.InspectionCost < 0)
            {
                await DialogHelper.ShowMessage("The Inspection Cost is not a valid number greater than 0 or equal to 0");
                return false;
            }

            return true;
        }
    }
}
