using MixItUp.Base.Model.Trovo;
using MixItUp.Base.Model.Trovo.Channels;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Trovo.API
{
    /// <summary>
    /// The APIs for channel-based services.
    /// </summary>
    public class ChannelsService : TrovoServiceBase
    {
        private class TopChannelsPageDataResponseModel : PageDataResponseModel
        {
            public List<TopChannelModel> top_channels_lists { get; set; } = new List<TopChannelModel>();

            public override int GetItemCount() { return this.top_channels_lists.Count; }
        }

        private class ChannelSubscriptionsWrapperModel : PageDataResponseModel
        {
            public new int? total { get; set; }

            public List<ChannelSubscriberModel> subscriptions { get; set; }

            public override int GetItemCount() { return this.subscriptions.Count; }
        }

        private class ChannelFollowersModel : PageDataResponseModel
        {
            public List<ChannelFollowerModel> follower { get; set; } = new List<ChannelFollowerModel>();

            public override int GetItemCount() { return this.follower.Count; }
        }

        /// <summary>
        /// Creates an instance of the ChannelService.
        /// </summary>
        /// <param name="connection">The Trovo connection to use</param>
        public ChannelsService(TrovoConnection connection) : base(connection) { }

        /// <summary>
        /// Gets the currently authenticated channel.
        /// </summary>
        /// <returns>The currently authenticated channel</returns>
        public async Task<PrivateChannelModel> GetCurrentChannel()
        {
            return await this.GetAsync<PrivateChannelModel>("channel");
        }

        /// <summary>
        /// Gets the channel matching the specified ID.
        /// </summary>
        /// <param name="id">The channel ID to search for</param>
        /// <returns>The matching channel</returns>
        public async Task<ChannelModel> GetChannelByID(string id)
        {
            Validator.ValidateString(id, "id");

            JObject requestParameters = new JObject();
            requestParameters["channel_id"] = id;

            return await this.PostAsync<ChannelModel>("channels/id", AdvancedHttpClient.CreateContentFromObject(requestParameters));
        }

        /// <summary>
        /// Gets the channel matching the specified username.
        /// </summary>
        /// <param name="username">The channel username to search for</param>
        /// <returns>The matching channel</returns>
        public async Task<ChannelModel> GetChannelByUsername(string username)
        {
            Validator.ValidateString(username, "username");

            JObject requestParameters = new JObject();
            requestParameters["username"] = username;

            return await this.PostAsync<ChannelModel>("channels/id", AdvancedHttpClient.CreateContentFromObject(requestParameters));
        }

        /// <summary>
        /// Gets the list of top channels with an optional category ID to search for.
        /// </summary>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <param name="categoryID">Optional ID of the category to filter channels by</param>
        /// <returns>The list of channel</returns>
        public async Task<IEnumerable<TopChannelModel>> GetTopChannels(int maxResults = 1, string categoryID = null)
        {
            Dictionary<string, object> parameters = null;
            if (!string.IsNullOrEmpty(categoryID))
            {
                parameters = new Dictionary<string, object>();
                parameters["category_id"] = categoryID;
            }

            IEnumerable<TopChannelsPageDataResponseModel> response = await this.PostPagedTokenAsync<TopChannelsPageDataResponseModel>("gettopchannels", maxResults, maxLimit: 100, parameters: parameters);

            List<TopChannelModel> results = new List<TopChannelModel>();
            foreach (TopChannelsPageDataResponseModel r in response)
            {
                if (r != null && r.top_channels_lists != null)
                {
                    results.AddRange(r.top_channels_lists);
                }
            }

            return results;
        }

        /// <summary>
        /// Updates the specified channel.
        /// </summary>
        /// <param name="id">The ID of the channel</param>
        /// <param name="title">The title of the channel</param>
        /// <param name="categoryID">The ID of the category for the channel</param>
        /// <param name="langaugeCode">The language code for the channel</param>
        /// <param name="audience">The viewing audience for the channel</param>
        /// <returns>Whether the update was successful</returns>
        public async Task<bool> UpdateChannel(string id, string title = null, string categoryID = null, string langaugeCode = null, ChannelAudienceTypeEnum? audience = null)
        {
            Validator.ValidateString(id, "id");

            JObject jobj = new JObject();
            jobj["channel_id"] = id;
            if (!string.IsNullOrEmpty(title)) { jobj["live_title"] = title; }
            if (!string.IsNullOrEmpty(categoryID)) { jobj["category_id"] = categoryID; }
            if (!string.IsNullOrEmpty(langaugeCode)) { jobj["language_code"] = langaugeCode; }
            if (audience != null) { jobj["audi_type"] = audience.ToString(); }

            HttpResponseMessage response = await this.PostAsync("channels/update", AdvancedHttpClient.CreateContentFromObject(jobj));
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Gets the subscribers to the specified channel ID
        /// </summary>
        /// <param name="channelID">The ID of the channel</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <returns>The subscriptions of the channel</returns>
        public async Task<IEnumerable<ChannelSubscriberModel>> GetSubscribers(string channelID, int maxResults = 1)
        {
            Validator.ValidateString(channelID, "channelID");
            IEnumerable<ChannelSubscriptionsWrapperModel> response = await this.GetPagedOffsetAsync<ChannelSubscriptionsWrapperModel>($"channels/{channelID}/subscriptions", maxResults, maxLimit: 100);

            List<ChannelSubscriberModel> result = new List<ChannelSubscriberModel>();
            foreach (ChannelSubscriptionsWrapperModel r in response)
            {
                result.AddRange(r.subscriptions);
            }
            return result;
        }

        /// <summary>
        /// Gets the followers to the specified channel ID
        /// </summary>
        /// <param name="channelID">The ID of the channel</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <returns>The followers of the channel</returns>
        public async Task<IEnumerable<ChannelFollowerModel>> GetFollowers(string channelID, int maxResults = 1)
        {
            Validator.ValidateString(channelID, "channelID");
            IEnumerable<ChannelFollowersModel> response = await this.PostPagedCursorAsync<ChannelFollowersModel>($"channels/{channelID}/followers", maxResults, maxLimit: 100);

            List<ChannelFollowerModel> result = new List<ChannelFollowerModel>();
            foreach (ChannelFollowersModel r in response)
            {
                result.AddRange(r.follower);
            }
            return result;
        }
    }
}
