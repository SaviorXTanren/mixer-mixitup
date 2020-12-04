using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.Requirement;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Games
{
    public abstract class GameCommandEditorWindowViewModelBase : ControlViewModelBase
    {
        public abstract Task<bool> Validate();

        public abstract void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements);

        protected void SaveGameCommand(GameCommandBase newCommand, GameCommandBase existingCommand)
        {
            // TODO
            //if (existingCommand != null)
            //{
            //    ChannelSession.Settings.GameCommands.Remove(existingCommand);
            //    newCommand.ID = existingCommand.ID;
            //}
            //ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        protected CustomCommand CreateBasicChatCommand() { return new CustomCommand("Game Sub-Command"); }

        protected CustomCommand CreateBasicChatCommand(string message, bool whisper = false)
        {
            CustomCommand command = this.CreateBasicChatCommand();
            command.Actions.Add(new ChatAction(message, sendAsStreamer: false, isWhisper: whisper));
            return command;
        }

        protected CustomCommand CreateBasic2ChatCommand(string message1, string message2)
        {
            CustomCommand command = this.CreateBasicChatCommand();
            command.Actions.Add(new ChatAction(message1));
            command.Actions.Add(new ChatAction(message2));
            return command;
        }

        protected CustomCommand CreateBasicCurrencyCommand(CurrencyModel currency, string amount, bool whisper = false)
        {
            CustomCommand command = this.CreateBasicChatCommand();
            command.Actions.Add(new CurrencyAction(currency, CurrencyActionTypeEnum.AddToUser, amount));
            return command;
        }
    }
}
