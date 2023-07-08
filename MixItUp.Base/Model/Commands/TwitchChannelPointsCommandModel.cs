using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class TwitchChannelPointsCommandModel : CommandModelBase
    {
        public static Dictionary<string, string> GetChannelPointTestSpecialIdentifiers()
        {
            return new Dictionary<string, string>()
            {
                { "rewardname", "Test Reward" },
                { "rewardcost", "100" },
                { "message", "Test Message" }
            };
        }

        [DataMember]
        public Guid ChannelPointRewardID { get; set; } = Guid.Empty;

        public TwitchChannelPointsCommandModel(string name, Guid channelPointRewardID)
            : base(name, CommandTypeEnum.TwitchChannelPoints)
        {
            this.ChannelPointRewardID = channelPointRewardID;
        }

        [Obsolete]
        public TwitchChannelPointsCommandModel() : base() { }

        public override Dictionary<string, string> GetTestSpecialIdentifiers() { return TwitchChannelPointsCommandModel.GetChannelPointTestSpecialIdentifiers(); }
    }
}
