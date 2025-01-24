namespace MixItUp.Base.Model.Twitch.Clients.Chat
{
    /// <summary>
    /// Information about a chat user leave packet.
    /// </summary>
    public class ChatUserLeavePacketModel : ChatPacketModelBase
    {
        /// <summary>
        /// The ID of the command for a chat user leave.
        /// </summary>
        public const string CommandID = "PART";

        /// <summary>
        /// The user's login name.
        /// </summary>
        public string UserLogin { get; set; }

        /// <summary>
        /// Creates a new instance of the ChatUserLeavePacketModel class.
        /// </summary>
        /// <param name="packet">The Chat packet</param>
        public ChatUserLeavePacketModel(ChatRawPacketModel packet)
            : base(packet)
        {
            this.UserLogin = packet.GetUserLogin;
        }
    }
}
