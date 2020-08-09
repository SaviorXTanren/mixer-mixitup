using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class TwitchChannelPointsCommandModel : CommandModelBase
    {
        private static SemaphoreSlim commandLockSemaphore = new SemaphoreSlim(1);

        public TwitchChannelPointsCommandModel(string name) : base(name, CommandTypeEnum.TwitchChannelPoints) { }

        internal TwitchChannelPointsCommandModel(MixItUp.Base.Commands.TwitchChannelPointsCommand command)
            : base(command)
        {
            this.Name = command.Name;
            this.Type = CommandTypeEnum.TwitchChannelPoints;
        }

        protected override SemaphoreSlim CommandLockSemaphore { get { return TwitchChannelPointsCommandModel.commandLockSemaphore; } }
    }
}
