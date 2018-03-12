using Mixer.Base.Model.OAuth;
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
    }

    public interface ITwitterService
    {
        Task<bool> Connect();

        Task Disconnect();

        void SetAuthPin(string pin);

        Task<IEnumerable<Tweet>> GetLatestTweets();

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
