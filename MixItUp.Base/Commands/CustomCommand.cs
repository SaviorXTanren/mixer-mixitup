using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Commands
{
    public class CustomCommand : CommandBase
    {
        private static SemaphoreSlim eventCommandPerformSemaphore = new SemaphoreSlim(1);

        [DataMember]
        public uint CommandID { get; set; }

        public CustomCommand(string name) : base(name, CommandTypeEnum.Custom, name)
        {
        }

        protected override SemaphoreSlim AsyncSempahore { get { return CustomCommand.eventCommandPerformSemaphore; } }
    }
}