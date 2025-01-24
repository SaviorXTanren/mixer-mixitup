namespace MixItUp.Base.Model.Twitch.Clients.EventSub
{
    /// <summary>
    /// The type of message.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Used to indicate a welcome message.
        /// <see cref="WelcomeMessage"/>
        /// </summary>
        session_welcome,

        /// <summary>
        /// Used to indicate a keepalive message.
        /// <see cref="KeepAliveMessage"/>
        /// </summary>
        session_keepalive,

        /// <summary>
        /// Used to indicate a reconnect message.
        /// <see cref="ReconnectMessage"/>
        /// </summary>
        session_reconnect,

        /// <summary>
        /// Used to indicate a notification message.
        /// <see cref="NotificationMessage"/>
        /// </summary>
        notification,

        /// <summary>
        /// Used to indicate a revokation message.
        /// <see cref="RevocationMessage"/>
        /// </summary>
        revocation,
    }
}
