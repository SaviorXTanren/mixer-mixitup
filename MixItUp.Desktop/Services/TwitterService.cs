using LinqToTwitter;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class TwitterService : ITwitterService, IDisposable
    {
        private const string ClientID = "3xNVEnW8FVc4iywkW8CCDZQsC";

        private OAuthTokenModel token;
        private IAuthorizer auth;

        private string authPin;

        private DateTimeOffset lastTweet = DateTimeOffset.MinValue;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

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

                    if (await this.InitializeInternal(singleUserAuth))
                    {
                        return true;
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }
            }

            try
            {
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

                return await this.InitializeInternal(pinAuth);
            }
            catch (Exception ex) { Logger.Log(ex); }

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
                            UserID = tweet.UserID,
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

        public async Task<IEnumerable<Tweet>> GetRetweets(Tweet tweet)
        {
            List<Tweet> results = new List<Tweet>();
            try
            {
                using (var twitterCtx = new TwitterContext(this.auth))
                {
                    List<Status> retweets = await (from retweet in twitterCtx.Status where retweet.Type == StatusType.Retweets && retweet.ID == tweet.ID select retweet).ToListAsync();

                    if (retweets != null)
                    {
                        foreach (Status retweet in retweets)
                        {
                            ulong.TryParse(retweet.User.UserIDResponse, out ulong userID);

                            Tweet t = new Tweet()
                            {
                                ID = retweet.StatusID,
                                UserID = userID,
                                UserName = retweet.User.ScreenNameResponse,
                                Text = retweet.Text,
                                DateTime = new DateTimeOffset(retweet.CreatedAt, DateTimeOffset.UtcNow.Offset),
                            };

                            foreach (var urlEntry in retweet.Entities.UrlEntities)
                            {
                                t.Links.Add((!string.IsNullOrEmpty(urlEntry.ExpandedUrl) ? urlEntry.ExpandedUrl : urlEntry.DisplayUrl));
                            }

                            results.Add(t);
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
        }

        public async Task SendTweet(string tweet, string imagePath = null)
        {
            try
            {
                if (this.lastTweet.TotalMinutesFromNow() > 5)
                {
                    this.lastTweet = DateTimeOffset.Now;

                    using (var twitterCtx = new TwitterContext(this.auth))
                    {
                        List<ulong> mediaIds = new List<ulong>();

                        try
                        {
                            if (!string.IsNullOrEmpty(imagePath))
                            {
                                // Download the image and upload to Twitter
                                using (WebClient client = new WebClient())
                                {
                                    var bytes = await Task.Run<byte[]>(async () => { return await client.DownloadDataTaskAsync(imagePath); });

                                    using (Image img = ByteArrayToImage(bytes))
                                    {
                                        string mediaType = $"image/{new ImageFormatConverter().ConvertToString(img.RawFormat).ToLower()}";

                                        Media media = await twitterCtx.UploadMediaAsync(bytes, mediaType, "tweet_image");
                                        mediaIds.Add(media.MediaID);
                                    }
                                }
                            }
                        }
                        catch (Exception ex) { Logger.Log(ex); }

                        await twitterCtx.TweetAsync(tweet, mediaIds);
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task UpdateName(string name)
        {
            try
            {
                using (var twitterCtx = new TwitterContext(this.auth))
                {
                    await twitterCtx.UpdateAccountProfileAsync(name, null, null, null, true);
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

        private static Image ByteArrayToImage(byte[] bmpBytes)
        {
            Image image = null;
            using (MemoryStream stream = new MemoryStream(bmpBytes))
            {
                image = Image.FromStream(stream);
            }

            return image;
        }

        private Task<bool> InitializeInternal(IAuthorizer auth)
        {
            this.auth = auth;
            if (!string.IsNullOrEmpty(this.auth.CredentialStore.OAuthToken))
            {
                this.token = new OAuthTokenModel();

                this.token.accessToken = this.auth.CredentialStore.OAuthToken;
                this.token.refreshToken = this.auth.CredentialStore.OAuthTokenSecret;

                this.token.clientID = this.auth.CredentialStore.UserID.ToString();
                this.token.authorizationCode = this.auth.CredentialStore.ScreenName;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(this.BackgroundRetweetCheck, this.cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        private async Task BackgroundRetweetCheck()
        {
            HashSet<ulong> existingRetweets = new HashSet<ulong>();

            IEnumerable<Tweet> tweets = await this.GetLatestTweets();
            if (tweets.Count() > 0)
            {
                Tweet streamTweet = tweets.FirstOrDefault(t => t.IsStreamTweet);
                if (streamTweet != null)
                {
                    IEnumerable<Tweet> retweets = await this.GetRetweets(streamTweet);
                    if (retweets != null)
                    {
                        foreach (Tweet retweet in retweets)
                        {
                            existingRetweets.Add(retweet.ID);
                        }
                    }
                }
            }

            while (!this.cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    EventCommand command = ChannelSession.Settings.EventCommands.FirstOrDefault(c => c.OtherEventType.Equals(OtherEventTypeEnum.TwitterStreamTweetRetweet));
                    if (command != null)
                    {
                        tweets = await this.GetLatestTweets();
                        if (tweets.Count() > 0)
                        {
                            Tweet streamTweet = tweets.FirstOrDefault(t => t.IsStreamTweet);
                            if (streamTweet != null)
                            {
                                IEnumerable<Tweet> retweets = await this.GetRetweets(streamTweet);
                                if (retweets != null)
                                {
                                    foreach (Tweet retweet in retweets)
                                    {
                                        if (!existingRetweets.Contains(retweet.ID))
                                        {
                                            existingRetweets.Add(retweet.ID);

                                            IEnumerable<UserViewModel> users = await ChannelSession.ActiveUsers.GetAllUsers();
                                            UserViewModel user = users.FirstOrDefault(u => u.TwitterURL != null && u.TwitterURL.Equals(string.Format("https://twitter.com/{0}", retweet.UserName)));
                                            if (user == null)
                                            {
                                                UserModel userModel = await ChannelSession.Connection.GetUser(retweet.UserName);
                                                if (userModel != null)
                                                {
                                                    user = new UserViewModel(userModel);
                                                }
                                                else
                                                {
                                                    user = new UserViewModel(0, retweet.UserName);
                                                }
                                            }

                                            await command.Perform(user);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }

                await Task.Delay(60000);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.cancellationTokenSource.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
