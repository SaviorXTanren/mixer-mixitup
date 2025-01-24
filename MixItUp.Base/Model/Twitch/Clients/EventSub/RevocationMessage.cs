namespace MixItUp.Base.Model.Twitch.Clients.EventSub
{
    /// <summary>
    /// The server has been told to revoke a subscription.
    /// <see cref="RevocationMessagePayload"/>
    /// </summary>
    public class RevocationMessage : EventSubMessageBase<RevocationMessagePayload>
    {
    }
}
