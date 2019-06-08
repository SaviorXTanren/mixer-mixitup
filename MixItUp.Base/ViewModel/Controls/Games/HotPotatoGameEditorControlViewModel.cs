using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class HotPotatoGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public string TimeLimitString
        {
            get { return this.TimeLimit.ToString(); }
            set
            {
                this.TimeLimit = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int TimeLimit { get; set; } = 30;

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

        public HotPotatoGameEditorControlViewModel(UserCurrencyViewModel currency)
        {
            this.StartedCommand = this.CreateBasicChatCommand("@$username has started a game of hot potato and tossed the potato to @$targetusername. Quick, type !potato to pass it to someone else!");
            this.TossPotatoCommand = this.CreateBasicChatCommand("@$username has tossed the potato to @$targetusername. Quick, type !potato to pass it to someone else!");
            this.PotatoExplodeCommand = this.CreateBasicChatCommand("The potato exploded in @$targetusername hands! @$username gains 100 " + currency.Name + "!");
            this.PotatoExplodeCommand.Actions.Add(new CurrencyAction(currency, CurrencyActionTypeEnum.AddToUser, "100"));
        }

        public HotPotatoGameEditorControlViewModel(HotPotatoGameCommand command)
        {
            this.existingCommand = command;

            this.TimeLimit = this.existingCommand.TimeLimit;
            this.AllowUserTargeting = this.existingCommand.AllowUserTargeting;

            this.StartedCommand = this.existingCommand.StartedCommand;
            this.TossPotatoCommand = this.existingCommand.TossPotatoCommand;
            this.PotatoExplodeCommand = this.existingCommand.PotatoExplodeCommand;
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            GameCommandBase newCommand = new HotPotatoGameCommand(name, triggers, requirements, this.TimeLimit, this.AllowUserTargeting, this.StartedCommand, this.TossPotatoCommand,
                this.PotatoExplodeCommand);
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        public override async Task<bool> Validate()
        {
            if (this.TimeLimit <= 0)
            {
                await DialogHelper.ShowMessage("The Time Limit is not a valid number greater than 0");
                return false;
            }
            return true;
        }
    }
}
