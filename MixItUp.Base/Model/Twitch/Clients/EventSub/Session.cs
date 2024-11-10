using Newtonsoft.Json;
using System;

namespace MixItUp.Base.Model.Twitch.Clients.EventSub
{
    /// <summary>
    /// An object that contains information about the connection.
    /// </summary>
    public class Session
    {
        /// <summary>
        /// An ID that uniquely identifies this WebSocket connection.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The connection’s status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The maximum number of seconds that you should expect silence before receiving a keepalive message.
        /// </summary>
        [JsonProperty("keepalive_timeout_seconds")]
        public int? KeepaliveTimeoutSeconds { get; set; }

        /// <summary>
        /// The URL to reconnect to. The connection automatically includes the subscriptions from the old connection.
        /// </summary>
        [JsonProperty("reconnect_url")]
        public string ReconnectUrl { get; set; }

        /// <summary>
        /// The UTC date and time when the connection was created.
        /// </summary>
        [JsonProperty("connected_at")]
        public DateTimeOffset ConnectedAt { get; set; }
    }
}
