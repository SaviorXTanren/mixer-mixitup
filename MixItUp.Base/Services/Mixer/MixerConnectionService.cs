using Mixer.Base;
using Mixer.Base.Interactive;
using Mixer.Base.Model.Broadcast;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.Clips;
using Mixer.Base.Model.Costream;
using Mixer.Base.Model.Game;
using Mixer.Base.Model.Leaderboards;
using Mixer.Base.Model.MixPlay;
using Mixer.Base.Model.Patronage;
using Mixer.Base.Model.Skills;
using Mixer.Base.Model.Teams;
using Mixer.Base.Model.TestStreams;
using Mixer.Base.Model.User;
using MixItUp.Base.Services.External;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Mixer
{
    public interface IMixerConnectionService
    {

    }

    public static class MixerModelExtensions
    {
        public static ChatUserModel GetUser(this ChatUserEventModel chatUserEvent)
        {
            return new ChatUserModel() { userId = chatUserEvent.id, userName = chatUserEvent.username, userRoles = chatUserEvent.roles };
        }

        public static ChatUserModel GetUser(this ChatMessageEventModel chatMessageEvent)
        {
            return new ChatUserModel() { userId = chatMessageEvent.user_id, userName = chatMessageEvent.user_name, userRoles = chatMessageEvent.user_roles };
        }

        public static ChatUserModel GetUser(this ChatSkillAttributionEventModel skillAttributionEvent)
        {
            return new ChatUserModel() { userId = skillAttributionEvent.user_id, userName = skillAttributionEvent.user_name, userRoles = skillAttributionEvent.user_roles };
        }
    }

    public class MixerConnectionService : MixerRequestWrapperBase, IMixerConnectionService
    {
        public const string ClientID = "5e3140d0719f5842a09dd2700befbfc100b5a246e35f2690";

        public static readonly List<OAuthClientScopeEnum> StreamerScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.chat__bypass_links,
            OAuthClientScopeEnum.chat__bypass_slowchat,
            OAuthClientScopeEnum.chat__change_ban,
            OAuthClientScopeEnum.chat__change_role,
            OAuthClientScopeEnum.chat__chat,
            OAuthClientScopeEnum.chat__connect,
            OAuthClientScopeEnum.chat__clear_messages,
            OAuthClientScopeEnum.chat__edit_options,
            OAuthClientScopeEnum.chat__giveaway_start,
            OAuthClientScopeEnum.chat__poll_start,
            OAuthClientScopeEnum.chat__poll_vote,
            OAuthClientScopeEnum.chat__purge,
            OAuthClientScopeEnum.chat__remove_message,
            OAuthClientScopeEnum.chat__timeout,
            OAuthClientScopeEnum.chat__view_deleted,
            OAuthClientScopeEnum.chat__whisper,

            OAuthClientScopeEnum.channel__clip__create__self,
            OAuthClientScopeEnum.channel__details__self,
            OAuthClientScopeEnum.channel__follow__self,
            OAuthClientScopeEnum.channel__update__self,
            OAuthClientScopeEnum.channel__analytics__self,

            OAuthClientScopeEnum.interactive__manage__self,
            OAuthClientScopeEnum.interactive__robot__self,

            OAuthClientScopeEnum.user__details__self,
        };

        public static readonly List<OAuthClientScopeEnum> ModeratorScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.chat__bypass_links,
            OAuthClientScopeEnum.chat__bypass_slowchat,
            OAuthClientScopeEnum.chat__change_ban,
            OAuthClientScopeEnum.chat__change_role,
            OAuthClientScopeEnum.chat__chat,
            OAuthClientScopeEnum.chat__connect,
            OAuthClientScopeEnum.chat__clear_messages,
            OAuthClientScopeEnum.chat__edit_options,
            OAuthClientScopeEnum.chat__giveaway_start,
            OAuthClientScopeEnum.chat__poll_start,
            OAuthClientScopeEnum.chat__poll_vote,
            OAuthClientScopeEnum.chat__purge,
            OAuthClientScopeEnum.chat__remove_message,
            OAuthClientScopeEnum.chat__timeout,
            OAuthClientScopeEnum.chat__view_deleted,
            OAuthClientScopeEnum.chat__whisper,

            OAuthClientScopeEnum.channel__follow__self,

            OAuthClientScopeEnum.user__details__self,

            OAuthClientScopeEnum.user__act_as,
        };

        public static readonly List<OAuthClientScopeEnum> BotScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.chat__bypass_links,
            OAuthClientScopeEnum.chat__bypass_slowchat,
            OAuthClientScopeEnum.chat__chat,
            OAuthClientScopeEnum.chat__connect,
            OAuthClientScopeEnum.chat__edit_options,
            OAuthClientScopeEnum.chat__giveaway_start,
            OAuthClientScopeEnum.chat__poll_start,
            OAuthClientScopeEnum.chat__poll_vote,
            OAuthClientScopeEnum.chat__whisper,

            OAuthClientScopeEnum.user__details__self,

            OAuthClientScopeEnum.user__act_as,
        };

        public static async Task<ExternalServiceResult<MixerConnectionService>> Connect(OAuthTokenModel token)
        {
            try
            {
                MixerConnection connection = await MixerConnection.ConnectViaOAuthToken(token);
                if (connection != null)
                {
                    return new ExternalServiceResult<MixerConnectionService>(new MixerConnectionService(connection));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new ExternalServiceResult<MixerConnectionService>(ex);
            }
            return new ExternalServiceResult<MixerConnectionService>("Mixer user OAuth token could not be used");
        }

        public static async Task<ExternalServiceResult<MixerConnectionService>> ConnectUser(bool isStreamer)
        {
            return await MixerConnectionService.Connect(isStreamer ? MixerConnectionService.StreamerScopes : MixerConnectionService.ModeratorScopes);
        }

        public static async Task<ExternalServiceResult<MixerConnectionService>> ConnectBot()
        {
            return await MixerConnectionService.Connect(MixerConnectionService.BotScopes);
        }

        public static async Task<ExternalServiceResult<MixerConnectionService>> Connect(IEnumerable<OAuthClientScopeEnum> scopes)
        {
            try
            {
                MixerConnection connection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(MixerConnectionService.ClientID, scopes, false, successResponse: OAuthServiceBase.LoginRedirectPageHTML);
                if (connection != null)
                {
                    return new ExternalServiceResult<MixerConnectionService>(new MixerConnectionService(connection));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new ExternalServiceResult<MixerConnectionService>(ex);
            }
            return new ExternalServiceResult<MixerConnectionService>("Failed to connect to establish connection to Mixer");
        }

        public MixerConnection Connection { get; private set; }

        public MixerConnectionService(MixerConnection connection)
        {
            this.Connection = connection;
        }

        public async Task<UserModel> GetUser(string username) { return await this.RunAsync(this.Connection.Users.GetUser(username), logNotFoundException: false); }

        public async Task<UserWithChannelModel> GetUser(uint userID) { return await this.RunAsync(this.Connection.Users.GetUser(userID), logNotFoundException: false); }

        public async Task<UserWithChannelModel> GetUser(UserModel user) { return await this.RunAsync(this.Connection.Users.GetUser(user), logNotFoundException: false); }

        public async Task<UserFanProgressionModel> GetUserFanProgression(ChannelModel channel, UserModel user) { return await this.RunAsync(this.Connection.Channels.GetUserFanProgression(channel, user), logNotFoundException: false); }

        public async Task<IEnumerable<TeamMembershipExpandedModel>> GetUserTeams(UserModel user) { return await this.RunAsync(this.Connection.Users.GetTeams(user)); }

        public async Task<UserWithGroupsModel> GetUserInChannel(ChannelModel channel, uint userID) { return await this.RunAsync(this.Connection.Channels.GetUser(channel, userID), logNotFoundException: false); }

        public async Task<IEnumerable<UserWithGroupsModel>> GetUsersWithRoles(ChannelModel channel, MixerRoleEnum role) { return await this.RunAsync(this.Connection.Channels.GetUsersWithRoles(channel, role.ToString(), int.MaxValue), logNotFoundException: false); }

        public async Task GetUsersWithRoles(ChannelModel channel, MixerRoleEnum role, Func<IEnumerable<UserWithGroupsModel>, Task> processor) { await this.RunAsync(this.Connection.Channels.GetUsersWithRoles(channel, role.ToString(), processor, int.MaxValue), logNotFoundException: false); }

        public async Task<PrivatePopulatedUserModel> GetCurrentUser() { return await this.RunAsync(this.Connection.Users.GetCurrentUser()); }

        public async Task<ChatUserModel> GetChatUser(ChannelModel channel, uint userID) { return await this.RunAsync(this.Connection.Chats.GetUser(channel, userID), logNotFoundException: false); }

        public async Task GetChatUsers(ChannelModel channel, Func<IEnumerable<ChatUserModel>, Task> processor, uint maxResults = 1) { await this.RunAsync(this.Connection.Chats.GetUsers(channel, processor, maxResults)); }

        public async Task<ExpandedChannelModel> GetChannel(string channelName) { return await this.RunAsync(this.Connection.Channels.GetChannel(channelName)); }

        public async Task<ExpandedChannelModel> GetChannel(uint channelID) { return await this.RunAsync(this.Connection.Channels.GetChannel(channelID)); }

        public async Task<ExpandedChannelModel> GetChannel(ChannelModel channel) { return await this.RunAsync(this.Connection.Channels.GetChannel(channel.id)); }

        public async Task<IEnumerable<EmoticonPackModel>> GetEmoticons(ChannelModel channel, UserModel user = null) { return await this.RunAsync(this.Connection.Channels.GetEmoticons(channel, user)); }

        public async Task<bool> Follow(ChannelModel channel, UserModel user) { return await this.RunAsync(this.Connection.Channels.Follow(channel, user)); }

        public async Task<bool> Unfollow(ChannelModel channel, UserModel user = null) { return await this.RunAsync(this.Connection.Channels.Unfollow(channel, user)); }

        public async Task<IEnumerable<ExpandedChannelModel>> GetFeaturedChannels() { return await this.RunAsync(this.Connection.Channels.GetFeaturedChannels()); }

        public async Task UpdateChannel(uint channelID, string name = null, uint? gameTypeID = null, string ageRating = null) { await this.RunAsync(this.Connection.Channels.UpdateChannel(channelID, name, gameTypeID, ageRating)); }

        public async Task<ChannelDetailsModel> GetChannelDetails(ChannelModel channel) { return await this.RunAsync(this.Connection.Channels.GetChannelDetails(channel.id)); }

        public async Task<IEnumerable<ChannelAdvancedModel>> GetHosters(ChannelModel channel) { return await this.RunAsync(this.Connection.Channels.GetHosters(channel, uint.MaxValue)); }

        public async Task<GameTypeModel> GetGameType(uint id) { return await this.RunAsync(this.Connection.GameTypes.GetGameType(id)); }

        public async Task<IEnumerable<GameTypeModel>> GetGameTypes(string name, uint maxResults = 1) { return await this.RunAsync(this.Connection.GameTypes.GetGameTypes(name, maxResults)); }

        public async Task<IEnumerable<ChannelModel>> GetChannelsByGameTypes(GameTypeSimpleModel gameType, uint maxResults = 1) { return await this.RunAsync(this.Connection.GameTypes.GetChannelsByGameType(gameType, maxResults)); }

        public async Task<DateTimeOffset?> CheckIfFollows(ChannelModel channel, UserModel user) { return await this.RunAsync(this.Connection.Channels.CheckIfFollows(channel, user)); }

        public async Task<CostreamModel> GetCurrentCostream() { return await this.RunAsync(this.Connection.Costream.GetCurrentCostream()); }

        public async Task<BroadcastModel> GetCurrentBroadcast(ChannelModel channel) { return await this.RunAsync(this.Connection.Channels.GetCurrentBroadcast(channel)); }

        public async Task<IEnumerable<StreamSessionsAnalyticModel>> GetStreamSessions(ChannelModel channel, DateTimeOffset startTime) { return await this.RunAsync(this.Connection.Channels.GetStreamSessions(channel, startTime)); }

        public async Task<IEnumerable<SparksLeaderboardModel>> GetWeeklySparksLeaderboard(ChannelModel channel, int amount = 10) { return await this.RunAsync(this.Connection.Leaderboards.GetWeeklySparksLeaderboard(channel, amount)); }

        public async Task<IEnumerable<SparksLeaderboardModel>> GetMonthlySparksLeaderboard(ChannelModel channel, int amount = 10) { return await this.RunAsync(this.Connection.Leaderboards.GetMonthlySparksLeaderboard(channel, amount)); }

        public async Task<IEnumerable<SparksLeaderboardModel>> GetYearlySparksLeaderboard(ChannelModel channel, int amount = 10) { return await this.RunAsync(this.Connection.Leaderboards.GetYearlySparksLeaderboard(channel, amount)); }

        public async Task<IEnumerable<SparksLeaderboardModel>> GetAllTimeSparksLeaderboard(ChannelModel channel, int amount = 10) { return await this.RunAsync(this.Connection.Leaderboards.GetAllTimeSparksLeaderboard(channel, amount)); }

        public async Task<IEnumerable<EmbersLeaderboardModel>> GetWeeklyEmbersLeaderboard(ChannelModel channel, int amount = 10) { return await this.RunAsync(this.Connection.Leaderboards.GetWeeklyEmbersLeaderboard(channel, amount)); }

        public async Task<IEnumerable<EmbersLeaderboardModel>> GetMonthlyEmbersLeaderboard(ChannelModel channel, int amount = 10) { return await this.RunAsync(this.Connection.Leaderboards.GetMonthlyEmbersLeaderboard(channel, amount)); }

        public async Task<IEnumerable<EmbersLeaderboardModel>> GetYearlyEmbersLeaderboard(ChannelModel channel, int amount = 10) { return await this.RunAsync(this.Connection.Leaderboards.GetYearlyEmbersLeaderboard(channel, amount)); }

        public async Task<IEnumerable<EmbersLeaderboardModel>> GetAllTimeEmbersLeaderboard(ChannelModel channel, int amount = 10) { return await this.RunAsync(this.Connection.Leaderboards.GetAllTimeEmbersLeaderboard(channel, amount)); }

        public async Task AddUserRoles(ChannelModel channel, UserModel user, IEnumerable<MixerRoleEnum> roles) { await this.RunAsync(this.Connection.Channels.UpdateUserRoles(channel, user, roles.Select(r => EnumHelper.GetEnumName(r)), null)); }

        public async Task RemoveUserRoles(ChannelModel channel, UserModel user, IEnumerable<MixerRoleEnum> roles) { await this.RunAsync(this.Connection.Channels.UpdateUserRoles(channel, user, null, roles.Select(r => EnumHelper.GetEnumName(r)))); }

        public async Task<IEnumerable<MixPlayGameListingModel>> GetOwnedMixPlayGames(ChannelModel channel) { return await this.RunAsync(this.Connection.MixPlay.GetOwnedMixPlayGames(channel)); }

        public async Task<MixPlayGameModel> GetMixPlayGame(uint gameID) { return await this.RunAsync(this.Connection.MixPlay.GetMixPlayGame(gameID)); }

        public async Task<MixPlayGameListingModel> CreateMixPlayGame(ChannelModel channel, UserModel user, string name, MixPlaySceneModel defaultScene) { return await this.RunAsync(MixPlayGameHelper.CreateMixPlay2Game(this.Connection, channel, user, name, defaultScene)); }

        public async Task<IEnumerable<MixPlayGameVersionModel>> GetMixPlayGameVersions(MixPlayGameModel game) { return await this.RunAsync(this.Connection.MixPlay.GetMixPlayGameVersions(game)); }

        public async Task<MixPlayGameVersionModel> GetMixPlayGameVersion(MixPlayGameVersionModel version) { return await this.RunAsync(this.Connection.MixPlay.GetMixPlayGameVersion(version)); }

        public async Task<MixPlayGameVersionModel> GetMixPlayGameVersion(uint versionID) { return await this.RunAsync(this.Connection.MixPlay.GetMixPlayGameVersion(versionID)); }

        public async Task UpdateMixPlayGameVersion(MixPlayGameVersionModel version) { await this.RunAsync(this.Connection.MixPlay.UpdateMixPlayGameVersion(version)); }

        public async Task<TeamModel> GetTeam(uint id) { return await this.RunAsync(this.Connection.Teams.GetTeam(id)); }

        public async Task<TeamModel> GetTeam(string name) { return await this.RunAsync(this.Connection.Teams.GetTeam(name)); }

        public async Task<IEnumerable<UserWithChannelModel>> GetTeamUsers(TeamModel team, uint maxResults = 1) { return await this.RunAsync(this.Connection.Teams.GetTeamUsers(team, maxResults)); }

        public async Task<ChannelModel> SetHostChannel(ChannelModel hosterChannel, ChannelModel channelToHost) { return await this.RunAsync(this.Connection.Channels.SetHostChannel(hosterChannel, channelToHost)); }

        public async Task<BroadcastModel> GetCurrentBroadcast() { return await this.RunAsync(this.Connection.Broadcasts.GetCurrentBroadcast()); }

        public async Task<bool> CanClipBeMade(BroadcastModel broadcast) { return await this.RunAsync(this.Connection.Clips.CanClipBeMade(broadcast)); }

        public async Task<ClipModel> CreateClip(ClipRequestModel clipRequest) { return await this.RunAsync(this.Connection.Clips.CreateClip(clipRequest)); }

        public async Task<ClipModel> GetClip(string shareableID) { return await this.RunAsync(this.Connection.Clips.GetClip(shareableID)); }

        public async Task<IEnumerable<ClipModel>> GetChannelClips(ChannelModel channel) { return await this.RunAsync(this.Connection.Clips.GetChannelClips(channel)); }

        public async Task<TestStreamSettingsModel> GetTestStreamSettings(ChannelModel channel) { return await this.RunAsync(this.Connection.TestStreams.GetSettings(channel)); }

        public async Task<PatronageStatusModel> GetPatronageStatus(ChannelModel channel) { return await this.RunAsync(this.Connection.Patronage.GetPatronageStatus(channel)); }

        public async Task<PatronagePeriodModel> GetPatronagePeriod(PatronageStatusModel patronageStatus) { return await this.RunAsync(this.Connection.Patronage.GetPatronagePeriod(patronageStatus.patronagePeriodId)); }

        public async Task<PatronageMilestoneModel> GetCurrentPatronageMilestone()
        {
            PatronageStatusModel patronageStatus = await this.GetPatronageStatus(ChannelSession.MixerChannel);
            if (patronageStatus != null)
            {
                PatronagePeriodModel patronagePeriod = await this.GetPatronagePeriod(patronageStatus);
                if (patronagePeriod != null)
                {
                    IEnumerable<PatronageMilestoneModel> patronageMilestones = patronagePeriod.milestoneGroups.SelectMany(mg => mg.milestones);
                    return patronageMilestones.FirstOrDefault(m => m.id == patronageStatus.currentMilestoneId);
                }
            }
            return null;
        }

        public async Task<SkillCatalogModel> GetSkillCatalog(ChannelModel channel) { return await this.RunAsync(this.Connection.Skills.GetSkillCatalog(channel)); }

        private void RestAPIService_OnRequestSent(object sender, Tuple<string, HttpContent> e)
        {
            if (e.Item2 != null)
            {
                try
                {
                    Logger.Log(string.Format("Rest API Request: {0} - {1}", e.Item1, e.Item2.ReadAsStringAsync().Result));
                }
                catch (Exception) { }
            }
            else
            {
                Logger.Log(string.Format("Rest API Request: {0}", e.Item1));
            }
        }

        private void RestAPIService_OnSuccessResponseReceived(object sender, string e) { Logger.Log(string.Format("Rest API Success Response: {0}", e)); }

        private void RestAPIServices_OnFailureResponseReceived(object sender, HttpRestRequestException e) { Logger.Log(string.Format("Rest API Failure Response: {0}", e.ToString())); }
    }
}