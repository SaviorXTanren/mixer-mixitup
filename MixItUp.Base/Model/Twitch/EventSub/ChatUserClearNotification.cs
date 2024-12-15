namespace MixItUp.Base.Model.Twitch.EventSub
{
    public class ChatUserClearNotification
    {
        public string broadcaster_user_id { get; set; }
        public string broadcaster_user_name { get; set; }
        public string broadcaster_user_login { get; set; }
        public string target_user_id { get; set; }
        public string target_user_name { get; set; }
        public string target_user_login { get; set; }
    }
}
