using MixItUp.Base.Model.Twitch.Ads;
using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Model.Twitch.ChannelPoints;
using MixItUp.Base.Model.Twitch.Channels;
using MixItUp.Base.Model.Twitch.Chat;
using MixItUp.Base.Model.Twitch.Clips;
using MixItUp.Base.Model.Twitch.Games;
using MixItUp.Base.Model.Twitch.Polls;
using MixItUp.Base.Model.Twitch.Predictions;
using MixItUp.Base.Model.Twitch.Streams;
using MixItUp.Base.Model.Twitch.Subscriptions;
using MixItUp.Base.Model.Twitch.Teams;
using MixItUp.Base.Model.Twitch.User;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Services.Twitch.API;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch
{
    [Obsolete]
    public class TwitchPlatformService : StreamingPlatformServiceBase
    {
        public const string ClientID = "50ipfqzuqbv61wujxcm80zyzqwoqp1";

        public static event EventHandler<ClipModel> OnTwitchClipCreated = delegate { };
        public static void TwitchClipCreated(ClipModel clip) { OnTwitchClipCreated(null, clip); }

        public static readonly List<OAuthClientScopeEnum> StreamerScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.bits__read,

            OAuthClientScopeEnum.channel__edit__commercial,

            OAuthClientScopeEnum.channel__manage__ads,
            OAuthClientScopeEnum.channel__manage__broadcast,
            OAuthClientScopeEnum.channel__manage__moderators,
            OAuthClientScopeEnum.channel__manage__polls,
            OAuthClientScopeEnum.channel__manage__predictions,
            OAuthClientScopeEnum.channel__manage__raids,
            OAuthClientScopeEnum.channel__manage__redemptions,
            OAuthClientScopeEnum.channel__manage__vips,

            OAuthClientScopeEnum.channel__moderate,

            OAuthClientScopeEnum.channel__read__ads,
            OAuthClientScopeEnum.channel__read__charity,
            OAuthClientScopeEnum.channel__read__editors,
            OAuthClientScopeEnum.channel__read__goals,
            OAuthClientScopeEnum.channel__read__hype_train,
            OAuthClientScopeEnum.channel__read__polls,
            OAuthClientScopeEnum.channel__read__predictions,
            OAuthClientScopeEnum.channel__read__redemptions,
            OAuthClientScopeEnum.channel__read__subscriptions,

            OAuthClientScopeEnum.clips__edit,

            OAuthClientScopeEnum.chat__edit,
            OAuthClientScopeEnum.chat__read,

            OAuthClientScopeEnum.moderation__read,

            OAuthClientScopeEnum.moderator__read__chatters,
            OAuthClientScopeEnum.moderator__read__chat_settings,
            OAuthClientScopeEnum.moderator__read__followers,

            OAuthClientScopeEnum.moderator__manage__announcements,
            OAuthClientScopeEnum.moderator__manage__banned_users,
            OAuthClientScopeEnum.moderator__manage__chat_messages,
            OAuthClientScopeEnum.moderator__manage__chat_settings,
            OAuthClientScopeEnum.moderator__manage__shoutouts,

            OAuthClientScopeEnum.user__edit,

            OAuthClientScopeEnum.user__manage__blocked_users,
            OAuthClientScopeEnum.user__manage__whispers,

            OAuthClientScopeEnum.user__read__blocked_users,
            OAuthClientScopeEnum.user__read__broadcast,
            OAuthClientScopeEnum.user__read__follows,
            OAuthClientScopeEnum.user__read__subscriptions,

            OAuthClientScopeEnum.whispers__read,
            OAuthClientScopeEnum.whispers__edit,
        };

        public static readonly List<OAuthClientScopeEnum> BotScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.bits__read,

            OAuthClientScopeEnum.channel__moderate,

            OAuthClientScopeEnum.chat__edit,
            OAuthClientScopeEnum.chat__read,

            OAuthClientScopeEnum.moderator__manage__announcements,

            OAuthClientScopeEnum.user__edit,

            OAuthClientScopeEnum.whispers__read,
            OAuthClientScopeEnum.whispers__edit,
        };

        public static async Task<Result<TwitchPlatformService>> Connect(OAuthTokenModel token)
        {
            try
            {
                if (token != null)
                {
                    TwitchConnection connection = await TwitchConnection.ConnectViaOAuthToken(token);
                    if (connection != null)
                    {
                        return new Result<TwitchPlatformService>(new TwitchPlatformService(connection));
                    }
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
                    scopes, forceApprovalPrompt: true);
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
            return new Result<TwitchPlatformService>(MixItUp.Base.Resources.TwitchFailedToConnect);
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

        public override string Name { get { return MixItUp.Base.Resources.TwitchConnection; } }

        public TwitchPlatformService(TwitchConnection connection)
        {
            this.Connection = connection;
        }

        public async Task<UserModel> GetNewAPICurrentUser() { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Users.GetCurrentUser()); }

        public async Task<UserModel> GetNewAPIUserByID(string userID) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Users.GetUserByID(userID)); }

        public async Task<UserModel> GetNewAPIUserByLogin(string login) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Users.GetUserByLogin(login)); }

        public async Task<IEnumerable<ChannelFollowerModel>> GetNewAPIFollowers(UserModel channel, int maxResult = 1)
        {
            return await this.RunAsync(this.Connection.NewAPI.Channels.GetFollowers(channel, maxResults: maxResult));
        }

        public async Task<ChannelFollowerModel> CheckIfFollowsNewAPI(UserModel channel, UserModel userToCheck)
        {
            return await AsyncRunner.RunAsync(this.Connection.NewAPI.Channels.CheckIfFollowing(channel, userToCheck));
        }

        public async Task<IEnumerable<SubscriptionModel>> GetSubscribers(UserModel channel, int maxResult = 1)
        {
            return await this.RunAsync(this.Connection.NewAPI.Subscriptions.GetBroadcasterSubscriptions(channel, maxResult));
        }

        public async Task<GameModel> GetNewAPIGameByID(string id) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Games.GetGameByID(id)); }

        public async Task<IEnumerable<GameModel>> GetNewAPIGamesByName(string name) { return await this.RunAsync(this.Connection.NewAPI.Games.GetGamesByName(name)); }

        public async Task<IEnumerable<GameModel>> GetNewAPIGamesByIDs(IEnumerable<string> ids) { return await this.RunAsync(this.Connection.NewAPI.Games.GetGamesByID(ids)); }

        public async Task<IEnumerable<ChannelContentClassificationLabelModel>> GetContentClassificationLabels(string language = null) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Channels.GetContentClassificationLabels(language)); }

        public async Task<ChannelInformationModel> GetChannelInformation(UserModel channel) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Channels.GetChannelInformation(channel)); }

        public async Task<bool> UpdateChannelInformation(UserModel channel, string title = null, string gameID = null, IEnumerable<string> tags = null, IEnumerable<string> cclIdsToAdd = null, IEnumerable<string> cclIdsToRemove = null) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Channels.UpdateChannelInformation(channel, title: title, gameID: gameID, tags: tags, cclIdsToAdd: cclIdsToAdd, cclIdsToRemove: cclIdsToRemove)); }

        public async Task SendChatAnnouncement(UserModel channel, UserModel sendAsUser, string message, string color) { await AsyncRunner.RunAsync(() => this.Connection.NewAPI.Chat.SendChatAnnouncement(channel.id, sendAsUser.id, new AnnouncementModel { message = message, color = color })); }

        public async Task SendShoutout(UserModel channel, UserModel targetChannel) { await AsyncRunner.RunAsync(() => this.Connection.NewAPI.Chat.SendShoutout(channel.id, targetChannel.id)); }

        public async Task RaidChannel(UserModel channel, UserModel targetChannel) { await AsyncRunner.RunAsync(this.Connection.NewAPI.Chat.RaidChannel(channel.id, targetChannel.id)); }

        public async Task VIPUser(UserModel channel, UserModel user) { await AsyncRunner.RunAsync(this.Connection.NewAPI.Chat.VIPUser(channel.id, user.id)); }

        public async Task UnVIPUser(UserModel channel, UserModel user) { await AsyncRunner.RunAsync(this.Connection.NewAPI.Chat.UnVIPUser(channel.id, user.id)); }

        public async Task ModUser(UserModel channel, UserModel user) { await AsyncRunner.RunAsync(this.Connection.NewAPI.Chat.ModUser(channel.id, user.id)); }

        public async Task UnmodUser(UserModel channel, UserModel user) { await AsyncRunner.RunAsync(this.Connection.NewAPI.Chat.UnmodUser(channel.id, user.id)); }

        public async Task BanUser(UserModel channel, UserModel user, string reason) { await AsyncRunner.RunAsync(this.Connection.NewAPI.Chat.BanUser(channel.id, user.id, reason)); }

        public async Task UnbanUser(UserModel channel, UserModel user) { await AsyncRunner.RunAsync(this.Connection.NewAPI.Chat.UnbanUser(channel.id, user.id)); }

        public async Task TimeoutUser(UserModel channel, UserModel user, int duration, string reason) { await AsyncRunner.RunAsync(this.Connection.NewAPI.Chat.TimeoutUser(channel.id, user.id, duration, reason)); }

        public async Task<IEnumerable<ChatterModel>> GetChatters(UserModel channel) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Chat.GetChatters(channel.id, maxResults: int.MaxValue)); }

        public async Task<ChatSettingsModel> GetChatSettings(UserModel channel) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Chat.GetChatSettings(channel.id)); }

        public async Task UpdateChatSettings(UserModel channel, ChatSettingsModel settings) { await AsyncRunner.RunAsync(this.Connection.NewAPI.Chat.UpdateChatSettings(channel.id, settings)); }

        public async Task<CreatedStreamMarkerModel> CreateStreamMarker(UserModel channel, string description) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Streams.CreateStreamMarker(channel, description)); }

        public async Task<StreamModel> GetStream(UserModel user)
        {
            IEnumerable<StreamModel> results = await this.RunAsync(this.Connection.NewAPI.Streams.GetStreamsByUserIDs(userIDs: new List<string>() { user.id }));
            if (results != null)
            {
                return results.FirstOrDefault();
            }
            return null;
        }

        public async Task<IEnumerable<StreamModel>> GetGameStreams(string gameID, int maxResults) { return await this.RunAsync(this.Connection.NewAPI.Streams.GetStreams(gameIDs: new List<string>() { gameID }, maxResults: maxResults)); }

        public async Task<IEnumerable<StreamModel>> GetLanguageStreams(string language, int maxResults) { return await this.RunAsync(this.Connection.NewAPI.Streams.GetStreams(languages: new List<string>() { language }, maxResults: maxResults)); }

        public async Task<AdResponseModel> RunAd(UserModel channel, int length) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Ads.RunAd(channel, length)); }

        public async Task<ClipCreationModel> CreateClip(UserModel channel, bool delay) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Clips.CreateClip(channel, delay)); }

        public async Task<ClipModel> GetClip(ClipCreationModel clip) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Clips.GetClip(clip)); }

        public async Task<ClipModel> GetClip(string id) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Clips.GetClipByID(id)); }

        public async Task<IEnumerable<ClipModel>> GetClips(UserModel broadcaster, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, bool featured = false, int maxResults = 1) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Clips.GetBroadcasterClips(broadcaster, startedAt: startDate, endedAt: endDate, featured: featured, maxResults: maxResults)); }

        public async Task<BitsLeaderboardModel> GetBitsLeaderboard(BitsLeaderboardPeriodEnum period, int count) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Bits.GetBitsLeaderboard(startedAt: DateTimeOffset.Now, period: period, count: count)); }

        public async Task<IEnumerable<BitsCheermoteModel>> GetBitsCheermotes(UserModel channel) { return await this.RunAsync(this.Connection.NewAPI.Bits.GetCheermotes(channel)); }

        public async Task<IEnumerable<ChatBadgeSetModel>> GetChannelChatBadges(UserModel channel) { return await this.RunAsync(this.Connection.NewAPI.Chat.GetChannelChatBadges(channel)); }

        public async Task<IEnumerable<ChatBadgeSetModel>> GetGlobalChatBadges() { return await this.RunAsync(this.Connection.NewAPI.Chat.GetGlobalChatBadges()); }

        public async Task<Result<CustomChannelPointRewardModel>> CreateCustomChannelPointRewards(UserModel broadcaster, UpdatableCustomChannelPointRewardModel reward)
        {
            try
            {
                return new Result<CustomChannelPointRewardModel>(await this.Connection.NewAPI.ChannelPoints.CreateCustomReward(broadcaster, reward));
            }
            catch (HttpRestRequestException ex)
            {
                return new Result<CustomChannelPointRewardModel>(await ex.Response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return new Result<CustomChannelPointRewardModel>(ex.Message);
            }
        }

        public async Task<IEnumerable<CustomChannelPointRewardModel>> GetCustomChannelPointRewards(UserModel broadcaster, bool managableRewardsOnly = false) { return await this.RunAsync(this.Connection.NewAPI.ChannelPoints.GetCustomRewards(broadcaster, managableRewardsOnly)); }

        public async Task<CustomChannelPointRewardModel> UpdateCustomChannelPointReward(UserModel broadcaster, Guid id, JObject jobj) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.ChannelPoints.UpdateCustomReward(broadcaster, id, jobj)); }

        public async Task<PollModel> CreatePoll(CreatePollModel poll) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Polls.CreatePoll(poll)); }

        public async Task<PollModel> GetPoll(UserModel broadcaster, string pollID) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Polls.GetPoll(broadcaster, pollID)); }

        public async Task<PredictionModel> CreatePrediction(CreatePredictionModel prediction) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Predictions.CreatePrediction(prediction)); }

        public async Task<PredictionModel> GetPrediction(UserModel broadcaster, string predictionID) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Predictions.GetPrediction(broadcaster, predictionID)); }

        public async Task<IEnumerable<ChatEmoteModel>> GetGlobalEmotes() { return await this.RunAsync(this.Connection.NewAPI.Chat.GetGlobalEmotes()); }

        public async Task<IEnumerable<ChatEmoteModel>> GetChannelEmotes(UserModel broadcaster) { return await this.RunAsync(this.Connection.NewAPI.Chat.GetChannelEmotes(broadcaster)); }

        public async Task<IEnumerable<ChatEmoteModel>> GetEmoteSets(IEnumerable<string> emoteSetIDs) { return await this.RunAsync(this.Connection.NewAPI.Chat.GetEmoteSets(emoteSetIDs)); }

        public async Task<SubscriptionModel> GetBroadcasterSubscription(UserModel broadcaster, UserModel user) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Subscriptions.GetBroadcasterSubscription(broadcaster, user)); }

        public async Task<IEnumerable<TeamModel>> GetChannelTeams(UserModel broadcaster) { return await this.RunAsync(this.Connection.NewAPI.Teams.GetChannelTeams(broadcaster)); }

        public async Task<TeamDetailsModel> GetTeam(string id) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Teams.GetTeam(id)); }

        public async Task<IEnumerable<ChannelEditorUserModel>> GetChannelEditors(UserModel broadcaster) { return await this.RunAsync(this.Connection.NewAPI.Channels.GetChannelEditorUsers(broadcaster)); }

        public async Task<long> GetSubscriberCount(UserModel broadcaster) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Subscriptions.GetBroadcasterSubscriptionsCount(broadcaster)); }

        public async Task<long> GetSubscriberPoints(UserModel broadcaster) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Subscriptions.GetBroadcasterSubscriptionPoints(broadcaster)); }

        public async Task<long> GetFollowerCount(UserModel broadcaster) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Channels.GetFollowerCount(broadcaster)); }

        public async Task<IEnumerable<StreamModel>> GetStreams(IEnumerable<string> userIDs) { return await this.RunAsync(this.Connection.NewAPI.Streams.GetStreamsByUserIDs(userIDs)); }

        public async Task<IEnumerable<StreamModel>> GetTopStreams(int maxResults) { return await this.RunAsync(this.Connection.NewAPI.Streams.GetTopStreams(maxResults)); }

        public async Task<IEnumerable<StreamModel>> GetFollowedStreams(UserModel broadcaster, int maxResults) { return await this.RunAsync(this.Connection.NewAPI.Streams.GetFollowedStreams(broadcaster, maxResults)); }

        public async Task<AdScheduleModel> GetAdSchedule(UserModel broadcaster) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Ads.GetAdSchedule(broadcaster)); }

        public async Task<AdSnoozeResponseModel> SnoozeNextAd(UserModel broadcaster) { return await AsyncRunner.RunAsync(this.Connection.NewAPI.Ads.SnoozeNextAd(broadcaster)); }
    }
}
