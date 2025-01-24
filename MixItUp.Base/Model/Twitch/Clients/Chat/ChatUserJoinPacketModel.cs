namespace MixItUp.Base.Model.Twitch.Clients.Chat
{
    /// <summary>
    /// Information about a chat user join packet.
    /// </summary>
    public class ChatUserJoinPacketModel : ChatPacketModelBase
    {
        /// <summary>
        /// The ID of the command for a chat user join.
        /// </summary>
        public const string CommandID = "JOIN";

        /// <summary>
        /// The user's login name.
        /// </summary>
        public string UserLogin { get; set; }

        /// <summary>
        /// Creates a new instance of the ChatUserJoinPacketModel class.
        /// </summary>
        /// <param name="packet">The Chat packet</param>
        public ChatUserJoinPacketModel(ChatRawPacketModel packet)
            : base(packet)
        {
            this.UserLogin = packet.GetUserLogin;
        }
    }
}
