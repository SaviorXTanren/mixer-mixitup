namespace MixItUp.Base.Model.Twitch.Clients.EventSub
{
    /// <summary>
    /// Contains information about the session.
    /// <see cref="Session"/>
    /// </summary>
    public class ReconnectMessagePayload
    {
        /// <summary>
        /// The session information.
        /// </summary>
        public Session Session { get; set; }
    }
}
