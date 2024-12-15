namespace MixItUp.Base.Model.Twitch.EventSub
{
    public class CheerNotification
    {
        public string user_id { get; set; }
        public string user_login { get; set; }
        public string user_name { get; set; }
        public string broadcaster_user_id { get; set; }
        public string broadcaster_user_login { get; set; }
        public string broadcaster_user_name { get; set; }
        public int bits { get; set; }
        public string message { get; set; }
        public bool is_anonymous { get; set; }
    }
}
