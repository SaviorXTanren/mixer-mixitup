namespace MixItUp.API.V2.Models
{
    public class TwitterAction : ActionBase
    {
        public string ActionType { get; set; }
        public string TweetText { get; set; }
        public string ImagePath { get; set; }
        public string NameUpdate { get; set; }
    }
}
