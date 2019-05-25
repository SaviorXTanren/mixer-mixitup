using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class PickpocketGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public string UserPercentageString
        {
            get { return this.UserPercentage.ToString(); }
            set
            {
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int intValue) && intValue > 0)
                {
                    this.UserPercentage = intValue;
                }
                else
                {
                    this.UserPercentage = 0;
                }
                this.NotifyPropertyChanged();
            }
        }
        public int UserPercentage { get; set; } = 40;

        public string SubscriberPercentageString
        {
            get { return this.SubscriberPercentage.ToString(); }
            set
            {
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int intValue) && intValue > 0)
                {
                    this.SubscriberPercentage = intValue;
                }
                else
                {
                    this.SubscriberPercentage = 0;
                }
                this.NotifyPropertyChanged();
            }
        }
        public int SubscriberPercentage { get; set; } = 40;

        public string ModPercentageString
        {
            get { return this.ModPercentage.ToString(); }
            set
            {
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int intValue) && intValue > 0)
                {
                    this.ModPercentage = intValue;
                }
                else
                {
                    this.ModPercentage = 0;
                }
                this.NotifyPropertyChanged();
            }
        }
        public int ModPercentage { get; set; } = 40;

        public CustomCommand SuccessOutcomeCommand { get; set; }
        public CustomCommand FailOutcomeCommand { get; set; }

        private PickpocketGameCommand existingCommand;

        public PickpocketGameEditorControlViewModel(UserCurrencyViewModel currency)
        {
            this.SuccessOutcomeCommand = this.CreateBasicChatCommand("@$username stole $gamepayout " + currency.Name + " from @$targetusername!");
            this.FailOutcomeCommand = this.CreateBasicChatCommand("@$username was unable to steal from @$targetusername...");
        }

        public PickpocketGameEditorControlViewModel(PickpocketGameCommand command)
        {
            this.existingCommand = command;

            this.UserPercentage = this.existingCommand.SuccessfulOutcome.RoleProbabilities[MixerRoleEnum.User];
            this.SubscriberPercentage = this.existingCommand.SuccessfulOutcome.RoleProbabilities[MixerRoleEnum.Subscriber];
            this.ModPercentage = this.existingCommand.SuccessfulOutcome.RoleProbabilities[MixerRoleEnum.Mod];

            this.SuccessOutcomeCommand = this.existingCommand.SuccessfulOutcome.Command;
            this.FailOutcomeCommand = this.existingCommand.FailedOutcome.Command;
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            Dictionary<MixerRoleEnum, int> successRoleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, this.UserPercentage }, { MixerRoleEnum.Subscriber, this.SubscriberPercentage }, { MixerRoleEnum.Mod, this.ModPercentage } };
            Dictionary<MixerRoleEnum, int> failRoleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 100 - this.UserPercentage }, { MixerRoleEnum.Subscriber, 100 - this.SubscriberPercentage }, { MixerRoleEnum.Mod, 100 - this.ModPercentage } };

            GameCommandBase newCommand = new PickpocketGameCommand(name, triggers, requirements, new GameOutcome("Success", 1, successRoleProbabilities, this.SuccessOutcomeCommand),
                new GameOutcome("Failure", 0, failRoleProbabilities, this.FailOutcomeCommand));
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        public override async Task<bool> Validate()
        {
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
