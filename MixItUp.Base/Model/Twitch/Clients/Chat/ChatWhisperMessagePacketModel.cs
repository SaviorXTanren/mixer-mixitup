namespace MixItUp.Base.Model.Twitch.Clients.Chat
{
    /// <summary>
    /// Information about a Chat whisper message packet.
    /// </summary>
    public class ChatWhisperMessagePacketModel : ChatMessagePacketModel
    {
        /// <summary>
        /// The ID of the command for a chat message.
        /// </summary>
        new public const string CommandID = "WHISPER";

        /// <summary>
        /// Creates a new instance of the ChatWhisperMessagePacketModel
        /// </summary>
        /// <param name="packet"></param>
        public ChatWhisperMessagePacketModel(ChatRawPacketModel packet) : base(packet)
        {
        }
    }
}
