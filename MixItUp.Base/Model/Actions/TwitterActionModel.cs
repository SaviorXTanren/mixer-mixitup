using MixItUp.Base.Model.Commands;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum TwitterActionTypeEnum
    {
        SendTweet,
        UpdateName,
    }

    [Obsolete]
    [DataContract]
    public class TwitterActionModel : ActionModelBase
    {
        public static bool CheckIfTweetContainsTooManyTags(string tweet) { return !string.IsNullOrEmpty(tweet) && tweet.Count(c => c == '@') > 0; }

        public static TwitterActionModel CreateTweetAction(string tweetText, string imagePath = null)
        {
            TwitterActionModel action = new TwitterActionModel(TwitterActionTypeEnum.SendTweet);
            action.TweetText = tweetText;
            action.ImagePath = imagePath;
            return action;
        }

        public static TwitterActionModel CreateUpdateProfileNameAction(string nameUpdate)
        {
            TwitterActionModel action = new TwitterActionModel(TwitterActionTypeEnum.UpdateName);
            action.NameUpdate = nameUpdate;
            return action;
        }

        [DataMember]
        public TwitterActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string TweetText { get; set; }
        [DataMember]
        public string ImagePath { get; set; }

        [DataMember]
        public string NameUpdate { get; set; }

        public TwitterActionModel(TwitterActionTypeEnum actionType)
            : base(ActionTypeEnum.Twitter)
        {
            this.ActionType = actionType;
        }

        [Obsolete]
        public TwitterActionModel() { }

        protected override Task PerformInternal(CommandParametersModel parameters)
        {
            return Task.CompletedTask;
        }
    }
}
