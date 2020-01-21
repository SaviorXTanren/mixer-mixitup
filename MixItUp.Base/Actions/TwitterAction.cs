using Mixer.Base.Util;
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

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.Twitter.IsConnected)
            {
                if (this.ActionType == TwitterActionTypeEnum.SendTweet)
                {
                    string tweet = await this.ReplaceStringWithSpecialModifiers(this.TweetText, user, arguments);
                    string imagePath = await this.ReplaceStringWithSpecialModifiers(this.ImagePath, user, arguments);

                    if (TwitterAction.CheckIfTweetContainsTooManyTags(tweet))
                    {
                        await ChannelSession.Services.Chat.Whisper(ChannelSession.MixerUser.username, "The tweet you specified can not be sent because it contains an @mention");
                        return;
                    }

                    if (!await ChannelSession.Services.Twitter.SendTweet(tweet, imagePath))
                    {
                        await ChannelSession.Services.Chat.Whisper(ChannelSession.MixerUser.username, "The tweet you specified could not be sent. Please ensure your Twitter account is correctly authenticated and you have not sent a tweet in the last 5 minutes");
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
