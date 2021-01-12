using MixItUp.Base.Actions;
using System;
using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Commands
{
    [Obsolete]
    [DataContract]
    public class CustomCommand : CommandBase
    {
        public static CustomCommand BasicChatCommand(string name) { return new CustomCommand(name); }

        public static CustomCommand BasicChatCommand(string name, string message, bool isWhisper = false)
        {
            CustomCommand command = CustomCommand.BasicChatCommand(name);
            command.Actions.Add(new ChatAction(message, isWhisper: isWhisper));
            return command;
        }

        private static SemaphoreSlim customCommandPerformSemaphore = new SemaphoreSlim(1);

        public CustomCommand(string name) : base(name, CommandTypeEnum.Custom, name) { }

        protected override SemaphoreSlim AsyncSemaphore { get { return CustomCommand.customCommandPerformSemaphore; } }
    }
}