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
                            ConsumerSecret = ChannelSession.SecretManager.GetSecret("TwitterSecret"),

                            AccessToken = this.token.accessToken,
                            AccessTokenSecret = this.token.refreshToken,

                            UserID = ulong.Parse(this.token.clientID),
                            ScreenName = this.token.authorizationCode,
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
                    ConsumerSecret = ChannelSession.SecretManager.GetSecret("TwitterSecret"),
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
            try
            {
                using (var twitterCtx = new TwitterContext(this.auth))
                {
                    List<Status> tweets = await (from tweet in twitterCtx.Status
                                                 where tweet.Type == StatusType.User && tweet.ScreenName == this.auth.CredentialStore.ScreenName && tweet.TweetMode == TweetMode.Extended
                                                 select tweet).ToListAsync();

                    foreach (Status tweet in tweets)
                    {
                        Tweet t = new Tweet()
                        {
                            ID = tweet.StatusID,
                            UserName = tweet.ScreenName,
                            Text = tweet.FullText,
                            DateTime = new DateTimeOffset(tweet.CreatedAt, DateTimeOffset.UtcNow.Offset),
                        };

                        foreach (var urlEntry in tweet.Entities.UrlEntities)
                        {
                            t.Links.Add((!string.IsNullOrEmpty(urlEntry.ExpandedUrl) ? urlEntry.ExpandedUrl : urlEntry.DisplayUrl));
                        }

                        results.Add(t);
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
        }

        public async Task SendTweet(string tweet)
        {
            try
            {
                using (var twitterCtx = new TwitterContext(this.auth))
                {
                    await twitterCtx.TweetAsync(tweet);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
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

            this.token.accessToken = this.auth.CredentialStore.OAuthToken;
            this.token.refreshToken = this.auth.CredentialStore.OAuthTokenSecret;

            this.token.clientID = this.auth.CredentialStore.UserID.ToString();
            this.token.authorizationCode = this.auth.CredentialStore.ScreenName;

            return Task.FromResult(0);
        }
    }
}
