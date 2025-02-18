using MixItUp.Base.Model;
using MixItUp.Base.Model.Twitch;
using MixItUp.Base.Model.Twitch.Ads;
using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Model.Twitch.ChannelPoints;
using MixItUp.Base.Model.Twitch.Channels;
using MixItUp.Base.Model.Twitch.Chat;
using MixItUp.Base.Model.Twitch.Clips;
using MixItUp.Base.Model.Twitch.EventSub;
using MixItUp.Base.Model.Twitch.Games;
using MixItUp.Base.Model.Twitch.Polls;
using MixItUp.Base.Model.Twitch.Predictions;
using MixItUp.Base.Model.Twitch.Streams;
using MixItUp.Base.Model.Twitch.Subscriptions;
using MixItUp.Base.Model.Twitch.Teams;
using MixItUp.Base.Model.Twitch.User;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace MixItUp.Base.Services.Twitch.New
{
    public class TwitchService : StreamingPlatformServiceBaseNew
    {
        private const string OAuthBaseAddress = "https://id.twitch.tv/oauth2/authorize";

        private const string BaseAddressFormat = "https://api.twitch.tv/helix/";

        public static DateTimeOffset GetTwitchDateTime(string dateTime)
        {
            return DateTimeOffsetExtensions.FromGeneralString(dateTime);
        }

        public override string Name { get { return Resources.Twitch; } }

        public override string ClientID { get { return "50ipfqzuqbv61wujxcm80zyzqwoqp1"; } }
        public override string ClientSecret { get { return ServiceManager.Get<SecretsService>().GetSecret("TwitchSecret"); } }

        public override StreamingPlatformTypeEnum Platform { get { return StreamingPlatformTypeEnum.Twitch; } }

        public override bool IsConnected { get; protected set; }

        public TwitchService(IEnumerable<string> scopes, bool isBotService = false)
            : base(BaseAddressFormat, scopes, isBotService)
        {
            this.HttpClient.AddHeader("Client-Id", this.ClientID);
        }

        public async Task<UserModel> GetNewAPICurrentUser()
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<UserModel> users = await this.GetDataResultAsync<UserModel>("users");
                return users?.FirstOrDefault();
            });
        }

        public async Task<UserModel> GetNewAPIUserByID(string userID)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<UserModel> users = await this.GetDataResultAsync<UserModel>("users?id=" + userID);
                return users?.FirstOrDefault();
            });
        }

        public async Task<UserModel> GetNewAPIUserByLogin(string login)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<UserModel> users = await this.GetDataResultAsync<UserModel>("users?login=" + login);
                return users?.FirstOrDefault();
            });
        }

        public async Task<IEnumerable<ChannelFollowerModel>> GetNewAPIFollowers(UserModel channel, int maxResults = 1)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetPagedDataResultAsync<ChannelFollowerModel>($"channels/followers?broadcaster_id={channel.id}", maxResults);
            });
        }

        public async Task<ChannelFollowerModel> CheckIfFollowsNewAPI(UserModel channel, UserModel userToCheck)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<ChannelFollowerModel> follows = await this.GetPagedDataResultAsync<ChannelFollowerModel>($"channels/followers?broadcaster_id={channel.id}&user_id={userToCheck.id}", maxResults: 1);
                return follows?.FirstOrDefault();
            });
        }

        public async Task<IEnumerable<SubscriptionModel>> GetSubscribers(UserModel channel, int maxResults = 1)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetPagedDataResultAsync<SubscriptionModel>("subscriptions?broadcaster_id=" + channel.id, maxResults);
            });
        }

        public async Task<GameModel> GetNewAPIGameByID(string id)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<GameModel> games = await this.GetDataResultAsync<GameModel>("games?id=" + id);
                return (games != null) ? games.FirstOrDefault() : null;
            });
        }

        public async Task<IEnumerable<GameModel>> GetNewAPIGamesByName(string name)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetDataResultAsync<GameModel>("games?name=" + AdvancedHttpClient.URLEncodeString(name));
            });
        }

        public async Task<IEnumerable<GameModel>> GetNewAPIGamesByIDs(IEnumerable<string> ids)
        { 
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetDataResultAsync<GameModel>("games?id=" + string.Join("&id=", ids));
            });
        }

        public async Task<IEnumerable<ChannelContentClassificationLabelModel>> GetContentClassificationLabels(string locale = null)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetDataResultAsync<ChannelContentClassificationLabelModel>($"content_classification_labels" + (!string.IsNullOrEmpty(locale) ? "?locale=" + locale : string.Empty));
            });
        }

        public async Task<ChannelInformationModel> GetChannelInformation(UserModel channel)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<ChannelInformationModel> results = await this.GetDataResultAsync<ChannelInformationModel>("channels?broadcaster_id=" + channel.id);
                return results?.FirstOrDefault();
            });
        }

        public async Task<Result> UpdateChannelInformation(UserModel channel, string title = null, string gameID = null, IEnumerable<string> tags = null, IEnumerable<string> cclIdsToAdd = null, IEnumerable<string> cclIdsToRemove = null)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                JObject jobj = new JObject();
                if (!string.IsNullOrEmpty(title)) { jobj["title"] = title; }
                if (!string.IsNullOrEmpty(gameID)) { jobj["game_id"] = gameID; }
                if (tags != null && tags.Count() > 0) { jobj["tags"] = JArray.FromObject(tags.ToArray()); }
                if ((cclIdsToAdd != null && cclIdsToAdd.Count() > 0) || (cclIdsToRemove != null && cclIdsToRemove.Count() > 0))
                {
                    JArray ccls = new JArray();

                    if (cclIdsToAdd != null)
                    {
                        foreach (string cclId in cclIdsToAdd)
                        {
                            JObject ccl = new JObject();
                            ccl["id"] = cclId;
                            ccl["is_enabled"] = true;
                            ccls.Add(ccl);
                        }
                    }

                    if (cclIdsToRemove != null)
                    {
                        foreach (string cclId in cclIdsToRemove)
                        {
                            JObject ccl = new JObject();
                            ccl["id"] = cclId;
                            ccl["is_enabled"] = false;
                            ccls.Add(ccl);
                        }
                    }

                    jobj["content_classification_labels"] = ccls;
                }
                HttpResponseMessage response = await this.HttpClient.PatchAsync("channels?broadcaster_id=" + channel.id, AdvancedHttpClient.CreateContentFromObject(jobj));
                if (!response.IsSuccessStatusCode)
                {
                    return new Result(await response.Content.ReadAsStringAsync());
                }

                return new Result();
            });
        }

        public async Task<bool> SetGame(UserModel user, string gameName)
        {
            IEnumerable<GameModel> games = await this.GetNewAPIGamesByName(gameName);
            if (games != null && games.Count() > 0)
            {
                GameModel game = games.FirstOrDefault(g => g.name.ToLower().Equals(gameName));
                if (game == null)
                {
                    game = games.First();
                }

                if (this.IsConnected && game != null)
                {
                    await this.UpdateChannelInformation(user, gameID: game.id);
                    return true;
                }
            }
            return false;
        }

        public async Task<SendChatMessageResponseModel> SendChatMessage(UserModel channel, UserModel sender, string message, string replyMessageID = null)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                JObject jobj = new JObject();
                jobj["broadcaster_id"] = channel.id;
                jobj["sender_id"] = sender.id;
                jobj["message"] = message;
                if (!string.IsNullOrEmpty(replyMessageID))
                {
                    jobj["reply_parent_message_id"] = replyMessageID;
                }

                return await this.HttpClient.PostAsync<SendChatMessageResponseModel>("chat/messages", AdvancedHttpClient.CreateContentFromObject(jobj));
            });
        }

        public async Task DeleteChatMessage(UserModel channel, string messageID)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                await this.HttpClient.DeleteAsync("moderation/chat?broadcaster_id=" + channel.id + "&moderator_id=" + channel.id + "&message_id=" + messageID);
            });
        }

        public async Task ClearChat(UserModel channel)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                await this.HttpClient.DeleteAsync("moderation/chat?broadcaster_id=" + channel.id + "&moderator_id=" + channel.id);
            });
        }

        public async Task SendWhisper(UserModel from, string to_id, string message)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                JObject jobj = new JObject();
                jobj["message"] = message;
                await this.HttpClient.PostAsync("whispers?from_user_id=" + from.id + "&to_user_id=" + to_id, AdvancedHttpClient.CreateContentFromObject(jobj));
            });
        }

        public async Task SendChatAnnouncement(UserModel channel, UserModel sendAsUser, string message, string color)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                await this.HttpClient.PostAsync("chat/announcements?broadcaster_id=" + channel.id + "&moderator_id=" + sendAsUser.id, AdvancedHttpClient.CreateContentFromObject(new AnnouncementModel { message = message, color = color }));
            });
        }

        public async Task SendShoutout(UserModel channel, UserModel targetChannel)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                await this.HttpClient.PostAsync("chat/shoutouts?from_broadcaster_id=" + channel.id + "&to_broadcaster_id=" + targetChannel.id + "&moderator_id=" + channel.id);
            });
        }

        public async Task RaidChannel(UserModel channel, UserModel targetChannel)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                await this.HttpClient.PostAsync("raids?from_broadcaster_id=" + channel.id + "&to_broadcaster_id=" + targetChannel.id);
            });
        }

        public async Task VIPUser(UserModel channel, string user_id)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                await this.HttpClient.PostAsync("channels/vips?broadcaster_id=" + channel.id + "&user_id=" + user_id);
            });
        }

        public async Task UnVIPUser(UserModel channel, string user_id)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                await this.HttpClient.DeleteAsync("channels/vips?broadcaster_id=" + channel.id + "&user_id=" + user_id);
            });
        }

        public async Task ModUser(UserModel channel, string user_id)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                await this.HttpClient.PostAsync("moderation/moderators?broadcaster_id=" + channel.id + "&user_id=" + user_id);
            });
        }

        public async Task UnmodUser(UserModel channel, string user_id)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                await this.HttpClient.DeleteAsync("moderation/moderators?broadcaster_id=" + channel.id + "&user_id=" + user_id);
            });
        }

        public async Task BanUser(UserModel channel, string user_id, string reason) { await this.TimeoutUser(channel, user_id, 0, reason); }

        public async Task UnbanUser(UserModel channel, string user_id) { await this.UntimeoutUser(channel, user_id); }

        public async Task TimeoutUser(UserModel channel, string user_id, int duration, string reason)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                JObject jdata = new JObject();
                JObject jobj = new JObject();
                jobj["user_id"] = user_id;
                jobj["reason"] = reason ?? string.Empty;
                if (duration > 0)
                {
                    jobj["duration"] = duration;
                }
                jdata["data"] = jobj;

                await this.HttpClient.PostAsync("moderation/bans?broadcaster_id=" + channel.id + "&moderator_id=" + channel.id, AdvancedHttpClient.CreateContentFromObject(jdata));
            });
        }

        public async Task UntimeoutUser(UserModel channel, string user_id)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                await this.HttpClient.DeleteAsync("moderation/bans?broadcaster_id=" + channel.id + "&moderator_id=" + channel.id + "&user_id=" + user_id);
            });
        }

        public async Task<IEnumerable<ChatterModel>> GetChatters(UserModel channel)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetPagedDataResultAsync<ChatterModel>("chat/chatters?broadcaster_id=" + channel.id + "&moderator_id=" + channel.id, int.MaxValue);
            });
        }

        public async Task<ChatSettingsModel> GetChatSettings(UserModel channel)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<ChatSettingsModel> settings = await this.GetDataResultAsync<ChatSettingsModel>("chat/settings?broadcaster_id=" + channel.id + "&moderator_id=" + channel.id);
                return settings?.FirstOrDefault();
            });
        }

        public async Task UpdateChatSettings(UserModel channel, ChatSettingsModel settings)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                NewTwitchAPIDataRestResult<ChatSettingsModel> updatedSettings = await this.HttpClient.PatchAsync<NewTwitchAPIDataRestResult<ChatSettingsModel>>("chat/settings?broadcaster_id=" + channel.id + "&moderator_id=" + channel.id,
                    AdvancedHttpClient.CreateContentFromObject(settings));
                return updatedSettings?.data?.FirstOrDefault();
            });
        }

        public async Task<CreatedStreamMarkerModel> CreateStreamMarker(UserModel channel, string description)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                JObject jobj = new JObject();
                jobj["user_id"] = channel.id;
                jobj["description"] = description;
                return await this.HttpClient.PostAsync<CreatedStreamMarkerModel>("streams/markers", AdvancedHttpClient.CreateContentFromObject(jobj));
            });
        }

        public async Task<StreamModel> GetLatestStream(UserModel user)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<StreamModel> streams = await this.GetPagedDataResultAsync<StreamModel>($"streams?user_id={user.id}", 1);
                return streams?.FirstOrDefault();
            });
        }

        public async Task<StreamModel> GetActiveStream(UserModel user)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<StreamModel> streams = await this.GetPagedDataResultAsync<StreamModel>($"streams?user_id={user.id}&type=live", 1);
                return streams?.FirstOrDefault();
            });
        }

        public async Task<IEnumerable<StreamModel>> GetGameStreams(string gameID, int maxResults)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetPagedDataResultAsync<StreamModel>($"streams?game_id={gameID}", maxResults);
            });
        }

        public async Task<IEnumerable<StreamModel>> GetLanguageStreams(string language, int maxResults)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetPagedDataResultAsync<StreamModel>($"streams?language={language}", maxResults);
            });
        }

        public async Task<AdResponseModel> RunAd(UserModel channel, int length)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                JObject body = new JObject();
                body["broadcaster_id"] = channel.id;
                body["length"] = length;

                JObject result = await this.HttpClient.PostAsync<JObject>("channels/commercial", AdvancedHttpClient.CreateContentFromObject(body));
                if (result != null && result.ContainsKey("data"))
                {
                    JArray array = (JArray)result["data"];
                    IEnumerable<AdResponseModel> adResult = array.ToTypedArray<AdResponseModel>();
                    return adResult?.FirstOrDefault();
                }
                return null;
            });
        }

        public async Task<ClipCreationModel> CreateClip(UserModel channel, bool delay)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<ClipCreationModel> clip = await this.PostDataResultAsync<ClipCreationModel>("clips?broadcaster_id=" + channel.id + "&has_delay=" + delay);
                return clip?.FirstOrDefault();
            });
        }

        public async Task<ClipModel> GetClip(ClipCreationModel clip) { return await this.GetClip(clip.id); }

        public async Task<ClipModel> GetClip(string id)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<ClipModel> clips = await this.GetDataResultAsync<ClipModel>("clips?id=" + id);
                return clips?.FirstOrDefault();
            });
        }

        public async Task<IEnumerable<ClipModel>> GetClips(UserModel channel, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, bool featured = false, int maxResults = 1)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                if (startDate != null)
                {
                    parameters.Add("started_at", HttpUtility.UrlEncode(startDate.GetValueOrDefault().ToRFC3339String()));
                }
                if (endDate != null)
                {
                    parameters.Add("ended_at", HttpUtility.UrlEncode(endDate.GetValueOrDefault().ToRFC3339String()));
                }
                if (featured)
                {
                    parameters.Add("is_featured", "true");
                }

                string parameterString = (parameters.Count > 0) ? "&" + string.Join("&", parameters.Select(kvp => kvp.Key + "=" + kvp.Value)) : string.Empty;
                return await this.GetPagedDataResultAsync<ClipModel>("clips?broadcaster_id=" + channel.id + parameterString, maxResults);
            });
        }

        public async Task<long> GetUserLifetimeBits(string user_id)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                JObject jobj = await this.HttpClient.GetJObjectAsync($"bits/leaderboard?user_id={user_id}&count=1&period=all");
                if (jobj != null)
                {
                    IEnumerable<BitsLeaderboardUserModel> bitsUsers = ((JArray)jobj["data"]).ToTypedArray<BitsLeaderboardUserModel>();
                    if (bitsUsers != null && bitsUsers.Count() > 0)
                    {
                        return bitsUsers.First().score;
                    }
                }
                return 0;
            });
        }

        public async Task<BitsLeaderboardModel> GetBitsLeaderboard(BitsLeaderboardPeriodEnum period, int count)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                DateTimeOffset pstDateTimeOffset = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-8));
                if (period == BitsLeaderboardPeriodEnum.Week)
                {
                    pstDateTimeOffset = pstDateTimeOffset.AddDays(-7);
                }
                else if (period == BitsLeaderboardPeriodEnum.Month)
                {
                    pstDateTimeOffset = pstDateTimeOffset.AddMonths(-1);
                }
                else if (period == BitsLeaderboardPeriodEnum.Year)
                {
                    pstDateTimeOffset = pstDateTimeOffset.AddYears(-1);
                }

                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("period", period.ToString().ToLower());
                parameters.Add("started_at", pstDateTimeOffset.ToRFC3339String());
                parameters.Add("count", count.ToString());

                string parameterString = string.Join("&", parameters.Select(kvp => kvp.Key + "=" + kvp.Value));
                JObject jobj = await this.HttpClient.GetJObjectAsync("bits/leaderboard?" + parameterString);
                if (jobj != null)
                {
                    BitsLeaderboardModel result = new BitsLeaderboardModel();
                    result.users = ((JArray)jobj["data"]).ToTypedArray<BitsLeaderboardUserModel>();
                    result.started_at = jobj["date_range"]["started_at"].ToString();
                    result.ended_at = jobj["date_range"]["ended_at"].ToString();
                    return result;
                }
                return null;
            });
        }

        public async Task<IEnumerable<BitsCheermoteModel>> GetBitsCheermotes(UserModel channel)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetDataResultAsync<BitsCheermoteModel>("bits/cheermotes" + ((channel != null) ? "?broadcaster_id=" + channel.id : ""));
            });
        }

        public async Task<IEnumerable<ChatBadgeSetModel>> GetChannelChatBadges(UserModel channel)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetDataResultAsync<ChatBadgeSetModel>(string.Format("chat/badges?broadcaster_id={0}", channel.id));
            });
        }

        public async Task<IEnumerable<ChatBadgeSetModel>> GetGlobalChatBadges()
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetDataResultAsync<ChatBadgeSetModel>("chat/badges/global");
            });
        }

        public async Task<Result<CustomChannelPointRewardModel>> CreateCustomChannelPointRewards(UserModel broadcaster, UpdatableCustomChannelPointRewardModel reward)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                try
                {
                    NewTwitchAPIDataRestResult<CustomChannelPointRewardModel> result = await this.HttpClient.PostAsync<NewTwitchAPIDataRestResult<CustomChannelPointRewardModel>>("channel_points/custom_rewards?broadcaster_id=" + broadcaster.id,
                        AdvancedHttpClient.CreateContentFromObject(reward));
                    return new Result<CustomChannelPointRewardModel>(result?.data?.FirstOrDefault());
                }
                catch (HttpRestRequestException ex)
                {
                    return new Result<CustomChannelPointRewardModel>(await ex.Response.Content.ReadAsStringAsync());
                }
                catch (Exception ex)
                {
                    return new Result<CustomChannelPointRewardModel>(ex.Message);
                }
            });
        }

        public async Task<IEnumerable<CustomChannelPointRewardModel>> GetCustomChannelPointRewards(UserModel broadcaster, bool managableRewardsOnly = false)
        {
            IEnumerable<CustomChannelPointRewardModel> results = await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetPagedDataResultAsync<CustomChannelPointRewardModel>("channel_points/custom_rewards?broadcaster_id=" + broadcaster.id + "&only_manageable_rewards=" + managableRewardsOnly, maxResults: int.MaxValue);
            });

            return results ?? new List<CustomChannelPointRewardModel>();
        }

        public async Task<CustomChannelPointRewardModel> UpdateCustomChannelPointReward(UserModel broadcaster, Guid rewardID, JObject updatableFields)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                NewTwitchAPIDataRestResult<CustomChannelPointRewardModel> result = await this.HttpClient.PatchAsync<NewTwitchAPIDataRestResult<CustomChannelPointRewardModel>>($"channel_points/custom_rewards?broadcaster_id={broadcaster.id}&id={rewardID}",
                    AdvancedHttpClient.CreateContentFromObject(updatableFields));
                return result?.data?.FirstOrDefault();
            });
        }

        public async Task<PollModel> CreatePoll(CreatePollModel poll)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<PollModel> result = await this.PostDataResultAsync<PollModel>("polls", AdvancedHttpClient.CreateContentFromObject(poll));
                return result?.FirstOrDefault();
            });
        }

        public async Task<PollModel> GetPoll(UserModel broadcaster, string pollID)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<PollModel> result = await this.GetDataResultAsync<PollModel>("polls?broadcaster_id=" + broadcaster.id + "&id=" + pollID);
                return result?.FirstOrDefault();
            });
        }

        public async Task<PredictionModel> CreatePrediction(CreatePredictionModel prediction)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<PredictionModel> result = await this.PostDataResultAsync<PredictionModel>("predictions", AdvancedHttpClient.CreateContentFromObject(prediction));
                return result?.FirstOrDefault();
            });
        }

        public async Task<PredictionModel> GetPrediction(UserModel broadcaster, string predictionID)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<PredictionModel> result = await this.GetDataResultAsync<PredictionModel>("predictions?broadcaster_id=" + broadcaster.id + "&id=" + predictionID);
                return result?.FirstOrDefault();
            });
        }

        public async Task<IEnumerable<ChatEmoteModel>> GetGlobalEmotes()
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetDataResultAsync<ChatEmoteModel>("chat/emotes/global");
            });
        }

        public async Task<IEnumerable<ChatEmoteModel>> GetChannelEmotes(UserModel channel) { return await this.GetChannelEmotes(channel.id); }

        public async Task<IEnumerable<ChatEmoteModel>> GetChannelEmotes(string channelID)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetDataResultAsync<ChatEmoteModel>("chat/emotes?broadcaster_id=" + channelID);
            });
        }

        public async Task<IEnumerable<ChatEmoteModel>> GetEmoteSets(IEnumerable<string> emoteSetIDs)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                List<ChatEmoteModel> results = new List<ChatEmoteModel>();
                for (int i = 0; i < emoteSetIDs.Count(); i = i + 10)
                {
                    results.AddRange(await this.GetDataResultAsync<ChatEmoteModel>("chat/emotes/set?" + string.Join("&", emoteSetIDs.Skip(i).Take(10).Select(id => "emote_set_id=" + id))));
                }
                return results;
            });
        }

        public async Task<SubscriptionModel> GetBroadcasterSubscription(UserModel broadcaster, string user_id)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<SubscriptionModel> subscriptions = await this.GetPagedDataResultAsync<SubscriptionModel>($"subscriptions?broadcaster_id={broadcaster.id}&user_id={user_id}");
                return subscriptions?.FirstOrDefault();
            });
        }

        public async Task<IEnumerable<TeamModel>> GetChannelTeams(UserModel broadcaster)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetDataResultAsync<TeamModel>("teams/channel?broadcaster_id=" + broadcaster.id);
            });
        }

        public async Task<TeamDetailsModel> GetTeam(string id)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<TeamDetailsModel> results = await this.GetDataResultAsync<TeamDetailsModel>("teams?id=" + id);
                return results?.FirstOrDefault();
            });
        }

        public async Task<IEnumerable<ChannelEditorUserModel>> GetChannelEditors(UserModel channel)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetDataResultAsync<ChannelEditorUserModel>("channels/editors?broadcaster_id=" + channel.id);
            });
        }

        public async Task<long> GetSubscriberCount(UserModel channel)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetPagedResultTotalCountAsync("subscriptions?broadcaster_id=" + channel.id);
            });
        }

        public async Task<long> GetSubscriberPoints(UserModel broadcaster)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                JObject data = await this.HttpClient.GetJObjectAsync($"subscriptions?broadcaster_id={broadcaster.id}&first=1");
                if (data != null && data.ContainsKey("points"))
                {
                    return (long)data["points"];
                }
                return 0;
            });
        }

        public async Task<long> GetFollowerCount(UserModel channel)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetPagedResultTotalCountAsync($"channels/followers?broadcaster_id={channel.id}");
            });
        }

        public async Task<IEnumerable<StreamModel>> GetStreams(IEnumerable<string> userIDs)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetPagedDataResultAsync<StreamModel>("streams?user_id" + string.Join("&user_id=", userIDs), userIDs.Count());
            });
        }

        public async Task<IEnumerable<StreamModel>> GetTopStreams(int maxResults)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetPagedDataResultAsync<StreamModel>("streams", maxResults);
            });
        }

        public async Task<IEnumerable<StreamModel>> GetFollowedStreams(UserModel channel, int maxResults)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetPagedDataResultAsync<StreamModel>("streams/followed?user_id=" + channel.id, maxResults);
            });
        }

        public async Task<AdScheduleModel> GetAdSchedule(UserModel channel)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<AdScheduleModel> result = await this.GetDataResultAsync<AdScheduleModel>("channels/ads?broadcaster_id=" + channel.id);
                return result?.FirstOrDefault();
            });
        }

        public async Task<AdSnoozeResponseModel> SnoozeNextAd(UserModel channel)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<AdSnoozeResponseModel> result = await this.PostDataResultAsync<AdSnoozeResponseModel>("channels/ads/schedule/snooze?broadcaster_id=" + channel.id);
                return result?.FirstOrDefault();
            });
        }

        public async Task<IEnumerable<EventSubSubscriptionModel>> GetEventSubSubscriptions()
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                return await this.GetPagedDataResultAsync<EventSubSubscriptionModel>("eventsub/subscriptions", 100);
            });
        }

        public async Task<EventSubSubscriptionModel> CreateEventSubSubscription(string type, string transportMethod, IReadOnlyDictionary<string, string> conditions, string secretOrSessionId, string webhookCallback = null, string version = null)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                JObject jobj = new JObject();
                jobj["type"] = type;

                if (string.IsNullOrEmpty(version))
                {
                    jobj["version"] = "1";
                }
                else
                {
                    jobj["version"] = version;
                }

                jobj["condition"] = new JObject();
                foreach (KeyValuePair<string, string> kvp in conditions)
                {
                    jobj["condition"][kvp.Key] = kvp.Value;
                }

                jobj["transport"] = new JObject();
                jobj["transport"]["method"] = transportMethod;
                if (transportMethod == "webhook")
                {
                    jobj["transport"]["callback"] = webhookCallback;
                    jobj["transport"]["secret"] = secretOrSessionId;
                }
                else
                {
                    jobj["transport"]["session_id"] = secretOrSessionId;
                }

                // TODO: Consider getting other top level fields
                //      "total": 1,
                //      "total_cost": 1,
                //      "max_total_cost": 10000,
                //      "limit": 10000
                var subs = await this.PostDataResultAsync<EventSubSubscriptionModel>("eventsub/subscriptions", AdvancedHttpClient.CreateContentFromObject(jobj));
                return subs?.FirstOrDefault();
            });
        }

        public async Task DeleteEventSubSubscription(string subscriptionId)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                await this.HttpClient.DeleteAsync($"eventsub/subscriptions?id={subscriptionId}");
            });
        }

        protected override async Task<string> GetAuthorizationCodeURL(IEnumerable<string> scopes, string state, bool forceApprovalPrompt = false)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "client_id", this.ClientID },
                { "scope", ConvertClientScopesToString(scopes) },
                { "response_type", "code" },
                { "redirect_uri", LocalOAuthHttpListenerServer.REDIRECT_URL },
            };

            if (forceApprovalPrompt)
            {
                parameters.Add("force_verify", "true");
            }

            FormUrlEncodedContent content = new FormUrlEncodedContent(parameters.AsEnumerable());

            return OAuthBaseAddress + "?" + await content.ReadAsStringAsync();
        }

        protected override async Task<OAuthTokenModel> RequestOAuthToken(string authorizationCode, IEnumerable<string> scopes, string state)
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

            OAuthTokenModel token = await this.HttpClient.PostAsync<OAuthTokenModel>("https://id.twitch.tv/oauth2/token?" + await content.ReadAsStringAsync());
            if (token != null)
            {
                token.clientID = ClientID;
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
                { "refresh_token", this.OAuthToken.refreshToken },
                { "grant_type", "refresh_token" },
            };
            FormUrlEncodedContent content = new FormUrlEncodedContent(parameters.AsEnumerable());

            OAuthTokenModel newToken = await this.HttpClient.PostAsync<OAuthTokenModel>("https://id.twitch.tv/oauth2/token?" + await content.ReadAsStringAsync());
            if (newToken != null)
            {
                newToken.clientID = OAuthToken.clientID;
                newToken.ScopeList = OAuthToken.ScopeList;
                OAuthToken = newToken;
            }
        }

        private string ConvertClientScopesToString(IEnumerable<string> scopes)
        {
            return string.Join(" ", scopes);
        }

        public async Task<IEnumerable<T>> GetDataResultAsync<T>(string requestUri)
        {
            NewTwitchAPIDataRestResult<T> result = await this.HttpClient.GetAsync<NewTwitchAPIDataRestResult<T>>(requestUri);
            if (result != null && result.data != null && result.data.Count > 0)
            {
                return result.data;
            }
            return new List<T>();
        }

        /// <summary>
        /// Performs a GET REST request using the provided request URI for New Twitch API-wrapped data to get total count.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <returns>The total count of the response</returns>
        public async Task<long> GetPagedResultTotalCountAsync(string requestUri)
        {
            if (!requestUri.Contains("?"))
            {
                requestUri += "?";
            }
            else
            {
                requestUri += "&";
            }
            requestUri += "first=1";

            JObject data = await this.HttpClient.GetJObjectAsync(requestUri);
            if (data != null && data.ContainsKey("total"))
            {
                return (long)data["total"];
            }
            return 0;
        }

        /// <summary>
        /// Performs a GET REST request using the provided request URI for New Twitch API-wrapped data.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        public async Task<IEnumerable<T>> GetPagedDataResultAsync<T>(string requestUri, int maxResults = 1)
        {
            if (!requestUri.Contains("?"))
            {
                requestUri += "?";
            }
            else
            {
                requestUri += "&";
            }

            Dictionary<string, string> queryParameters = new Dictionary<string, string>();
            queryParameters.Add("first", ((maxResults > 100) ? 100 : maxResults).ToString());

            List<T> results = new List<T>();
            string cursor = null;
            do
            {
                if (!string.IsNullOrEmpty(cursor))
                {
                    queryParameters["after"] = cursor;
                }
                NewTwitchAPIDataRestResult<T> data = await this.HttpClient.GetAsync<NewTwitchAPIDataRestResult<T>>(requestUri + string.Join("&", queryParameters.Select(kvp => kvp.Key + "=" + kvp.Value)));

                cursor = null;
                if (data != null && data.data != null && data.data.Count > 0)
                {
                    results.AddRange(data.data);
                    cursor = data.Cursor;
                }
            }
            while (results.Count < maxResults && !string.IsNullOrEmpty(cursor));

            return results;
        }


        /// <summary>
        ///  Performs a GET REST request using the provided request URI for New Twitch API-wrapped data that has a single data result.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="maxResults">Maximum number of items per page of results</param>
        /// <param name="cursor">Pagination cursor</param>
        /// <returns>A single data node result set object of the response</returns>
        public async Task<NewTwitchAPISingleDataRestResult<T>> GetPagedSingleDataResultAsync<T>(string requestUri, int maxResults, string cursor = null)
        {
            if (!requestUri.Contains("?"))
            {
                requestUri += "?";
            }
            else
            {
                requestUri += "&";
            }

            Dictionary<string, string> queryParameters = new Dictionary<string, string>();
            queryParameters.Add("first", maxResults.ToString());
            if (!string.IsNullOrEmpty(cursor))
            {
                queryParameters["after"] = cursor;
            }
            return await this.HttpClient.GetAsync<NewTwitchAPISingleDataRestResult<T>>(requestUri + string.Join("&", queryParameters.Select(kvp => kvp.Key + "=" + kvp.Value)));
        }

        /// <summary>
        /// Performs a POST REST request using the provided request URI for New Twitch API-wrapped data.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        public async Task<T[]> PostDataResultAsync<T>(string requestUri)
        {
            NewTwitchAPIDataRestResult<T> result = await this.HttpClient.PostAsync<NewTwitchAPIDataRestResult<T>>(requestUri);
            if (result != null && result.data != null && result.data.Count > 0)
            {
                return result.data.ToArray();
            }
            return null;
        }

        /// <summary>
        /// Performs a POST REST request using the provided request URI for New Twitch API-wrapped data.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="content">The post body content</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        public async Task<T[]> PostDataResultAsync<T>(string requestUri, HttpContent content)
        {
            NewTwitchAPIDataRestResult<T> result = await this.HttpClient.PostAsync<NewTwitchAPIDataRestResult<T>>(requestUri, content);
            if (result != null && result.data != null && result.data.Count > 0)
            {
                return result.data.ToArray();
            }
            return null;
        }
    }
}
