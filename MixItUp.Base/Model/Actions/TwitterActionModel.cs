using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum TwitterActionTypeEnum
    {
        [Name("Send Tweet")]
        SendTweet,
        [Name("Update Name")]
        UpdateName,
    }

    [DataContract]
    public class TwitterActionModel : ActionModelBase
    {
        public static bool CheckIfTweetContainsTooManyTags(string tweet) { return !string.IsNullOrEmpty(tweet) && tweet.Count(c => c == '@') > 0; }

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return TwitterActionModel.asyncSemaphore; } }

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

        internal TwitterActionModel(MixItUp.Base.Actions.TwitterAction action)
            : base(ActionTypeEnum.Twitter)
        {
            this.ActionType = (TwitterActionTypeEnum)(int)action.ActionType;
            this.TweetText = action.TweetText;
            this.ImagePath = action.ImagePath;
            this.NameUpdate = action.NewProfileName;
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (ChannelSession.Services.Twitter.IsConnected)
            {
                if (this.ActionType == TwitterActionTypeEnum.SendTweet)
                {
                    string tweet = await this.ReplaceStringWithSpecialModifiers(this.TweetText, user, platform, arguments, specialIdentifiers);
                    string imagePath = await this.ReplaceStringWithSpecialModifiers(this.ImagePath, user, platform, arguments, specialIdentifiers);

                    if (!string.IsNullOrEmpty(tweet))
                    {
                        if (TwitterActionModel.CheckIfTweetContainsTooManyTags(tweet))
                        {
                            await ChannelSession.Services.Chat.SendMessage("The tweet you specified can not be sent because it contains an @mention");
                            return;
                        }

                        if (!await ChannelSession.Services.Twitter.SendTweet(tweet, imagePath))
                        {
                            await ChannelSession.Services.Chat.SendMessage("The tweet you specified could not be sent. Please ensure your Twitter account is correctly authenticated and you have not sent a tweet in the last 5 minutes");
                        }
                    }
                }
                else if (this.ActionType == TwitterActionTypeEnum.UpdateName)
                {
                    await ChannelSession.Services.Twitter.UpdateName(this.NameUpdate);
                }
            }
        }
    }
}
