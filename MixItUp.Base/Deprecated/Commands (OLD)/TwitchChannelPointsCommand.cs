using System;
using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Commands
{
    [Obsolete]
    [DataContract]
    public class TwitchChannelPointsCommand : CommandBase
    {
        private static SemaphoreSlim twitchChannelPointsCommandPerformSemaphore = new SemaphoreSlim(1);

        public TwitchChannelPointsCommand() { }

        public TwitchChannelPointsCommand(string name)
            : base(name, CommandTypeEnum.TwitchChannelPoints, name)
        { }

        protected override SemaphoreSlim AsyncSemaphore { get { return TwitchChannelPointsCommand.twitchChannelPointsCommandPerformSemaphore; } }
    }
}
