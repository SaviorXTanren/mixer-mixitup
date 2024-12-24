using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.EventSub
{
    public class ChannelUpdateNotification
    {
        public string broadcaster_user_id { get; set; }
        public string broadcaster_user_login { get; set; }
        public string broadcaster_user_name { get; set; }
        public string title { get; set; }
        public string language { get; set; }
        public string category_id { get; set; }
        public string category_name { get; set; }
        public List<string> content_classification_labels { get; set; } = new List<string>();
    }
}
