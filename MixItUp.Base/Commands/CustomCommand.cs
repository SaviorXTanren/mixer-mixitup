using System.Threading;

namespace MixItUp.Base.Commands
{
    public class CustomCommand : CommandBase
    {
        private static SemaphoreSlim customCommandPerformSemaphore = new SemaphoreSlim(1);

        public CustomCommand(string name) : base(name, CommandTypeEnum.Custom, name) { }

        protected override SemaphoreSlim AsyncSempahore { get { return CustomCommand.customCommandPerformSemaphore; } }
    }
}