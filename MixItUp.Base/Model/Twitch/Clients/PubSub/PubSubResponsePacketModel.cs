namespace MixItUp.Base.Model.Twitch.Clients.PubSub
{
    /// <summary>
    /// A PubSub web socket response packet.
    /// </summary>
    public class PubSubResponsePacketModel : PubSubPacketModel
    {
        /// <summary>
        /// Error information related to the request.
        /// </summary>
        public string error { get; set; }
    }
}
