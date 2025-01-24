using MixItUp.Base.Util;

namespace MixItUp.Base.Model.Twitch.Clients.PubSub
{
    /// <summary>
    /// Listenable topics for PubSub.
    /// 
    /// https://dev.twitch.tv/docs/pubsub/#topics
    /// </summary>
    public enum PubSubTopicsEnum
    {
        /// <summary>
        /// Anyone cheers in a specified channel.
        /// </summary>
        [Name("channel-bits-events-v1")]
        ChannelBitsEventsV1,
        /// <summary>
        /// Anyone cheers in a specified channel.
        /// </summary>
        [Name("channel-bits-events-v2")]
        ChannelBitsEventsV2,
        /// <summary>
        /// Message sent when a user earns a new Bits badge in a particular channel, and chooses to share the notification with chat.
        /// </summary>
        [Name("channel-bits-badge-unlocks")]
        ChannelBitsBadgeUnlocks,

        /// <summary>
        /// Anyone subscribes (first month), resubscribes (subsequent months), or gifts a subscription to a channel. 
        /// 
        /// Subgift subscription messages contain recipient information.
        /// </summary>
        [Name("channel-subscribe-events-v1")]
        ChannelSubscriptionsV1,

        /// <summary>
        /// Anyone whispers the specified user.
        /// </summary>
        [Name("whispers")]
        UserWhispers,

        /// <summary>
        /// A custom reward is redeemed in a channel. 
        /// </summary>
        [Name("channel-points-channel-v1")]
        ChannelPointsRedeemed,
    }

    /// <summary>
    /// A listen request for PubSub.
    /// </summary>
    public class PubSubListenTopicModel
    {
        /// <summary>
        /// The type of topic to listen for.
        /// </summary>
        public PubSubTopicsEnum Type { get; set; }
        /// <summary>
        /// The optional specific identifier to listen for related to the topic.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Creates a new instance of the PubSubListenModel class.
        /// </summary>
        /// <param name="type">The type of topic</param>
        public PubSubListenTopicModel(PubSubTopicsEnum type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Creates a new instance of the PubSubListenModel class.
        /// </summary>
        /// <param name="type">The type of topic</param>
        /// <param name="identifier">The identifier for the topic</param>
        public PubSubListenTopicModel(PubSubTopicsEnum type, string identifier)
        {
            this.Type = type;
            this.Identifier = identifier;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        public override string ToString()
        {
            return (!string.IsNullOrEmpty(this.Identifier)) ? string.Format("{0}.{1}", EnumHelper.GetEnumName(this.Type), this.Identifier) : EnumHelper.GetEnumName(this.Type);
        }
    }
}
