namespace MixItUp.Base.Model.Twitch.Clients.Chat
{
    /// <summary>
    /// Information about a chat notice packet.
    /// </summary>
    public class ChatNoticePacketModel : ChatPacketModelBase
    {
        /// <summary>
        /// The ID of the command for a chat notice.
        /// </summary>
        public const string CommandID = "NOTICE";

        /// <summary>
        /// The message ID associated with notice.
        /// </summary>
        public string MessageID { get; set; }

        /// <summary>
        /// The message of the notice
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Creates a new instance of the ChatNoticePacketModel class.
        /// </summary>
        /// <param name="packet">The Chat packet</param>
        public ChatNoticePacketModel(ChatRawPacketModel packet)
            : base(packet)
        {
            this.MessageID = packet.GetTagString("msg-id");
            this.Message = packet.Get1SkippedParameterText;
        }
    }
}
