using Mixer.Base.Clients;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.User
{
    [DataContract]
    public class UserItemAcquisitonViewModel
    {
        [JsonIgnore]
        public static ConstellationEventType ChannelFollowEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__followed, ChannelSession.Channel.id); } }
        [JsonIgnore]
        public static ConstellationEventType ChannelHostedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__hosted, ChannelSession.Channel.id); } }
        [JsonIgnore]
        public static ConstellationEventType ChannelSubscribedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__subscribed, ChannelSession.Channel.id); } }
        [JsonIgnore]
        public static ConstellationEventType ChannelResubscribedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__resubscribed, ChannelSession.Channel.id); } }
        [JsonIgnore]
        public static ConstellationEventType ChannelResubscribedSharedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__resubShared, ChannelSession.Channel.id); } }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int AcquireAmount { get; set; }
        [DataMember]
        public int AcquireInterval { get; set; }

        [DataMember]
        public int FollowBonus { get; set; }
        [DataMember]
        public int HostBonus { get; set; }
        [DataMember]
        public int SubscribeBonus { get; set; }

        [DataMember]
        public string ResetInterval { get; set; }
        [DataMember]
        public DateTimeOffset LastReset { get; set; }

        [DataMember]
        public bool Enabled { get; set; }

        public UserItemAcquisitonViewModel()
        {
            this.ResetInterval = "Never";
            this.LastReset = DateTimeOffset.MinValue;
        }

        public bool ShouldBeReset()
        {
            if (this.Enabled && !this.ResetInterval.Equals("Never"))
            {
                DateTimeOffset newResetDate = DateTimeOffset.MinValue;
                switch (this.ResetInterval)
                {
                    case "Daily":
                        newResetDate = this.LastReset.AddDays(1);
                        break;
                    case "Weekly":
                        newResetDate = this.LastReset.AddDays(7);
                        break;
                    case "Monthly":
                        newResetDate = this.LastReset.AddMonths(1);
                        break;
                    case "Yearly":
                        newResetDate = this.LastReset.AddYears(1);
                        break;
                }
                return (newResetDate < DateTimeOffset.Now);
            }
            return false;
        }
    }
}
