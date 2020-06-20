using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class HotPotatoGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public string LowerTimeLimitString
        {
            get { return this.LowerTimeLimit.ToString(); }
            set
            {
                this.LowerTimeLimit = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int LowerTimeLimit { get; set; } = 30;

        public string UpperTimeLimitString
        {
            get { return this.UpperTimeLimit.ToString(); }
            set
            {
                this.UpperTimeLimit = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int UpperTimeLimit { get; set; } = 30;

        public bool AllowUserTargeting
        {
            get { return this.allowUserTargeting; }
            set
            {
                this.allowUserTargeting = value;
                this.NotifyPropertyChanged();
            }
        }
        public bool allowUserTargeting { get; set; } = false;

        public CustomCommand StartedCommand { get; set; }

        public CustomCommand TossPotatoCommand { get; set; }

        public CustomCommand PotatoExplodeCommand { get; set; }

        public HotPotatoGameCommand existingCommand;

        public HotPotatoGameEditorControlViewModel(CurrencyModel currency)
        {
            this.StartedCommand = this.CreateBasicChatCommand("@$username has started a game of hot potato and tossed the potato to @$targetusername. Quick, type !potato to pass it to someone else!");
            this.TossPotatoCommand = this.CreateBasicChatCommand("@$username has tossed the potato to @$targetusername. Quick, type !potato to pass it to someone else!");
            this.PotatoExplodeCommand = this.CreateBasicChatCommand("The potato exploded in @$targetusername hands! @$username gains 100 " + currency.Name + "!");
            this.PotatoExplodeCommand.Actions.Add(new CurrencyAction(currency, CurrencyActionTypeEnum.AddToUser, "100"));
        }

        public HotPotatoGameEditorControlViewModel(HotPotatoGameCommand command)
        {
            this.existingCommand = command;

#pragma warning disable CS0612 // Type or member is obsolete
            if (this.existingCommand.TimeLimit > 0)
            {
                this.existingCommand.LowerLimit = this.existingCommand.TimeLimit;
                this.existingCommand.UpperLimit = this.existingCommand.TimeLimit;
                this.existingCommand.TimeLimit = 0;
            }
#pragma warning restore CS0612 // Type or member is obsolete

            this.LowerTimeLimit = this.existingCommand.LowerLimit;
            this.UpperTimeLimit = this.existingCommand.UpperLimit;
            this.AllowUserTargeting = this.existingCommand.AllowUserTargeting;

            this.StartedCommand = this.existingCommand.StartedCommand;
            this.TossPotatoCommand = this.existingCommand.TossPotatoCommand;
            this.PotatoExplodeCommand = this.existingCommand.PotatoExplodeCommand;
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            GameCommandBase newCommand = new HotPotatoGameCommand(name, triggers, requirements, this.LowerTimeLimit, this.UpperTimeLimit, this.AllowUserTargeting, this.StartedCommand, this.TossPotatoCommand,
                this.PotatoExplodeCommand);
            this.SaveGameCommand(newCommand, this.existingCommand);
        }

        public override async Task<bool> Validate()
        {
            if (this.LowerTimeLimit <= 0)
            {
                await DialogHelper.ShowMessage("The Lower Time Limit is not a valid number greater than 0");
                return false;
            }

            if (this.UpperTimeLimit <= 0)
            {
                await DialogHelper.ShowMessage("The Upper Time Limit is not a valid number greater than 0");
                return false;
            }
            return true;
        }
    }
}
