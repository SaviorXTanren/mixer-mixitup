using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Twitch.Base;
using Twitch.Base.Models.NewAPI.Games;
using Twitch.Base.Models.NewAPI.Streams;
using Twitch.Base.Models.V5.Channel;
using Twitch.Base.Models.V5.Teams;
using Twitch.Base.Services.V5API;
using NewAPI = Twitch.Base.Models.NewAPI;
using NewAPIServices = Twitch.Base.Services.NewAPI;
using V5API = Twitch.Base.Models.V5;

namespace MixItUp.Base.Services.Twitch
{
    public interface ITwitchPlatformService
    {

    }

    public class TwitchPlatformService : StreamingPlatformServiceBase, ITwitchPlatformService
    {
        public const string ClientID = "50ipfqzuqbv61wujxcm80zyzqwoqp1";

        public static readonly List<OAuthClientScopeEnum> StreamerScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.channel_check_subscription,
            OAuthClientScopeEnum.channel_commercial,
            OAuthClientScopeEnum.channel_editor,
            OAuthClientScopeEnum.channel_read,
            OAuthClientScopeEnum.channel_subscriptions,
            OAuthClientScopeEnum.user_follows_edit,
            OAuthClientScopeEnum.user_read,
            OAuthClientScopeEnum.user_subscriptions,

            OAuthClientScopeEnum.bits__read,
            OAuthClientScopeEnum.channel__manage__broadcast,
            OAuthClientScopeEnum.channel__manage__redemptions,
            OAuthClientScopeEnum.channel__moderate,
            OAuthClientScopeEnum.channel__read__redemptions,
            OAuthClientScopeEnum.channel__read__subscriptions,
            OAuthClientScopeEnum.clips__edit,
            OAuthClientScopeEnum.chat__edit,
            OAuthClientScopeEnum.chat__read,
            OAuthClientScopeEnum.user__edit,
            OAuthClientScopeEnum.user__edit__broadcast,
            OAuthClientScopeEnum.user__edit__follows,
            OAuthClientScopeEnum.whispers__read,
            OAuthClientScopeEnum.whispers__edit,
        };

        public static readonly List<OAuthClientScopeEnum> BotScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.channel_editor,
            OAuthClientScopeEnum.channel_read,

            OAuthClientScopeEnum.user_read,

            OAuthClientScopeEnum.bits__read,
            OAuthClientScopeEnum.channel__moderate,
            OAuthClientScopeEnum.chat__edit,
            OAuthClientScopeEnum.chat__read,
            OAuthClientScopeEnum.user__edit,
            OAuthClientScopeEnum.whispers__read,
            OAuthClientScopeEnum.whispers__edit,
        };

        public static async Task<Result<TwitchPlatformService>> Connect(OAuthTokenModel token)
        {
            try
            {
                TwitchConnection connection = await TwitchConnection.ConnectViaOAuthToken(token);
                if (connection != null)
                {
                    return new Result<TwitchPlatformService>(new TwitchPlatformService(connection));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result<TwitchPlatformService>(ex);
            }
            return new Result<TwitchPlatformService>("Twitch OAuth token could not be used");
        }

        public static async Task<Result<TwitchPlatformService>> ConnectUser()
        {
            return await TwitchPlatformService.Connect(TwitchPlatformService.StreamerScopes);
        }

        public static async Task<Result<TwitchPlatformService>> ConnectBot()
        {
            return await TwitchPlatformService.Connect(TwitchPlatformService.BotScopes);
        }

        public static async Task<Result<TwitchPlatformService>> Connect(IEnumerable<OAuthClientScopeEnum> scopes)
        {
            try
            {
                TwitchConnection connection = await TwitchConnection.ConnectViaLocalhostOAuthBrowser(TwitchPlatformService.ClientID, ServiceManager.Get<SecretsService>().GetSecret("TwitchSecret"),
                    scopes, forceApprovalPrompt: true, successResponse: OAuthExternalServiceBase.LoginRedirectPageHTML);
                if (connection != null)
                {
                    return new Result<TwitchPlatformService>(new TwitchPlatformService(connection));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result<TwitchPlatformService>(ex);
            }
            return new Result<TwitchPlatformService>("Failed to connect to establish connection to Twitch");
        }

        public static DateTimeOffset GetTwitchDateTime(string dateTime)
        {
            try
            {
                if (!string.IsNullOrEmpty(dateTime))
                {
                    if (dateTime.Contains("Z", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (DateTimeOffset.TryParse(dateTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset startUTC))
                        {
                            return startUTC.ToCorrectLocalTime();
                        }
                    }
                    else if (DateTime.TryParse(dateTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime start))
                    {
                        return new DateTimeOffset(start).ToCorrectLocalTime();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"{dateTime} - {ex}");
            }
            return DateTimeOffset.MinValue;
        }

        public TwitchConnection Connection { get; private set; }

        public override string Name { get { return "Twitch Connection"; } }

        public TwitchPlatformService(TwitchConnection connection)
        {
            this.Connection = connection;
        }

        // V5 API Methods

        public async Task<V5API.Users.UserModel> GetV5APIUserByLogin(string login) { return await AsyncRunner.RunAsync(this.Connection.V5API.Users.GetUserByLogin(login)); }

        public async Task<V5API.Channel.PrivateChannelModel> GetCurrentV5APIChannel() { return await AsyncRunner.RunAsync(this.Connection.V5API.Channels.GetCurrentChannel()); }

        public async Task<V5API.Channel.ChannelModel> GetV5APIChannel(string channelID) { return await AsyncRunner.RunAsync(this.Connection.V5API.Channels.GetChannelByID(channelID)); }

        public async Task UpdateV5Channel(V5API.Channel.ChannelModel channel, string status = null, GameModel game = null)
        {
            ChannelUpdateModel update = new ChannelUpdateModel()
            {
                status = (!string.IsNullOrEmpty(status)) ? status : null,
                game = (game != null) ? game.name : null
            };
            await AsyncRunner.RunAsync(this.Connection.V5API.Channels.UpdateChannel(channel, update));
        }

        public async Task<IEnumerable<V5API.Users.UserModel>> GetV5APIChannelEditors(V5API.Channel.ChannelModel channel) { return await this.RunAsync(this.Connection.V5API.Channels.GetChannelEditors(channel)); }

        public async Task<V5API.Users.UserSubscriptionModel> CheckIfSubscribedV5(V5API.Channel.ChannelModel channel, V5API.Users.UserModel userToCheck) { return await AsyncRunner.RunAsync(this.Connection.V5API.Channels.GetChannelUserSubscription(channel, userToCheck)); }

        public async Task<IEnumerable<V5API.Users.UserSubscriptionModel>> GetSubscribersV5(V5API.Channel.ChannelModel channel, int maxResults = 1) { return await this.RunAsync(this.Connection.V5API.Channels.GetChannelSubscribers(channel, maxResults)); }

        public async Task<long> GetSubscriberCountV5(V5API.Channel.ChannelModel channel) { return await AsyncRunner.RunAsync(this.Connection.V5API.Channels.GetChannelSubscribersCount(channel)); }

        public async Task<IEnumerable<V5API.Emotes.EmoteModel>> GetEmotesForUserV5(V5API.Users.UserModel user) { return await this.RunAsync(this.Connection.V5API.Users.GetUserEmotes(user)); }

        public async Task<V5API.Streams.StreamModel> GetV5LiveStream(V5API.Channel.ChannelModel channel) { return await AsyncRunner.RunAsync(this.Connection.V5API.Streams.GetChannelStream(channel)); }

        public async Task<IEnumerable<V5API.Streams.StreamModel>> GetV5ChannelStreams(IEnumerable<string> channelIDs, int maxResults) { return await this.RunAsync(this.Connection.V5API.Streams.GetStreams(channelIDs: channelIDs, streamType: StreamType.Live, maxResults: maxResults)); }

        public async Task<IEnumerable<V5API.Streams.StreamModel>> GetV5FollowedStreams(int maxResults) { return await this.RunAsync(this.Connection.V5API.Streams.GetFollowedStreams(StreamType.Live, maxResults)); }

        public async Task<IEnumerable<V5API.Streams.FeaturedStreamModel>> GetV5FeaturedStreams(int maxResults) { return await this.RunAsync(this.Connection.V5API.Streams.GetFeaturedStreams(maxResults)); }

        public async Task<IEnumerable<V5API.Teams.TeamModel>> GetChannelTeams(ChannelModel channel) { return await this.RunAsync(this.Connection.V5API.Channels.GetChannelTeams(channel)); }

        public async Task<TeamDetailsModel> GetTeamDetails(TeamModel team) { return await AsyncRunner.RunAsync(this.Connection.V5API.Teams.GetTeam(team)); }

        // New API Methods

        public async Task<NewAPI.Users.UserModel> GetNewAPICurrentUser() { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Users.GetCurrentUser()); }

        public async Task<NewAPI.Users.UserModel> GetNewAPIUserByID(string userID) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Users.GetUserByID(userID)); }

        public async Task<NewAPI.Users.UserModel> GetNewAPIUserByLogin(string login) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Users.GetUserByLogin(login)); }

        public async Task<IEnumerable<NewAPI.Users.UserFollowModel>> GetNewAPIFollowers(NewAPI.Users.UserModel channel, int maxResult = 1)
        {
            return await this.RunAsync(this.Connection.NewAPI.Users.GetFollows(to: channel, maxResults: maxResult));
        }

        public async Task<NewAPI.Users.UserFollowModel> CheckIfFollowsNewAPI(NewAPI.Users.UserModel channel, NewAPI.Users.UserModel userToCheck)
        {
            IEnumerable<NewAPI.Users.UserFollowModel> follows = await this.RunAsync(this.Connection.NewAPI.Users.GetFollows(from: userToCheck, to: channel, maxResults: 1));
            if (follows != null)
            {
                return follows.FirstOrDefault();
            }
            return null;
        }

        public async Task<bool> FollowUser(NewAPI.Users.UserModel channel, NewAPI.Users.UserModel user) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Users.FollowUser(channel, user)); }

        public async Task<bool> UnfollowUser(NewAPI.Users.UserModel channel, NewAPI.Users.UserModel user) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Users.UnfollowUser(channel, user)); }

        public async Task<NewAPI.Games.GameModel> GetNewAPIGameByID(string id) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Games.GetGameByID(id)); }

        public async Task<IEnumerable<NewAPI.Games.GameModel>> GetNewAPIGamesByName(string name) { return await this.RunAsync(this.Connection.NewAPI.Games.GetGamesByName(name)); }

        public async Task<IEnumerable<NewAPI.Games.GameModel>> GetNewAPIGamesByIDs(IEnumerable<string> ids) { return await this.RunAsync(this.Connection.NewAPI.Games.GetGamesByID(ids)); }

        public async Task<NewAPI.Channels.ChannelInformationModel> GetChannelInformation(NewAPI.Users.UserModel channel) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Channels.GetChannelInformation(channel)); }

        public async Task<bool> UpdateChannelInformation(NewAPI.Users.UserModel channel, string title = null, string gameID = null) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Channels.UpdateChannelInformation(channel, title, gameID)); }

        public async Task<IEnumerable<NewAPI.Tags.TagModel>> GetStreamTags() { return await this.RunAsync(this.Connection.NewAPI.Tags.GetStreamTags(int.MaxValue)); }

        public async Task<IEnumerable<NewAPI.Tags.TagModel>> GetStreamTagsForChannel(NewAPI.Users.UserModel channel) { return await this.RunAsync(this.Connection.NewAPI.Tags.GetStreamTagsForBroadcaster(channel)); }

        public async Task<bool> UpdateStreamTagsForChannel(NewAPI.Users.UserModel channel, IEnumerable<NewAPI.Tags.TagModel> tags) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Tags.UpdateStreamTags(channel, tags)); }

        public async Task<CreatedStreamMarkerModel> CreateStreamMarker(NewAPI.Users.UserModel channel, string description) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Streams.CreateStreamMarker(channel, description)); }

        public async Task<bool> GetStreamTagsForChannel(NewAPI.Users.UserModel channel, IEnumerable<NewAPI.Tags.TagModel> tags) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Tags.UpdateStreamTags(channel, tags)); }

        public async Task<NewAPI.Streams.StreamModel> GetStream(NewAPI.Users.UserModel user)
        {
            IEnumerable<NewAPI.Streams.StreamModel> results = await this.RunAsync(this.Connection.NewAPI.Streams.GetStreamsByUserIDs(userIDs: new List<string>() { user.id }));
            if (results != null)
            {
                return results.FirstOrDefault();
            }
            return null;
        }

        public async Task<IEnumerable<NewAPI.Streams.StreamModel>> GetGameStreams(string gameID, int maxResults) { return await this.RunAsync(this.Connection.NewAPI.Streams.GetStreams(gameIDs: new List<string>() { gameID }, maxResults: maxResults)); }

        public async Task<IEnumerable<NewAPI.Streams.StreamModel>> GetLanguageStreams(string language, int maxResults) { return await this.RunAsync(this.Connection.NewAPI.Streams.GetStreams(languages: new List<string>() { language }, maxResults: maxResults)); }

        public async Task<NewAPI.Ads.AdResponseModel> RunAd(NewAPI.Users.UserModel channel, int length) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Ads.RunAd(channel, length)); }

        public async Task<NewAPI.Clips.ClipCreationModel> CreateClip(NewAPI.Users.UserModel channel, bool delay) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Clips.CreateClip(channel, delay)); }

        public async Task<NewAPI.Clips.ClipModel> GetClip(NewAPI.Clips.ClipCreationModel clip) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Clips.GetClip(clip)); }

        public async Task<NewAPI.Bits.BitsLeaderboardModel> GetBitsLeaderboard(NewAPIServices.BitsLeaderboardPeriodEnum period, int count) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Bits.GetBitsLeaderboard(startedAt: DateTimeOffset.Now, period: period, count: count)); }

        public async Task<IEnumerable<NewAPI.Bits.BitsCheermoteModel>> GetBitsCheermotes(NewAPI.Users.UserModel channel) { return await this.RunAsync(this.Connection.NewAPI.Bits.GetCheermotes(channel)); }

        public async Task<IEnumerable<NewAPI.Chat.ChatBadgeSetModel>> GetChannelChatBadges(NewAPI.Users.UserModel channel) { return await this.RunAsync(this.Connection.NewAPI.Chat.GetChannelChatBadges(channel)); }

        public async Task<IEnumerable<NewAPI.Chat.ChatBadgeSetModel>> GetGlobalChatBadges() { return await this.RunAsync(this.Connection.NewAPI.Chat.GetGlobalChatBadges()); }

        public async Task<IEnumerable<NewAPI.ChannelPoints.CustomChannelPointRewardModel>> GetCustomChannelPointRewards(NewAPI.Users.UserModel broadcaster) { return await this.RunAsync(this.Connection.NewAPI.ChannelPoints.GetCustomRewards(broadcaster)); }
    }
}