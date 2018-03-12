using LinqToTwitter;
using Mixer.Base.Model.OAuth;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class TwitterService : ITwitterService
    {
        private const string ClientID = "TUcAbNHvyJuK6rtLK1NnHZSBV";
        private const string ClientSecret = "OsaDTtU0ESBZRcIZuS0KpaZCraqNDIFC5qxQNRSQxvJlcQukcO";

        private OAuthTokenModel token;
        private IAuthorizer auth;

        private string authPin;

        public TwitterService() { }

        public TwitterService(OAuthTokenModel token)
        {
            this.token = token;
        }

        public async Task<bool> Connect()
        {
            if (this.token != null)
            {
                try
                {
                    SingleUserAuthorizer singleUserAuth = new SingleUserAuthorizer
                    {
                        CredentialStore = new SingleUserInMemoryCredentialStore
                        {
                            ConsumerKey = TwitterService.ClientID,
                            ConsumerSecret = TwitterService.ClientSecret,
                            AccessToken = this.token.clientID,
                            AccessTokenSecret = this.token.accessToken,
                        }
                    };
                    await singleUserAuth.AuthorizeAsync();

                    await this.InitializeInternal(singleUserAuth);

                    return true;
                }
                catch (Exception ex) { Logger.Log(ex); }
            }

            PinAuthorizer pinAuth = new PinAuthorizer()
            {
                CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = TwitterService.ClientID,
                    ConsumerSecret = TwitterService.ClientSecret,
                },
                GoToTwitterAuthorization = pageLink => Process.Start(pageLink),
                GetPin = () =>
                {
                    while (string.IsNullOrEmpty(this.authPin))
                    {
                        Task.Delay(1000).Wait();
                    }
                    return this.authPin;
                }
            };

            await pinAuth.AuthorizeAsync();
            this.authPin = null;

            if (!string.IsNullOrEmpty(pinAuth.CredentialStore.OAuthToken))
            {
                await this.InitializeInternal(pinAuth);
                return true;
            }
            return false;
        }

        public Task Disconnect()
        {
            return Task.FromResult(0);
        }

        public void SetAuthPin(string pin)
        {
            this.authPin = pin;
        }

        public async Task<IEnumerable<Tweet>> GetLatestTweets()
        {
            List<Tweet> results = new List<Tweet>();
            using (var twitterCtx = new TwitterContext(this.auth))
            {
                List<Status> tweets = await (from tweet in twitterCtx.Status
                                             where tweet.Type == StatusType.User && tweet.ScreenName == this.auth.CredentialStore.ScreenName
                                             select tweet).ToListAsync();

                foreach (Status tweet in tweets)
                {
                    results.Add(new Tweet()
                    {
                        ID = tweet.StatusID,
                        UserName = tweet.ScreenName,
                        Text = tweet.Text,
                        DateTime = new DateTimeOffset(tweet.CreatedAt, DateTimeOffset.UtcNow.Offset),
                    });
                }
            }
            return results;
        }

        public OAuthTokenModel GetOAuthTokenCopy()
        {
            if (this.token != null)
            {
                return new OAuthTokenModel()
                {
                    clientID = this.token.clientID,
                    authorizationCode = this.token.authorizationCode,
                    refreshToken = this.token.refreshToken,
                    accessToken = this.token.accessToken,
                    expiresIn = this.token.expiresIn
                };
            }
            return null;
        }

        private Task InitializeInternal(IAuthorizer auth)
        {
            this.auth = auth;

            this.token = new OAuthTokenModel();
            this.token.clientID = this.auth.CredentialStore.OAuthToken;
            this.token.accessToken = this.auth.CredentialStore.OAuthTokenSecret;

            return Task.FromResult(0);
        }
    }
}
