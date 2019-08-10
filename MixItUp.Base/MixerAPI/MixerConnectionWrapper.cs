using Mixer.Base;
using Mixer.Base.Interactive;
using Mixer.Base.Model.Broadcast;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.Clips;
using Mixer.Base.Model.Costream;
using Mixer.Base.Model.Game;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.Leaderboards;
using Mixer.Base.Model.Patronage;
using Mixer.Base.Model.Skills;
using Mixer.Base.Model.Teams;
using Mixer.Base.Model.TestStreams;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Services.Mixer;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public class MixerConnectionWrapper : MixerRequestWrapperBase
    {
        public MixerConnection Connection { get; private set; }

        public MixerConnectionWrapper(MixerConnection connection)
        {
            this.Connection = connection;
        }

        public void Initialize()
        {
            if (ChannelSession.Settings.DiagnosticLogging)
            {
                this.Connection.Channels.OnRequestSent += RestAPIService_OnRequestSent;
                this.Connection.Channels.OnSuccessResponseReceived += RestAPIService_OnSuccessResponseReceived;
                this.Connection.Channels.OnFailureResponseReceived += RestAPIServices_OnFailureResponseReceived;

                this.Connection.Chats.OnRequestSent += RestAPIService_OnRequestSent;
                this.Connection.Chats.OnSuccessResponseReceived += RestAPIService_OnSuccessResponseReceived;
                this.Connection.Chats.OnFailureResponseReceived += RestAPIServices_OnFailureResponseReceived;

                this.Connection.Costream.OnRequestSent += RestAPIService_OnRequestSent;
                this.Connection.Costream.OnSuccessResponseReceived += RestAPIService_OnSuccessResponseReceived;
                this.Connection.Costream.OnFailureResponseReceived += RestAPIServices_OnFailureResponseReceived;

                this.Connection.GameTypes.OnRequestSent += RestAPIService_OnRequestSent;
                this.Connection.GameTypes.OnSuccessResponseReceived += RestAPIService_OnSuccessResponseReceived;
                this.Connection.GameTypes.OnFailureResponseReceived += RestAPIServices_OnFailureResponseReceived;

                this.Connection.Interactive.OnRequestSent += RestAPIService_OnRequestSent;
                this.Connection.Interactive.OnSuccessResponseReceived += RestAPIService_OnSuccessResponseReceived;
                this.Connection.Interactive.OnFailureResponseReceived += RestAPIServices_OnFailureResponseReceived;

                this.Connection.OAuth.OnRequestSent += RestAPIService_OnRequestSent;
                this.Connection.OAuth.OnSuccessResponseReceived += RestAPIService_OnSuccessResponseReceived;
                this.Connection.OAuth.OnFailureResponseReceived += RestAPIServices_OnFailureResponseReceived;

                this.Connection.Teams.OnRequestSent += RestAPIService_OnRequestSent;
                this.Connection.Teams.OnSuccessResponseReceived += RestAPIService_OnSuccessResponseReceived;
                this.Connection.Teams.OnFailureResponseReceived += RestAPIServices_OnFailureResponseReceived;

                this.Connection.Users.OnRequestSent += RestAPIService_OnRequestSent;
                this.Connection.Users.OnSuccessResponseReceived += RestAPIService_OnSuccessResponseReceived;
                this.Connection.Users.OnFailureResponseReceived += RestAPIServices_OnFailureResponseReceived;
            }
        }

        public async Task<UserModel> GetUser(string username) { return await this.RunAsync(this.Connection.Users.GetUser(username), logNotFoundException: false); }

        public async Task<UserWithChannelModel> GetUser(uint userID) { return await this.RunAsync(this.Connection.Users.GetUser(userID), logNotFoundException: false); }

        public async Task<UserWithChannelModel> GetUser(UserModel user) { return await this.RunAsync(this.Connection.Users.GetUser(user), logNotFoundException: false); }

        public async Task<UserFanProgressionModel> GetUserFanProgression(ChannelModel channel, UserModel user) { return await this.RunAsync(this.Connection.Channels.GetUserFanProgression(channel, user), logNotFoundException: false); }

        public async Task<IEnumerable<TeamMembershipExpandedModel>> GetUserTeams(UserModel user) { return await this.RunAsync(this.Connection.Users.GetTeams(user)); }

        public async Task<UserWithGroupsModel> GetUserInChannel(ChannelModel channel, uint userID) { return await this.RunAsync(this.Connection.Channels.GetUser(channel, userID), logNotFoundException: false); }

        public async Task<IEnumerable<UserWithGroupsModel>> GetUsersWithRoles(ChannelModel channel, MixerRoleEnum role) { return await this.RunAsync(this.Connection.Channels.GetUsersWithRoles(channel, role.ToString(), int.MaxValue), logNotFoundException: false); }

        public async Task<PrivatePopulatedUserModel> GetCurrentUser() { return await this.RunAsync(this.Connection.Users.GetCurrentUser()); }

        public async Task<ChatUserModel> GetChatUser(ChannelModel channel, uint userID) { return await this.RunAsync(this.Connection.Chats.GetUser(channel, userID), logNotFoundException: false); }

        public async Task<IEnumerable<ChatUserModel>> GetChatUsers(ChannelModel channel, uint maxResults = 1) { return await this.RunAsync(this.Connection.Chats.GetUsers(channel, maxResults)); }

        public async Task<ExpandedChannelModel> GetChannel(string channelName) { return await this.RunAsync(this.Connection.Channels.GetChannel(channelName)); }

        public async Task<ExpandedChannelModel> GetChannel(uint channelID) { return await this.RunAsync(this.Connection.Channels.GetChannel(channelID)); }

        public async Task<ExpandedChannelModel> GetChannel(ChannelModel channel) { return await this.RunAsync(this.Connection.Channels.GetChannel(channel.id)); }

        public async Task<IEnumerable<ExpandedChannelModel>> GetChannels(uint maxResults = 1) { return await this.RunAsync(this.Connection.Channels.GetChannels(maxResults)); }

        public async Task<IEnumerable<ExpandedChannelModel>> GetChannelsFromUsers(IEnumerable<uint> userIDs) { return await this.RunAsync(this.Connection.Channels.GetChannelsFromUsers(userIDs)); }

        public async Task<IEnumerable<EmoticonPackModel>> GetEmoticons(ChannelModel channel, UserModel user = null) { return await this.RunAsync(this.Connection.Channels.GetEmoticons(channel, user)); }

        public async Task<bool> Follow(ChannelModel channel, UserModel user) { return await this.RunAsync(this.Connection.Channels.Follow(channel, user)); }

        public async Task<bool> Unfollow(ChannelModel channel, UserModel user = null) { return await this.RunAsync(this.Connection.Channels.Unfollow(channel, user)); }

        public async Task<IEnumerable<ExpandedChannelModel>> GetFeaturedChannels() { return await this.RunAsync(this.Connection.Channels.GetFeaturedChannels()); }

        public async Task UpdateChannel(uint channelID, string name = null, uint? gameTypeID = null, string ageRating = null)
        {
            try
            {
                JObject jobj = new JObject();
                if (!string.IsNullOrEmpty(name))
                {
                    jobj["name"] = name;
                }
                if (gameTypeID.HasValue)
                {
                    jobj["typeId"] = gameTypeID;
                }
                if (!string.IsNullOrEmpty(ageRating))
                {
                    jobj["audience"] = ageRating;
                }
                await ChannelSession.MixerStreamerConnection.Connection.Channels.PatchAsync<ChannelModel>("channels/" + channelID, ChannelSession.MixerStreamerConnection.Connection.Channels.CreateContentFromObject(jobj));
            }
            catch (Exception ex)
            {
                MixItUp.Base.Util.Logger.Log(ex);
            }
        }

        public async Task<ChannelDetailsModel> GetChannelDetails(ChannelModel channel) { return await this.RunAsync(this.Connection.Channels.GetChannelDetails(channel.id)); }

        public async Task<IEnumerable<ChannelAdvancedModel>> GetHosters(ChannelModel channel) { return await this.RunAsync(this.Connection.Channels.GetHosters(channel, uint.MaxValue)); }

        public async Task<GameTypeModel> GetGameType(uint id) { return await this.RunAsync(this.Connection.GameTypes.GetGameType(id)); }

        public async Task<IEnumerable<GameTypeModel>> GetGameTypes(string name, uint maxResults = 1) { return await this.RunAsync(this.Connection.GameTypes.GetGameTypes(name, maxResults)); }

        public async Task<IEnumerable<ChannelModel>> GetChannelsByGameTypes(GameTypeSimpleModel gameType, uint maxResults = 1) { return await this.RunAsync(this.Connection.GameTypes.GetChannelsByGameType(gameType, maxResults)); }

        public async Task<DateTimeOffset?> CheckIfFollows(ChannelModel channel, UserModel user) { return await this.RunAsync(this.Connection.Channels.CheckIfFollows(channel, user)); }

        public async Task<Dictionary<uint, DateTimeOffset?>> CheckIfFollows(ChannelModel channel, IEnumerable<UserModel> users) { return await this.RunAsync(this.Connection.Channels.CheckIfFollows(channel, users)); }

        public async Task<Dictionary<uint, DateTimeOffset?>> CheckIfUsersHaveRole(ChannelModel channel, IEnumerable<UserModel> users, MixerRoleEnum role) { return await this.RunAsync(this.Connection.Channels.CheckIfUsersHaveRole(channel, users, EnumHelper.GetEnumName(role))); }

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

        public async Task<IEnumerable<InteractiveGameListingModel>> GetOwnedInteractiveGames(ChannelModel channel) { return await this.RunAsync(this.Connection.Interactive.GetOwnedInteractiveGames(channel)); }

        public async Task<InteractiveGameModel> GetInteractiveGame(uint gameID) { return await this.RunAsync(this.Connection.Interactive.GetInteractiveGame(gameID)); }

        public async Task<InteractiveGameListingModel> CreateInteractiveGame(ChannelModel channel, UserModel user, string name, InteractiveSceneModel defaultScene) { return await this.RunAsync(InteractiveGameHelper.CreateInteractive2Game(this.Connection, channel, user, name, defaultScene)); }

        public async Task<IEnumerable<InteractiveGameVersionModel>> GetInteractiveGameVersions(InteractiveGameModel game) { return await this.RunAsync(this.Connection.Interactive.GetInteractiveGameVersions(game)); }

        public async Task<InteractiveGameVersionModel> GetInteractiveGameVersion(InteractiveGameVersionModel version) { return await this.RunAsync(this.Connection.Interactive.GetInteractiveGameVersion(version)); }

        public async Task<InteractiveGameVersionModel> GetInteractiveGameVersion(uint versionID) { return await this.RunAsync(this.Connection.Interactive.GetInteractiveGameVersion(versionID)); }

        public async Task UpdateInteractiveGameVersion(InteractiveGameVersionModel version) { await this.RunAsync(this.Connection.Interactive.UpdateInteractiveGameVersion(version)); }

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
                    Util.Logger.Log(string.Format("Rest API Request: {0} - {1}", e.Item1, e.Item2.ReadAsStringAsync().Result));
                }
                catch (Exception) { }
            }
            else
            {
                Util.Logger.Log(string.Format("Rest API Request: {0}", e.Item1));
            }
        }

        private void RestAPIService_OnSuccessResponseReceived(object sender, string e) { Util.Logger.Log(string.Format("Rest API Success Response: {0}", e)); }

        private void RestAPIServices_OnFailureResponseReceived(object sender, HttpRestRequestException e) { Util.Logger.Log(string.Format("Rest API Failure Response: {0}", e.ToString())); }
    }
}
