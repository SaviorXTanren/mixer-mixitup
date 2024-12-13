namespace MixItUp.Base.Model.Twitch.EventSub
{
    public class ChatMessageNotification
    {
    }

    public class ChatMessageNotificationEmote
    {
        public string id { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
    }
}
