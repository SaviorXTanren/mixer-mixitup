using System.Linq;

namespace MixItUp.Base.Model.Twitch.Clients.Chat
{
    /// <summary>
    /// Information about a chat clear packet.
    /// </summary>
    public class ChatClearChatPacketModel : ChatPacketModelBase
    {
        /// <summary>
        /// The ID of the command for a chat clear.
        /// </summary>
        public const string CommandID = "CLEARCHAT";

        /// <summary>
        /// The user's ID who was purged, if any.
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// The user's login name who was purged, if any.
        /// </summary>
        public string UserLogin { get; set; }

        /// <summary>
        /// The time of the ban in seconds. A value of 0 is a permanent ban.
        /// </summary>
        public long BanDuration { get; set; } = 0;

        /// <summary>
        /// Creates a new instance of the ChatClearChatPacketModel class.
        /// </summary>
        /// <param name="packet">The Chat packet</param>
        public ChatClearChatPacketModel(ChatRawPacketModel packet)
            : base(packet)
        {
            if (packet.Parameters.Count > 1)
            {
                this.UserLogin = packet.Parameters.Last();
            }
            this.UserID = packet.GetTagString("target-user-id");
            this.BanDuration = packet.GetTagLong("ban-duration");
        }

        /// <summary>
        /// Indicates if this was a regular chat clear and not directed at a specific user.
        /// </summary>
        public bool IsClear { get { return string.IsNullOrEmpty(this.UserID); } }

        /// <summary>
        /// Indicates if this is a timeout of a specific user.
        /// </summary>
        public bool IsTimeout { get { return !string.IsNullOrEmpty(this.UserID) && this.BanDuration > 0; } }

        /// <summary>
        /// Indicates if this is a ban of a specific user.
        /// </summary>
        public bool IsBan { get { return !string.IsNullOrEmpty(this.UserID) && this.BanDuration == 0; } }
    }
}
