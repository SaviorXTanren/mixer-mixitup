using MixItUp.Base.Actions;
using System.Threading;

namespace MixItUp.Base.Commands
{
    public class CustomCommand : CommandBase
    {
        public static CustomCommand BasicChatCommand(string name, string message, bool isWhisper = false)
        {
            CustomCommand command = new CustomCommand(name);
            command.Actions.Add(new ChatAction(message, isWhisper: isWhisper));
            return command;
        }

        private static SemaphoreSlim customCommandPerformSemaphore = new SemaphoreSlim(1);

        public CustomCommand(string name) : base(name, CommandTypeEnum.Custom, name) { }

        protected override SemaphoreSlim AsyncSemaphore { get { return CustomCommand.customCommandPerformSemaphore; } }
    }
}