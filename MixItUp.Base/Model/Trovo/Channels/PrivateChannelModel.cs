namespace MixItUp.Base.Model.Trovo.Channels
{
    /// <summary>
    /// Private information for a channel.
    /// </summary>
    public class PrivateChannelModel : ChannelModel
    {
        /// <summary>
        /// The ID of the user.
        /// </summary>
        public string uid { get; set; }
        /// <summary>
        /// The ID of the channel.
        /// </summary>
        public string channel_id { get; set; }
        /// <summary>
        /// The stream key for the user.
        /// </summary>
        public string stream_key { get; set; }
    }
}
