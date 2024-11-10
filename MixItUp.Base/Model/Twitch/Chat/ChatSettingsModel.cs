using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Twitch.Chat
{
    /// <summary>
    /// Information about updating a channel's chat settings.
    /// </summary>
    [DataContract]
    public class ChatSettingsModel
    {
        /// <summary>
        /// A Boolean value that determines whether chat messages must contain only emotes.
        ///
        /// Set to true, if only messages that are 100% emotes are allowed; otherwise, false. Default is false.
        /// </summary>
        [DataMember]
        public bool emote_mode { get; set; }

        /// <summary>
        /// A Boolean value that determines whether the broadcaster restricts the chat room to followers only, based on how long they’ve followed.
        /// 
        /// Set to true, if the broadcaster restricts the chat room to followers only; otherwise, false. Default is true.
        ///
        /// See follower_mode_duration for how long the followers must have followed the broadcaster to participate in the chat room.
        /// </summary>
        [DataMember]
        public bool follower_mode { get; set; }

        /// <summary>
        /// The length of time, in minutes, that the followers must have followed the broadcaster to participate in the chat room (see follower_mode).
        ///
        /// You may specify a value in the range: 0 (no restriction) through 129600 (3 months). The default is 0.
        /// </summary>
        [DataMember]
        public int? follower_mode_duration { get; set; }

        /// <summary>
        /// A Boolean value that determines whether the broadcaster adds a short delay before chat messages appear in the chat room. This gives chat moderators and bots a chance to remove them before viewers can see the message.
        ///
        /// Set to true, if the broadcaster applies a delay; otherwise, false. Default is false.
        ///
        /// See non_moderator_chat_delay_duration for the length of the delay.
        /// </summary>
        [DataMember]
        public bool non_moderator_chat_delay { get; set; }

        /// <summary>
        /// The amount of time, in seconds, that messages are delayed from appearing in chat.
        ///
        /// Possible values are:
        /// 2 — 2 second delay(recommended)
        /// 4 — 4 second delay
        /// 6 — 6 second delay
        /// See non_moderator_chat_delay.
        /// </summary>
        [DataMember]
        public int? non_moderator_chat_delay_duration { get; set; }

        /// <summary>
        /// A Boolean value that determines whether the broadcaster limits how often users in the chat room are allowed to send messages.
        ///
        /// Set to true, if the broadcaster applies a wait period messages; otherwise, false. Default is false.
        ///
        /// See slow_mode_wait_time for the delay.
        /// </summary>
        [DataMember]
        public bool slow_mode { get; set; }

        /// <summary>
        /// The amount of time, in seconds, that users need to wait between sending messages (see slow_mode).
        ///
        /// You may specify a value in the range: 3 (3 second delay) through 120 (2 minute delay). The default is 30 seconds.
        /// </summary>
        [DataMember]
        public int? slow_mode_wait_time { get; set; }

        /// <summary>
        /// A Boolean value that determines whether only users that subscribe to the broadcaster’s channel can talk in the chat room.
        ///
        /// Set to true, if the broadcaster restricts the chat room to subscribers only; otherwise, false. Default is false.
        /// </summary>
        [DataMember]
        public bool subscriber_mode { get; set; }

        /// <summary>
        /// A Boolean value that determines whether the broadcaster requires users to post only unique messages in the chat room.
        ///
        /// Set to true, if the broadcaster requires unique messages only; otherwise, false. Default is false.
        /// </summary>
        [DataMember]
        public bool unique_chat_mode { get; set; }
    }
}
