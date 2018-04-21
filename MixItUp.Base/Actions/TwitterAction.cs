using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class TwitterAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return TwitterAction.asyncSemaphore; } }

        [DataMember]
        public string TweetText { get; set; }

        public TwitterAction() : base(ActionTypeEnum.Twitter) { }

        public TwitterAction(string tweetText)
            : this()
        {
            this.TweetText = tweetText;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.Twitter != null)
            {
                string tweet = await this.ReplaceStringWithSpecialModifiers(this.TweetText, user, arguments);
                await ChannelSession.Services.Twitter.SendTweet(tweet);
            }
        }
    }
}
