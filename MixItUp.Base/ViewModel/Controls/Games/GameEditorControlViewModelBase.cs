using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public abstract class GameEditorControlViewModelBase : ControlViewModelBase
    {
        public abstract Task<bool> Validate();

        public abstract void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements);

        protected CustomCommand CreateBasicChatCommand(string message, bool whisper = false)
        {
            CustomCommand command = new CustomCommand("Game Sub-Command");
            command.Actions.Add(new ChatAction(message, sendAsStreamer: false, isWhisper: whisper));
            return command;
        }

        protected CustomCommand CreateBasic2ChatCommand(string message1, string message2)
        {
            CustomCommand command = new CustomCommand("Game Sub-Command");
            command.Actions.Add(new ChatAction(message1));
            command.Actions.Add(new ChatAction(message2));
            return command;
        }

        protected CustomCommand CreateBasicCurrencyCommand(string message, UserCurrencyModel currency, int amount, bool whisper = false)
        {
            CustomCommand command = this.CreateBasicChatCommand(message, whisper);
            command.Actions.Add(new CurrencyAction(currency, CurrencyActionTypeEnum.AddToUser, amount.ToString()));
            return command;
        }

        protected CustomCommand CreateBasicCurrencyCommand(string message, UserCurrencyModel currency, string amount, bool whisper = false)
        {
            CustomCommand command = this.CreateBasicChatCommand(message, whisper);
            command.Actions.Add(new CurrencyAction(currency, CurrencyActionTypeEnum.AddToUser, amount));
            return command;
        }
    }
}
