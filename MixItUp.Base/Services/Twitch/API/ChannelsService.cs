using MixItUp.Base.Model.Twitch.Channels;
using MixItUp.Base.Model.Twitch.User;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch.API
{
    /// <summary>
    /// The APIs for Channels-based services.
    /// </summary>
    public class ChannelsService : NewTwitchAPIServiceBase
    {
        /// <summary>
        /// Creates an instance of the ChannelsService.
        /// </summary>
        /// <param name="connection">The Twitch connection to use</param>
        public ChannelsService(TwitchConnection connection) : base(connection) { }

        /// <summary>
        /// Gets channel information for the specified user.
        /// </summary>
        /// <param name="user">The user to get channel information for</param>
        /// <returns>The channel information</returns>
        public async Task<ChannelInformationModel> GetChannelInformation(UserModel user)
        {
            Validator.ValidateVariable(user, "user");
            IEnumerable<ChannelInformationModel> results = await this.GetDataResultAsync<ChannelInformationModel>("channels?broadcaster_id=" + user.id);
            return results.FirstOrDefault();
        }

        /// <summary>
        /// Updates channel information for the specified channel information.
        /// </summary>
        /// <param name="channelInformation">The channel to update channel information for</param>
        /// <param name="title">The optional title to update to</param>
        /// <param name="gameID">The optional game ID to update to</param>
        /// <param name="broadcasterLanguage">The optional broadcast language to update to</param>
        /// <param name="tags">The optional tags to update to</param>
        /// <param name="cclIdsToAdd">ID of the Content Classification Labels that must be added to the channel.</param>
        /// <param name="cclIdsToRemove">ID of the Content Classification Labels that must be removed from the channel.</param>
        /// <returns>Whether the update was successful or not</returns>
        public async Task<bool> UpdateChannelInformation(ChannelInformationModel channelInformation, string title = null, string gameID = null, string broadcasterLanguage = null, IEnumerable<string> tags = null, IEnumerable<string> cclIdsToAdd = null, IEnumerable<string> cclIdsToRemove = null)
        {
            Validator.ValidateVariable(channelInformation, "channelInformation");
            return await this.UpdateChannelInformation(channelInformation.broadcaster_id, title, gameID, broadcasterLanguage, tags, cclIdsToAdd, cclIdsToRemove);
        }

        /// <summary>
        /// Updates channel information for the specified user.
        /// </summary>
        /// <param name="channel">The channel to update information for</param>
        /// <param name="title">The optional title to update to</param>
        /// <param name="gameID">The optional game ID to update to</param>
        /// <param name="broadcasterLanguage">The optional broadcast language to update to</param>
        /// <param name="tags">The optional tags to update to</param>
        /// <param name="cclIdsToAdd">ID of the Content Classification Labels that must be added to the channel.</param>
        /// <param name="cclIdsToRemove">ID of the Content Classification Labels that must be removed from the channel.</param>
        /// <returns>Whether the update was successful or not</returns>
        public async Task<bool> UpdateChannelInformation(UserModel channel, string title = null, string gameID = null, string broadcasterLanguage = null, IEnumerable<string> tags = null, IEnumerable<string> cclIdsToAdd = null, IEnumerable<string> cclIdsToRemove = null)
        {
            Validator.ValidateVariable(channel, "channel");
            return await this.UpdateChannelInformation(channel.id, title, gameID, broadcasterLanguage, tags, cclIdsToAdd, cclIdsToRemove);
        }

        private async Task<bool> UpdateChannelInformation(string broadcasterID, string title = null, string gameID = null, string broadcasterLanguage = null, IEnumerable<string> tags = null, IEnumerable<string> cclIdsToAdd = null, IEnumerable<string> cclIdsToRemove = null)
        {
            JObject jobj = new JObject();
            if (!string.IsNullOrEmpty(title)) { jobj["title"] = title; }
            if (!string.IsNullOrEmpty(gameID)) { jobj["game_id"] = gameID; }
            if (!string.IsNullOrEmpty(broadcasterLanguage)) { jobj["broadcaster_language"] = broadcasterLanguage; }
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
            HttpResponseMessage response = await this.PatchAsync("channels?broadcaster_id=" + broadcasterID, AdvancedHttpClient.CreateContentFromObject(jobj));
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Gets the most recent banned events for the specified channel.
        /// </summary>
        /// <param name="channel">The channel to get banned events for</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <returns>The set of banned events</returns>
        public async Task<IEnumerable<ChannelBannedEventModel>> GetChannelBannedEvents(UserModel channel, int maxResults = 1)
        {
            Validator.ValidateVariable(channel, "channel");
            return await this.GetPagedDataResultAsync<ChannelBannedEventModel>("moderation/banned/events?broadcaster_id=" + channel.id, maxResults);
        }

        /// <summary>
        /// Returns all banned and timed-out users in a channel.
        /// </summary>
        /// <param name="channel">The channel to get banned and timed-out users for</param>
        /// <param name="userIDs">If specified, filters banned and timed-out users to those userIDs specified.</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <returns>The set of banned or timed-out users</returns>
        public async Task<IEnumerable<ChannelBannedUserModel>> GetChannelBannedUsers(UserModel channel, IEnumerable<string> userIDs = null, int maxResults = 1)
        {
            Validator.ValidateVariable(channel, "channel");
            List<string> parameters = new List<string>();
            if (userIDs != null)
            {
                foreach (string userID in userIDs)
                {
                    parameters.Add("user_id=" + userID);
                }
            }
            parameters.Add("broadcaster_id=" + channel.id);
            return await this.GetPagedDataResultAsync<ChannelBannedUserModel>("moderation/banned?" + string.Join("&", parameters), maxResults);
        }

        /// <summary>
        /// Gets the most recent moderator events for the specified channel.
        /// </summary>
        /// <param name="channel">The channel to get moderator events for</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <returns>The set of moderator events</returns>
        public async Task<IEnumerable<ChannelModeratorEventModel>> GetChannelModeratorEvents(UserModel channel, int maxResults = 1)
        {
            Validator.ValidateVariable(channel, "channel");
            return await this.GetPagedDataResultAsync<ChannelModeratorEventModel>("moderation/moderators/events?broadcaster_id=" + channel.id, maxResults);
        }

        /// <summary>
        /// Returns all moderator users in a channel.
        /// </summary>
        /// <param name="channel">The channel to get moderators for</param>
        /// <param name="userIDs">If specified, filters moderator users to those userIDs specified.</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <returns>The set of moderator users</returns>
        public async Task<IEnumerable<ChannelModeratorUserModel>> GetChannelModeratorUsers(UserModel channel, IEnumerable<string> userIDs = null, int maxResults = 1)
        {
            Validator.ValidateVariable(channel, "channel");
            List<string> parameters = new List<string>();
            if (userIDs != null)
            {
                foreach (string userID in userIDs)
                {
                    parameters.Add("user_id=" + userID);
                }
            }
            parameters.Add("broadcaster_id=" + channel.id);
            return await this.GetPagedDataResultAsync<ChannelModeratorUserModel>("moderation/moderators?" + string.Join("&", parameters), maxResults);
        }

        /// <summary>
        /// Gets the list of channel editors for the specified channel.
        /// </summary>
        /// <param name="channel">The channel to get channel editors for</param>
        /// <returns>The list of channel editors</returns>
        public async Task<IEnumerable<ChannelEditorUserModel>> GetChannelEditorUsers(UserModel channel)
        {
            Validator.ValidateVariable(channel, "channel");
            return await this.GetDataResultAsync<ChannelEditorUserModel>("channels/editors?broadcaster_id=" + channel.id);
        }

        /// <summary>
        /// Gets the information of the most recent Hype Train of the given channel ID.
        /// </summary>
        /// <param name="channel">The channel to get Hype Train data for</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <returns>The most recent Hype Train</returns>
        public async Task<IEnumerable<ChannelHypeTrainModel>> GetHypeTrainEvents(UserModel channel, int maxResults = 1)
        {
            Validator.ValidateVariable(channel, "channel");
            return await this.GetPagedDataResultAsync<ChannelHypeTrainModel>($"hypetrain/events?broadcaster_id={channel.id}", maxResults);
        }

        /// <summary>
        /// Gets the list of content classification labels.
        /// </summary>
        /// <param name="locale">The locale to use when looking up</param>
        /// <returns>The list of content classification labels</returns>
        public async Task<IEnumerable<ChannelContentClassificationLabelModel>> GetContentClassificationLabels(string locale = null)
        {
            return await this.GetDataResultAsync<ChannelContentClassificationLabelModel>($"content_classification_labels" + (!string.IsNullOrEmpty(locale) ? "?locale=" + locale : string.Empty));
        }

        /// <summary>
        /// Gets the followers for the specified channel.
        /// </summary>
        /// <param name="channel">The channel to get followers for</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <returns>The list of followers</returns>
        public async Task<IEnumerable<ChannelFollowerModel>> GetFollowers(UserModel channel, int maxResults = 1)
        {
            Validator.ValidateVariable(channel, "channel");
            return await this.GetPagedDataResultAsync<ChannelFollowerModel>($"channels/followers?broadcaster_id={channel.id}", maxResults);
        }

        /// <summary>
        /// Gets the follower count for the specified channel.
        /// </summary>
        /// <param name="channel">The channel to get followers for</param>
        /// <returns>The count of followers</returns>
        public async Task<long> GetFollowerCount(UserModel channel)
        {
            Validator.ValidateVariable(channel, "channel");
            return await this.GetPagedResultTotalCountAsync($"channels/followers?broadcaster_id={channel.id}");
        }

        /// <summary>
        /// Checks if the specified user is following the specified channel.
        /// </summary>
        /// <param name="channel">The channel to check follows for</param>
        /// <param name="user">The user to check if they are following</param>
        /// <returns>The follow information for the user if they are following</returns>
        public async Task<ChannelFollowerModel> CheckIfFollowing(UserModel channel, UserModel user)
        {
            Validator.ValidateVariable(channel, "channel");
            IEnumerable<ChannelFollowerModel> follows = await this.GetPagedDataResultAsync<ChannelFollowerModel>($"channels/followers?broadcaster_id={channel.id}&user_id={user.id}", maxResults: 1);
            return (follows != null && follows.Count() > 0) ? follows.First() : null;
        }

        /// <summary>
        /// Gets the channels that the specified user is following.
        /// </summary>
        /// <param name="user">The user to get followed channels for</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <returns>The list of followed channels</returns>
        public async Task<IEnumerable<ChannelFollowedModel>> GetFollowedChannels(UserModel user, int maxResults = 1)
        {
            Validator.ValidateVariable(user, "user");
            return await this.GetPagedDataResultAsync<ChannelFollowedModel>($"channels/followed?user_id={user.id}", maxResults);
        }
    }
}
