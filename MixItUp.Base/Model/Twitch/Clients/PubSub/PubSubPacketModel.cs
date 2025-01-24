using Newtonsoft.Json.Linq;
using System;

namespace MixItUp.Base.Model.Twitch.Clients.PubSub
{
    /// <summary>
    /// A PubSub web socket packet.
    /// </summary>
    public class PubSubPacketModel
    {
        /// <summary>
        /// The type of packet.
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// The data of the packet.
        /// </summary>
        public JObject data { get; set; }
        /// <summary>
        /// Optional string used to identify the response for a request.
        /// </summary>
        public string nonce { get; set; }

        /// <summary>
        /// Creates a new instance of the PubSubPacketModel class.
        /// </summary>
        public PubSubPacketModel()
        {
            this.nonce = Guid.NewGuid().ToString().Replace("-", "");
        }

        /// <summary>
        /// Creates a new instance of the PubSubPacketModel class.
        /// </summary>
        /// <param name="type">The type of packet</param>
        public PubSubPacketModel(string type)
            : this()
        {
            this.type = type;
        }

        /// <summary>
        /// Creates a new instance of the PubSubPacketModel class.
        /// </summary>
        /// <param name="type">The type of packet</param>
        /// <param name="data">The data of the packet</param>
        public PubSubPacketModel(string type, object data)
            : this(type)
        {
            this.data = JObject.FromObject(data);
        }
    }
}
