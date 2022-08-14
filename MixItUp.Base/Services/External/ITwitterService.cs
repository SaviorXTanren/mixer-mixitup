using MixItUp.Base.Model;
using Newtonsoft.Json;
using StreamingClient.Base.Model.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    [DataContract]
    public class Tweet
    {
        [DataMember]
        public ulong ID { get; set; }

        [DataMember]
        public ulong UserID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public DateTimeOffset DateTime { get; set; }

        [DataMember]
        public List<string> Links { get; set; }

        [JsonIgnore]
        public string TweetLink { get { return string.Format("https://twitter.com/{0}/status/{1}", this.UserName, this.ID); } }

        public Tweet()
        {
            this.Links = new List<string>();
        }

        public bool IsStreamTweet
        {
            get
            {
                bool result = false;
                StreamingPlatforms.ForEachPlatform(p =>
                {
                    if (StreamingPlatforms.GetPlatformSessionService(p).IsConnected && this.Links.Any(l => l.ToLower().Contains(StreamingPlatforms.GetPlatformSessionService(p).ChannelLink)))
                    {
                        result = true;
                    }
                });
                return result;
            }
        }
    }

    public interface ITwitterService : IExternalService
    {
        Task<IEnumerable<Tweet>> GetLatestTweets();
        OAuthTokenModel GetOAuthTokenCopy();
        Task<Util.Result> SendTweet(string tweet, string imagePath);
        void SetAuthPin(string authorizationPin);
        Task<Util.Result> UpdateName(string nameUpdate);
    }
}
