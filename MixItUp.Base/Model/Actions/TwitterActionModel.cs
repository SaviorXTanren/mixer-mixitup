using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
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

#pragma warning disable CS0612 // Type or member is obsolete
        internal TwitterActionModel(MixItUp.Base.Actions.TwitterAction action)
            : base(ActionTypeEnum.Twitter)
        {
            this.ActionType = (TwitterActionTypeEnum)(int)action.ActionType;
            this.TweetText = action.TweetText;
            this.ImagePath = action.ImagePath;
            this.NameUpdate = action.NewProfileName;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private TwitterActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (ChannelSession.Services.Twitter.IsConnected)
            {
                if (this.ActionType == TwitterActionTypeEnum.SendTweet)
                {
                    string tweet = await ReplaceStringWithSpecialModifiers(this.TweetText, parameters);
                    string imagePath = await ReplaceStringWithSpecialModifiers(this.ImagePath, parameters);

                    if (!string.IsNullOrEmpty(tweet))
                    {
                        if (TwitterActionModel.CheckIfTweetContainsTooManyTags(tweet))
                        {
                            await ChannelSession.Services.Chat.SendMessage("The tweet you specified can not be sent because it contains an @mention");
                            return;
                        }

                        Result result = await ChannelSession.Services.Twitter.SendTweet(tweet, imagePath);
                        if (!result.Success)
                        {
                            await ChannelSession.Services.Chat.SendMessage("Twitter Error: " + result.Message);
                        }
                    }
                }
                else if (this.ActionType == TwitterActionTypeEnum.UpdateName)
                {
                    Result result = await ChannelSession.Services.Twitter.UpdateName(this.NameUpdate);
                    if (!result.Success)
                    {
                        await ChannelSession.Services.Chat.SendMessage("Twitter Error: " + result.Message);
                    }
                }
            }
        }
    }
}
