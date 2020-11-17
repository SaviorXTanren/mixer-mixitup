using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum TwitterActionTypeEnum
    {
        [Name("Send Tweet")]
        SendTweet,
        [Name("Update Name")]
        UpdateName,
    }

    [DataContract]
    public class TwitterAction : ActionBase
    {
        public static bool CheckIfTweetContainsTooManyTags(string tweet) { return !string.IsNullOrEmpty(tweet) && tweet.Count(c => c == '@') > 0; }

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return TwitterAction.asyncSemaphore; } }

        [DataMember]
        public TwitterActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string TweetText { get; set; }
        [DataMember]
        public string ImagePath { get; set; }

        [DataMember]
        public string NewProfileName { get; set; }

        public TwitterAction() : base(ActionTypeEnum.Twitter) { this.ActionType = TwitterActionTypeEnum.SendTweet; }

        public TwitterAction(string tweetText, string imagePath)
            : this()
        {
            this.ActionType = TwitterActionTypeEnum.SendTweet;
            this.TweetText = tweetText;
            this.ImagePath = imagePath;
        }

        public TwitterAction(string profileName)
            : this()
        {
            this.ActionType = TwitterActionTypeEnum.UpdateName;
            this.NewProfileName = profileName;
        }

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            return Task.FromResult(0);
        }
    }
}
