using Mixer.Base.Model.OAuth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class Tweet
    {
        [DataMember]
        public ulong ID { get; set; }

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
    }

    public interface ITwitterService
    {
        Task<bool> Connect();

        Task Disconnect();

        void SetAuthPin(string pin);

        Task<IEnumerable<Tweet>> GetLatestTweets();

        Task SendTweet(string tweet, string imagePath = null);

        Task UpdateName(string name);

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
