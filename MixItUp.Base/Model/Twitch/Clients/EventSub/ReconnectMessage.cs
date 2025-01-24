namespace MixItUp.Base.Model.Twitch.Clients.EventSub
{
    /// <summary>
    /// The reconnect message indicates the server is going to disconnect you and requests 
    /// that you reconnect on the supplied URL.
    /// <see cref="ReconnectMessagePayload"/>
    /// </summary>
    public class ReconnectMessage : EventSubMessageBase<ReconnectMessagePayload>
    {
    }
}
