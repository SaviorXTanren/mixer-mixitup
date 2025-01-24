namespace MixItUp.Base.Model.Twitch.Clients.EventSub
{
    /// <summary>
    /// A base class with no payload for the EventSub messages.
    /// </summary>
    public abstract class EventSubMessageBase
    {
        /// <summary>
        /// The message metadata including information about the message type.
        /// </summary>
        public MessageMetadata Metadata { get; set; }
    }

    /// <summary>
    /// A base class with a typed payload for the EventSub messages.
    /// </summary>
    public abstract class EventSubMessageBase<T> : EventSubMessageBase
    {
        /// <summary>
        /// The payload which is different depending on the message type.
        /// </summary>
        public T Payload { get; set; }
    }
}
