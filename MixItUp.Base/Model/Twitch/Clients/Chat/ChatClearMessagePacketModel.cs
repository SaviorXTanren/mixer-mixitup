namespace MixItUp.Base.Model.Twitch.Clients.Chat
{
    /// <summary>
    /// Information about a chat message clear packet.
    /// </summary>
    public class ChatClearMessagePacketModel : ChatPacketModelBase
    {
        /// <summary>
        /// The ID of the command for a chat message clear.
        /// </summary>
        public const string CommandID = "CLEARMSG";

        /// <summary>
        /// The ID of the message.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The user's login name.
        /// </summary>
        public string UserLogin { get; set; }

        /// <summary>
        /// The message text.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Creates a new instance of the ChatClearMessagePacketModel class.
        /// </summary>
        /// <param name="packet">The Chat packet</param>
        public ChatClearMessagePacketModel(ChatRawPacketModel packet)
            : base(packet)
        {
            this.ID = packet.GetTagString("target-msg-id");
            this.UserLogin = packet.GetTagString("login");
            this.Message = packet.Get1SkippedParameterText;
        }
    }
}
