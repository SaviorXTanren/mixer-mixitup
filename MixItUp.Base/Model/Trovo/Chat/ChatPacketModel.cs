using Newtonsoft.Json.Linq;
using System;

namespace MixItUp.Base.Model.Trovo.Chat
{
    /// <summary>
    /// Information about a chat packet.
    /// </summary>
    public class ChatPacketModel
    {
        /// <summary>
        /// The type of packet.
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// The unique ID of the packet.
        /// </summary>
        public string nonce { get; set; }
        /// <summary>
        /// Errors from the connection,
        /// </summary>
        public string error { get; set; }
        /// <summary>
        /// The data of the packet.
        /// </summary>
        public JObject data { get; set; }

        /// <summary>
        /// Creates a new instance of the ChatPacketModel
        /// </summary>
        public ChatPacketModel() { }

        /// <summary>
        /// Creates a new instance of the ChatPacketModel
        /// </summary>
        /// <param name="type">The type of packet</param>
        public ChatPacketModel(string type)
        {
            this.type = type;
            this.nonce = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Creates a new instance of the ChatPacketModel
        /// </summary>
        /// <param name="type">The type of packet</param>
        /// <param name="data">The data of the packet</param>
        public ChatPacketModel(string type, JObject data)
            : this(type)
        {
            this.data = data;
        }
    }
}
