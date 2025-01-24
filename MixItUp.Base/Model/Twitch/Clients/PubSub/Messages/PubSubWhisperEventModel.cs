using Newtonsoft.Json.Linq;

namespace MixItUp.Base.Model.Twitch.Clients.PubSub.Messages
{
    /// <summary>
    /// Information about a user whisper.
    /// </summary>
    public class PubSubWhisperEventModel
    {
        /// <summary>
        /// The ID of the event
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// The message ID
        /// </summary>
        public string message_id { get; set; }
        /// <summary>
        /// The thread ID
        /// </summary>
        public string thread_id { get; set; }
        /// <summary>
        /// The contents of the message.
        /// </summary>
        public string body { get; set; }
        /// <summary>
        /// The timestamp of when the message was sent.
        /// </summary>
        public long sent_ts { get; set; }
        /// <summary>
        /// The ID of the user who sent the message.
        /// </summary>
        public long from_id { get; set; }
        /// <summary>
        /// The tags of the message.
        /// </summary>
        public PubSubWhisperEventTagsModel tags { get; set; }
        /// <summary>
        /// Information about the recipient.
        /// </summary>
        public PubSubWhisperEventRecipientModel recipient { get; set; }
        /// <summary>
        /// The nonce tracker ID.
        /// </summary>
        public string nonce { get; set; }
    }

    /// <summary>
    /// The tag information of a whisper.
    /// </summary>
    public class PubSubWhisperEventTagsModel
    {
        /// <summary>
        /// The login of the user.
        /// </summary>
        public string login { get; set; }
        /// <summary>
        /// The display name of the user.
        /// </summary>
        public string display_name { get; set; }
        /// <summary>
        /// The color of the user.
        /// </summary>
        public string color { get; set; }
        /// <summary>
        /// The badges of the user.
        /// </summary>
        public JArray badges { get; set; }
        /// <summary>
        /// The emotes of the message.
        /// </summary>
        public JArray emotes { get; set; }
    }

    /// <summary>
    /// The recipient information of a whisper.
    /// </summary>
    public class PubSubWhisperEventRecipientModel
    {
        /// <summary>
        /// The ID of the user.
        /// </summary>
        public uint id { get; set; }
        /// <summary>
        /// The name of the user.
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// The display name of the user.
        /// </summary>
        public string display_name { get; set; }
        /// <summary>
        /// The color of the user.
        /// </summary>
        public string color { get; set; }
        /// <summary>
        /// The profile image of the user.
        /// </summary>
        public string profile_image { get; set; }
    }
}
