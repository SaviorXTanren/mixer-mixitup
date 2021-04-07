using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class TwitchChannelPointsCommandModel : CommandModelBase
    {
        public Guid ChannelPointRewardID { get; set; } = Guid.Empty;

        public TwitchChannelPointsCommandModel(string name, Guid channelPointRewardID)
            : base(name, CommandTypeEnum.TwitchChannelPoints)
        {
            this.ChannelPointRewardID = channelPointRewardID;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal TwitchChannelPointsCommandModel(MixItUp.Base.Commands.TwitchChannelPointsCommand command)
            : base(command)
        {
            this.Name = command.Name;
            this.Type = CommandTypeEnum.TwitchChannelPoints;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        protected TwitchChannelPointsCommandModel() : base() { }
    }
}
