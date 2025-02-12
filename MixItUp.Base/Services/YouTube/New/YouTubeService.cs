using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.YouTubePartner.v1;
using Google.Apis.YouTubePartner.v1.Data;
using MixItUp.Base.Model;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Model.YouTube;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static Google.Apis.YouTube.v3.LiveBroadcastsResource.ListRequest;

namespace MixItUp.Base.Services.YouTube.New
{
    public static class YouTubeAPIModelExtensions
    {
        public static bool IsLive(this LiveBroadcast broadcast) { return string.Equals(broadcast?.Status?.LifeCycleStatus, "live", StringComparison.OrdinalIgnoreCase); }
    }

    public class YouTubeService : StreamingPlatformServiceBaseNew
    {
        private const string OAuthLoginBaseAddress = "https://accounts.google.com/o/oauth2/v2/auth";

        private const string OAuthBaseAddress = "https://www.googleapis.com/oauth2/v4/token";

        private const string BaseAddressFormat = "https://www.googleapis.com/youtube/v3/";

        private const string VideoIDShortsDataRegexPattern = "\\\"videoId\\\":\\\"\\w+\\\"";

        private static readonly IgnorePropertiesResolver requestPropertiesToIgnore = new IgnorePropertiesResolver(new List<string>() { "Service" });

        public override string Name { get { return Resources.YouTube; } }

        public override string ClientID { get { return "284178717531-kago2rk85ip02qb0vmlo8898m17s6oo8.apps.googleusercontent.com"; } }
        public override string ClientSecret { get { return ServiceManager.Get<SecretsService>().GetSecret("YouTubeSecret"); } }

        public override StreamingPlatformTypeEnum Platform { get { return StreamingPlatformTypeEnum.YouTube; } }

        public override bool IsConnected { get; protected set; }

        private UserCredential credential;

        /// <summary>
        /// The underlying YouTube service from Google's .NET client library.
        /// </summary>
        private Google.Apis.YouTube.v3.YouTubeService GoogleYouTubeService;

        /// <summary>
        /// The underlying YouTube Partner service from Google's .NET client library.
        /// </summary>
        private YouTubePartnerService GoogleYouTubePartnerService;

        public YouTubeService(IEnumerable<string> scopes, bool isBotService = false)
            : base(BaseAddressFormat, scopes, isBotService)
        { }

        public async override Task<Result> ManualConnect(CancellationToken cancellationToken)
        {
            Result result = await base.ManualConnect(cancellationToken);
            if (!result.Success)
            {
                return result;
            }

            this.BuildYouTubeServices();

            return new Result();
        }

        public async Task<LiveChatMessage> SendMessage(LiveBroadcast broadcast, string message)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                LiveChatMessage newMessage = new LiveChatMessage();
                newMessage.Snippet = new LiveChatMessageSnippet();
                newMessage.Snippet.LiveChatId = broadcast.Snippet.LiveChatId;
                newMessage.Snippet.Type = "textMessageEvent";
                newMessage.Snippet.TextMessageDetails = new LiveChatTextMessageDetails();
                newMessage.Snippet.TextMessageDetails.MessageText = message;

                LiveChatMessagesResource.InsertRequest request = this.GoogleYouTubeService.LiveChatMessages.Insert(newMessage, "snippet");
                LogRequest(request);

                LiveChatMessage liveChatMessage = await request.ExecuteAsync();
                LogResponse(request, liveChatMessage);
                return liveChatMessage;
            });
        }

        public async Task DeleteMessage(string messageID)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                LiveChatMessagesResource.DeleteRequest request = this.GoogleYouTubeService.LiveChatMessages.Delete(messageID);
                LogRequest(request);

                string response = await request.ExecuteAsync();
                LogResponse(request, response);
            });
        }

        public async Task<LiveChatModerator> ModUser(LiveBroadcast broadcast, UserV2ViewModel user)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                LiveChatModerator moderator = await AsyncRunner.RunAsync(async () =>
                {
                    LiveChatModerator liveChatModerator = new LiveChatModerator();
                    liveChatModerator.Snippet = new LiveChatModeratorSnippet();
                    liveChatModerator.Snippet.LiveChatId = broadcast.Snippet.LiveChatId;
                    liveChatModerator.Snippet.ModeratorDetails = new ChannelProfileDetails();
                    liveChatModerator.Snippet.ModeratorDetails.ChannelId = user.PlatformID;

                    LiveChatModeratorsResource.InsertRequest request = this.GoogleYouTubeService.LiveChatModerators.Insert(liveChatModerator, "snippet");
                    LogRequest(request);

                    moderator = await request.ExecuteAsync();
                    LogResponse(request, moderator);
                    return moderator;
                });

                if (moderator != null)
                {
                    user.GetPlatformData<YouTubeUserPlatformV2Model>(StreamingPlatformTypeEnum.YouTube).ModeratorID = moderator.Id;
                }

                return moderator;
            });
        }

        public async Task UnmodUser(UserV2ViewModel user)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                string moderatorID = user.GetPlatformData<YouTubeUserPlatformV2Model>(StreamingPlatformTypeEnum.YouTube)?.ModeratorID;
                if (!string.IsNullOrWhiteSpace(moderatorID))
                {
                    await AsyncRunner.RunAsync(async () =>
                    {
                        LiveChatModeratorsResource.DeleteRequest request = this.GoogleYouTubeService.LiveChatModerators.Delete(moderatorID);
                        LogRequest(request);

                        string response = await request.ExecuteAsync();
                        LogResponse(request, response);
                    });

                    user.GetPlatformData<YouTubeUserPlatformV2Model>(StreamingPlatformTypeEnum.YouTube).ModeratorID = null;
                }
            });
        }

        public async Task<LiveChatBan> TimeoutUser(LiveBroadcast broadcast, UserV2ViewModel user, int duration)
        {
            return await this.BanUserInternal(broadcast, user.PlatformID, "temporary", banDuration: duration);
        }

        public async Task<LiveChatBan> BanUser(LiveBroadcast broadcast, UserV2ViewModel user)
        {
            LiveChatBan ban = await this.BanUserInternal(broadcast, user.PlatformID, "permanent", banDuration: 0);
            if (ban != null)
            {
                user.GetPlatformData<YouTubeUserPlatformV2Model>(StreamingPlatformTypeEnum.YouTube).BanID = ban.Id;
            }
            return ban;
        }

        public async Task UnbanUser(UserV2ViewModel user)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                string banID = user.GetPlatformData<YouTubeUserPlatformV2Model>(StreamingPlatformTypeEnum.YouTube)?.BanID;
                if (!string.IsNullOrWhiteSpace(banID))
                {
                    await AsyncRunner.RunAsync(async () =>
                    {
                        LiveChatBansResource.DeleteRequest request = this.GoogleYouTubeService.LiveChatBans.Delete(banID);
                        LogRequest(request);

                        string response = await request.ExecuteAsync();
                        LogResponse(request, response);
                    });

                    user.GetPlatformData<YouTubeUserPlatformV2Model>(StreamingPlatformTypeEnum.YouTube).BanID = null;
                }
            });
        }

        public async Task<Channel> GetCurrentChannel()
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                ChannelsResource.ListRequest request = this.GoogleYouTubeService.Channels.List("snippet,statistics,contentDetails");
                request.Mine = true;
                request.MaxResults = 1;
                LogRequest(request);

                ChannelListResponse response = await request.ExecuteAsync();
                LogResponse(request, response);

                if (response.Items != null)
                {
                    return response.Items.FirstOrDefault();
                }
                return null;
            });
        }

        public async Task<Channel> GetChannelByID(string id)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                ChannelsResource.ListRequest request = this.GoogleYouTubeService.Channels.List("snippet,statistics,contentDetails");
                request.Id = new List<string>() { id };
                request.MaxResults = 1;
                LogRequest(request);

                ChannelListResponse response = await request.ExecuteAsync();
                LogResponse(request, response);

                return response?.Items?.FirstOrDefault();
            });
        }

        public async Task<Channel> GetChannelByUsername(string username)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                ChannelsResource.ListRequest request = this.GoogleYouTubeService.Channels.List("snippet,statistics");
                request.ForUsername = username;
                request.MaxResults = 1;
                LogRequest(request);

                ChannelListResponse response = await request.ExecuteAsync();
                LogResponse(request, response);

                if (response.Items != null)
                {
                    return response.Items.FirstOrDefault();
                }
                return null;
            });
        }

        public async Task<Result> ValidateAccountIsEnabledForLiveStreaming()
        {
            try
            {
                LiveBroadcastsResource.ListRequest request = this.GoogleYouTubeService.LiveBroadcasts.List("snippet,contentDetails,status");
                request.BroadcastType = BroadcastTypeEnum.All;
                request.Mine = true;
                request.MaxResults = 10;
                LogRequest(request);

                LiveBroadcastListResponse response = await request.ExecuteAsync();
                return new Result();
            }
            catch (GoogleApiException gex)
            {
                if (gex.HttpStatusCode == HttpStatusCode.Forbidden)
                {
                    return new Result(Resources.YouTubeAccountNotEnabledForLiveStreaming);
                }
                else
                {
                    return new Result(gex);
                }
            }
            catch (Exception ex)
            {
                return new Result(ex);
            }
        }

        public async Task<IEnumerable<LiveBroadcast>> GetActiveBroadcasts()
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                LiveBroadcastsResource.ListRequest request = this.GoogleYouTubeService.LiveBroadcasts.List("snippet,contentDetails,status");
                request.BroadcastType = BroadcastTypeEnum.All;
                request.BroadcastStatus = BroadcastStatusEnum.Active;
                request.MaxResults = 10;
                LogRequest(request);

                LiveBroadcastListResponse response = await request.ExecuteAsync();
                LogResponse(request, response);
                return response.Items;
            });
        }

        public async Task<IEnumerable<LiveBroadcast>> GetLatestBroadcasts()
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                LiveBroadcastsResource.ListRequest request = this.GoogleYouTubeService.LiveBroadcasts.List("snippet,contentDetails,status");
                request.BroadcastType = BroadcastTypeEnum.All;
                request.Mine = true;
                request.MaxResults = 10;
                LogRequest(request);

                LiveBroadcastListResponse response = await request.ExecuteAsync();
                LogResponse(request, response);
                return response.Items;
            });
        }

        public async Task<LiveBroadcast> GetBroadcastByID(string id)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                LiveBroadcastsResource.ListRequest request = this.GoogleYouTubeService.LiveBroadcasts.List("snippet,contentDetails,status");
                request.BroadcastType = BroadcastTypeEnum.All;
                request.Id = id;
                request.MaxResults = 10;
                LogRequest(request);

                LiveBroadcastListResponse response = await request.ExecuteAsync();
                LogResponse(request, response);
                return response.Items.FirstOrDefault();
            });
        }

        public async Task<LiveCuepoint> StartAdBreak(LiveBroadcast broadcast, long duration)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                LiveCuepoint liveCuepoint = new LiveCuepoint();
                liveCuepoint.BroadcastId = broadcast.Id;
                liveCuepoint.Settings = new CuepointSettings();
                liveCuepoint.Settings.CueType = "ad";
                liveCuepoint.Settings.DurationSecs = duration;

                LiveCuepointsResource.InsertRequest request = this.GoogleYouTubePartnerService.LiveCuepoints.Insert(liveCuepoint, broadcast.Snippet.ChannelId);
                LogRequest(request);

                liveCuepoint = await request.ExecuteAsync();
                LogResponse(request, liveCuepoint);
                return liveCuepoint;
            });
        }

        public async Task<IEnumerable<Video>> GetVideosByID(IEnumerable<string> ids, bool isOwned = true)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                string parts = "snippet,contentDetails,statistics,liveStreamingDetails,recordingDetails,status,topicDetails";
                if (isOwned)
                {
                    parts += ",fileDetails,processingDetails";
                }

                VideosResource.ListRequest request = this.GoogleYouTubeService.Videos.List(parts);
                request.MaxResults = ids.Count();
                request.Id = new List<string>(ids);
                LogRequest(request); 

                VideoListResponse response = await request.ExecuteAsync();
                LogResponse(request, response);

                return response?.Items;
            });
        }

        public async Task<IEnumerable<SearchResult>> GetLatestVideos(string channelID, int maxResults = 20)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                List<SearchResult> results = new List<SearchResult>();
                string pageToken = null;
                do
                {
                    SearchResource.ListRequest request = this.GoogleYouTubeService.Search.List("snippet");
                    request.ChannelId = channelID;
                    request.Type = "video";
                    request.EventType = SearchResource.ListRequest.EventTypeEnum.None;
                    request.Order = SearchResource.ListRequest.OrderEnum.Date;
                    request.MaxResults = Math.Min(maxResults, 50);
                    request.PageToken = pageToken;
                    LogRequest(request);

                    SearchListResponse response = await request.ExecuteAsync();
                    LogResponse(request, response);
                    results.AddRange(response.Items);
                    maxResults -= response.Items.Count;
                    pageToken = response.NextPageToken;

                } while (maxResults > 0 && !string.IsNullOrEmpty(pageToken));
                return results;
            });
        }

        public async Task<IEnumerable<string>> GetLatestShortIDs(string channelID)
        {
            channelID = "UCgLbxi89b8NvLevVp2ypbRA";


            return await AsyncRunner.RunAsync(async () =>
            {
                List<string> results = new List<string>();

                string response = await this.HttpClient.GetStringAsync($"https://www.youtube.com/channel/{channelID}/shorts");
                if (!string.IsNullOrEmpty(response))
                {
                    MatchCollection matches = Regex.Matches(response, VideoIDShortsDataRegexPattern);
                    foreach (Match match in matches)
                    {
                        JObject jobj = JObject.Parse($"{{{match.Value}}}");
                        if (jobj.TryGetValue("videoId", out JToken value))
                        {
                            string videoID = value.ToString();
                            if (!results.Contains(videoID))
                            {
                                results.Add(videoID);
                            }
                        }
                    }
                }
                return results;
            });
        }

        public async Task<LiveBroadcast> UpdateVideo(LiveBroadcast broadcast, string title = null, string description = null)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                broadcast.Snippet.Title = title;
                broadcast.Snippet.Description = description;

                LiveBroadcastsResource.UpdateRequest request = this.GoogleYouTubeService.LiveBroadcasts.Update(new LiveBroadcast()
                {
                    Id = broadcast.Id,
                    Snippet = new LiveBroadcastSnippet()
                    {
                        Title = title ?? broadcast.Snippet.Title,
                        Description = description ?? broadcast.Snippet.Description,
                        ScheduledStartTimeDateTimeOffset = broadcast.Snippet.ScheduledStartTimeDateTimeOffset,
                    }
                }, "snippet");
                LogRequest(request);

                LiveBroadcast response = await request.ExecuteAsync();
                LogResponse(request, response);
                return response;
            });
        }

        public async Task<IEnumerable<Subscription>> GetSubscribers(int maxResults = 1) { return await this.GetSubscriptions(myRecentSubscribers: true, maxResults: maxResults); }

        public async Task<Subscription> CheckIfSubscribed(string channelID, string userID)
        {
            var subscriptions = await this.GetSubscriptions(forChannelId: channelID, channelId: userID, maxResults: 1);
            if (subscriptions != null)
            {
                return subscriptions.FirstOrDefault();
            }
            return null;
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptions(bool mySubscriptions = false, bool myRecentSubscribers = false, bool mySubscribers = false, string forChannelId = null, string channelId = null, int maxResults = 1)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                List<Subscription> results = new List<Subscription>();
                string pageToken = null;
                do
                {
                    SubscriptionsResource.ListRequest request = this.GoogleYouTubeService.Subscriptions.List("snippet,contentDetails");
                    if (mySubscriptions)
                    {
                        request.Mine = true;
                    }
                    else if (myRecentSubscribers)
                    {
                        request.MyRecentSubscribers = myRecentSubscribers;
                    }
                    else if (mySubscribers)
                    {
                        request.MySubscribers = mySubscribers;
                    }
                    else if (!string.IsNullOrEmpty(forChannelId) && !string.IsNullOrEmpty(channelId))
                    {
                        request.ForChannelId = forChannelId;
                        request.ChannelId = channelId;
                    }
                    request.MaxResults = Math.Min(maxResults, 50);
                    request.PageToken = pageToken;
                    LogRequest(request);

                    SubscriptionListResponse response = await request.ExecuteAsync();
                    LogResponse(request, response);
                    results.AddRange(response.Items);
                    maxResults -= response.Items.Count;
                    pageToken = response.NextPageToken;

                } while (maxResults > 0 && !string.IsNullOrEmpty(pageToken));
                return results;
            });
        }

        public async Task<IEnumerable<MembershipsLevel>> GetMembershipLevels()
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                MembershipsLevelsResource.ListRequest request = this.GoogleYouTubeService.MembershipsLevels.List("id,snippet");
                LogRequest(request);

                MembershipsLevelListResponse response = await request.ExecuteAsync();
                LogResponse(request, response);
                return response.Items;
            });
        }

        public async Task<IEnumerable<Member>> GetChannelMemberships(int maxResults = 1)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                List<Member> results = new List<Member>();
                string pageToken = null;
                do
                {
                    MembersResource.ListRequest request = this.GoogleYouTubeService.Members.List("id,snippet");
                    request.MaxResults = Math.Min(maxResults, 50);
                    request.PageToken = pageToken;
                    LogRequest(request);

                    MemberListResponse response = await request.ExecuteAsync();
                    LogResponse(request, response);
                    results.AddRange(response.Items);
                    maxResults -= response.Items.Count;
                    pageToken = response.NextPageToken;

                } while (maxResults > 0 && !string.IsNullOrEmpty(pageToken));
                return results;
            });
        }

        public async Task<IEnumerable<Member>> GetMembers(int maxResults = 1) { return await this.GetMembersInternal(maxResults: maxResults); }

        public async Task<Member> CheckIfMember(string userID)
        {
            var memberships = await this.GetMembersInternal(filterToSpecificMemberChannelId: userID, maxResults: 1);
            if (memberships != null)
            {
                return memberships.FirstOrDefault();
            }
            return null;
        }

        internal async Task<IEnumerable<Member>> GetMembersInternal(bool onlyUpdates = false, string filterToSpecificLevel = null, string filterToSpecificMemberChannelId = null, int maxResults = 1)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                List<Member> results = new List<Member>();
                string pageToken = null;
                do
                {
                    MembersResource.ListRequest request = this.GoogleYouTubeService.Members.List("snippet");
                    if (onlyUpdates)
                    {
                        request.Mode = MembersResource.ListRequest.ModeEnum.Updates;
                    }
                    if (!string.IsNullOrEmpty(filterToSpecificLevel))
                    {
                        request.HasAccessToLevel = filterToSpecificLevel;
                    }
                    if (!string.IsNullOrEmpty(filterToSpecificMemberChannelId))
                    {
                        request.FilterByMemberChannelId = filterToSpecificMemberChannelId;
                    }

                    request.MaxResults = Math.Min(maxResults, 50);
                    request.PageToken = pageToken;
                    LogRequest(request);

                    MemberListResponse response = await request.ExecuteAsync();
                    LogResponse(request, response);
                    results.AddRange(response.Items);
                    maxResults -= response.Items.Count;
                    pageToken = response.NextPageToken;

                } while (maxResults > 0 && !string.IsNullOrEmpty(pageToken));
                return results;
            });
        }

        // Chat

        public async Task<LiveChatMessagesResultModel> GetChatMessages(LiveBroadcast broadcast, string nextResultsToken = null, int maxResults = 0)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                LiveChatMessagesResource.ListRequest request = this.GoogleYouTubeService.LiveChatMessages.List(broadcast.Snippet.LiveChatId, "id,snippet,authorDetails");

                if (maxResults > 0)
                {
                    request.MaxResults = maxResults;
                }

                if (!string.IsNullOrEmpty(nextResultsToken))
                {
                    request.PageToken = nextResultsToken;
                }

                LogRequest(request);

                LiveChatMessageListResponse response = await request.ExecuteAsync();
                LogResponse(request, response);

                return new LiveChatMessagesResultModel(response);
            });
        }

        public async Task<IEnumerable<Model.YouTube.YouTubeChatEmoteModel>> GetChatEmotes()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync("https://www.gstatic.com/youtube/img/emojis/emojis-svg-8.json");
                    if (response.IsSuccessStatusCode)
                    {
                        return JSONSerializerHelper.DeserializeFromString<List<Model.YouTube.YouTubeChatEmoteModel>>(await response.Content.ReadAsStringAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new List<Model.YouTube.YouTubeChatEmoteModel>();
        }

        public async Task<IEnumerable<LiveChatModerator>> GetModerators(LiveBroadcast broadcast, int maxResults = 1)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                List<LiveChatModerator> results = new List<LiveChatModerator>();
                string pageToken = null;
                do
                {
                    LiveChatModeratorsResource.ListRequest request = this.GoogleYouTubeService.LiveChatModerators.List(broadcast.Snippet.LiveChatId, "id,snippet");
                    request.MaxResults = Math.Min(maxResults, 50);
                    request.PageToken = pageToken;
                    LogRequest(request);

                    LiveChatModeratorListResponse response = await request.ExecuteAsync();
                    LogResponse(request, response);
                    results.AddRange(response.Items);
                    maxResults -= response.Items.Count;
                    pageToken = response.NextPageToken;

                } while (maxResults > 0 && !string.IsNullOrEmpty(pageToken));
                return results;
            });
        }

        private async Task<LiveChatBan> BanUserInternal(LiveBroadcast broadcast, string userID, string banType, int banDuration = 0)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                LiveChatBan ban = new LiveChatBan();
                ban.Snippet = new LiveChatBanSnippet();
                ban.Snippet.LiveChatId = broadcast.Snippet.LiveChatId;
                ban.Snippet.Type = banType;
                if (banDuration > 0)
                {
                    ban.Snippet.BanDurationSeconds = Convert.ToUInt64(banDuration);
                }
                ban.Snippet.BannedUserDetails = new ChannelProfileDetails();
                ban.Snippet.BannedUserDetails.ChannelId = userID;

                LiveChatBansResource.InsertRequest request = this.GoogleYouTubeService.LiveChatBans.Insert(ban, "snippet");
                LogRequest(request);

                ban = await request.ExecuteAsync();
                LogResponse(request, ban);
                return ban;
            });
        }

        protected async override Task<string> GetAuthorizationCodeURL(IEnumerable<string> scopes, string state, bool forceApprovalPrompt = false)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "client_id", ClientID },
                { "response_type", LocalOAuthHttpListenerServer.AUTHORIZATION_CODE_URL_PARAMETER },
                { "scope", ConvertClientScopesToString(scopes) },
                { "redirect_uri", LocalOAuthHttpListenerServer.REDIRECT_URL },
            };

            if (forceApprovalPrompt)
            {
                parameters.Add("force_verify", "force");
            }

            FormUrlEncodedContent content = new FormUrlEncodedContent(parameters.AsEnumerable());
            return OAuthLoginBaseAddress + "?" + await content.ReadAsStringAsync();
        }

        protected async override Task<OAuthTokenModel> RequestOAuthToken(string authorizationCode, IEnumerable<string> scopes, string state)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "client_id", this.ClientID },
                { "client_secret", this.ClientSecret },
                { "code", authorizationCode },
                { "grant_type", "authorization_code" },
                { "redirect_uri", LocalOAuthHttpListenerServer.REDIRECT_URL },
            };
            FormUrlEncodedContent content = new FormUrlEncodedContent(parameters.AsEnumerable());

            OAuthTokenModel token = await this.HttpClient.PostAsync<OAuthTokenModel>(OAuthBaseAddress, new StringContent(await content.ReadAsStringAsync(), Encoding.UTF8, "application/x-www-form-urlencoded"));
            if (token != null)
            {
                token.clientID = this.ClientID;
                token.ScopeList = OAuthTokenModel.GenerateScopeList(scopes);
                return token;
            }
            return null;
        }

        protected override async Task RefreshOAuthToken()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "client_id", this.ClientID },
                { "client_secret", this.ClientSecret },
                { "refresh_token", OAuthToken.refreshToken },
                { "grant_type", "refresh_token" },
            };
            FormUrlEncodedContent content = new FormUrlEncodedContent(parameters.AsEnumerable());

            OAuthTokenModel newToken = await this.HttpClient.PostAsync<OAuthTokenModel>(OAuthBaseAddress, new StringContent(await content.ReadAsStringAsync(), Encoding.UTF8, "application/x-www-form-urlencoded"));
            if (newToken != null)
            {
                newToken.clientID = this.ClientID;
                newToken.refreshToken = OAuthToken.refreshToken;
                newToken.ScopeList = OAuthToken.ScopeList;
                OAuthToken = newToken;

                this.BuildYouTubeServices();
            }
        }

        private void BuildYouTubeServices()
        {
            this.credential = new UserCredential(new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets()
                    {
                        ClientId = this.ClientID,
                        ClientSecret = this.ClientSecret
                    },
                }),
                "user",
                new TokenResponse()
                {
                    AccessToken = this.OAuthToken.accessToken,
                    ExpiresInSeconds = this.OAuthToken.expiresIn,
                    RefreshToken = this.OAuthToken.refreshToken,
                }
            );

            this.GoogleYouTubeService = new Google.Apis.YouTube.v3.YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = this.credential,
                ApplicationName = this.ClientID
            });

            this.GoogleYouTubePartnerService = new YouTubePartnerService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = this.credential,
                ApplicationName = this.ClientID
            });
        }

        /// <summary>
        /// Logs a YouTube service request.
        /// </summary>
        /// <typeparam name="T">The type of the request</typeparam>
        /// <param name="request">The request to log</param>
        private void LogRequest<T>(YouTubeBaseServiceRequest<T> request)
        {
            if (Logger.Level == LogLevel.Debug)
            {
                Logger.Log(LogLevel.Debug, "Rest API Request Sent: " + request.RestPath + " - " + JSONSerializerHelper.SerializeToString(request, propertiesToIgnore: requestPropertiesToIgnore));
            }
        }

        /// <summary>
        /// Logs a YouTube service request.
        /// </summary>
        /// <typeparam name="T">The type of the request</typeparam>
        /// <param name="request">The request to log</param>
        protected void LogRequest<T>(YouTubePartnerBaseServiceRequest<T> request)
        {
            if (Logger.Level == LogLevel.Debug)
            {
                Logger.Log(LogLevel.Debug, "Rest API Request Sent: " + request.RestPath + " - " + JSONSerializerHelper.SerializeToString(request, propertiesToIgnore: requestPropertiesToIgnore));
            }
        }

        /// <summary>
        /// Logs a YouTube service response
        /// </summary>
        /// <typeparam name="T">The type of the request</typeparam>
        /// <param name="request">The request to log</param>
        /// <param name="response">The response to log</param>
        private void LogResponse<T>(YouTubeBaseServiceRequest<T> request, IDirectResponseSchema response)
        {
            if (Logger.Level == LogLevel.Debug)
            {
                Logger.Log(LogLevel.Debug, "Rest API Request Complete: " + request.RestPath + " - " + JSONSerializerHelper.SerializeToString(response));
            }
        }

        /// <summary>
        /// Logs a YouTube service response
        /// </summary>
        /// <typeparam name="T">The type of the request</typeparam>
        /// <param name="request">The request to log</param>
        /// <param name="response">The response to log</param>
        private void LogResponse<T>(YouTubeBaseServiceRequest<T> request, string response)
        {
            if (Logger.Level == LogLevel.Debug)
            {
                Logger.Log(LogLevel.Debug, "Rest API Request Complete: " + request.RestPath + " - " + response);
            }
        }

        /// <summary>
        /// Logs a YouTube service response
        /// </summary>
        /// <typeparam name="T">The type of the request</typeparam>
        /// <param name="request">The request to log</param>
        /// <param name="response">The response to log</param>
        private void LogResponse<T>(YouTubePartnerBaseServiceRequest<T> request, IDirectResponseSchema response)
        {
            if (Logger.Level == LogLevel.Debug)
            {
                Logger.Log(LogLevel.Debug, "Rest API Request Complete: " + request.RestPath + " - " + JSONSerializerHelper.SerializeToString(response));
            }
        }

        private string ConvertClientScopesToString(IEnumerable<string> scopes)
        {
            return string.Join(" ", scopes);
        }
    }
}
