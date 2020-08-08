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
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return TwitterActionModel.asyncSemaphore; } }

        public static TwitterActionModel CreateTweetAction(string tweetText, string imagePath = null)
        {
            TwitterActionModel action = new TwitterActionModel(TwitterActionTypeEnum.SendTweet);
            action.TweetText = tweetText;
            action.ImagePath = imagePath;
            return action;
        }

        public static TwitterActionModel CreateUpdateProfileNameAction(string newProfileName)
        {
            TwitterActionModel action = new TwitterActionModel(TwitterActionTypeEnum.UpdateName);
            action.NewProfileName = newProfileName;
            return action;
        }

        [DataMember]
        public TwitterActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string TweetText { get; set; }
        [DataMember]
        public string ImagePath { get; set; }

        [DataMember]
        public string NewProfileName { get; set; }

        public TwitterActionModel(TwitterActionTypeEnum actionType)
            : base(ActionTypeEnum.Twitter)
        {
            this.ActionType = actionType;
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
                        if (tweet.Any(c => c == '@'))
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
                    await ChannelSession.Services.Twitter.UpdateName(this.NewProfileName);
                }
            }
        }
    }
}
