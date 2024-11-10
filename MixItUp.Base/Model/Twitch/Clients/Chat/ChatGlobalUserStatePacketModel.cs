namespace MixItUp.Base.Model.Twitch.Clients.Chat
{
    /// <summary>
    /// Information about a chat global user state packet.
    /// </summary>
    public class ChatGlobalUserStatePacketModel : ChatUserStatePacketModel
    {
        /// <summary>
        /// The ID of the command for a chat global user state.
        /// </summary>
        public new const string CommandID = "GLOBALUSERSTATE";

        /// <summary>
        /// The user’s ID.
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// Creates a new instance of the ChatUserStatePacketModel class.
        /// </summary>
        /// <param name="packet">The Chat packet</param>
        public ChatGlobalUserStatePacketModel(ChatRawPacketModel packet)
            : base(packet)
        {
            this.UserID = packet.GetTagLong("user-id");
        }
    }
}
