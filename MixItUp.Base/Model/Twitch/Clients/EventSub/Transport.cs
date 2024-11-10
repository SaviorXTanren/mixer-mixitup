using Newtonsoft.Json;

namespace MixItUp.Base.Model.Twitch.Clients.EventSub
{
    /// <summary>
    /// An object that contains information about the transport used for notifications.
    /// </summary>
    public class Transport
    {
        /// <summary>
        /// The transport method.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// An ID that uniquely identifies the WebSocket.
        /// </summary>
        [JsonProperty("session_id")]
        public string SessionId { get; set; }
    }
}
