using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.YouTube.API
{
    /// <summary>
    /// The APIs for Channel-based services.
    /// </summary>
    public class ChannelsService : YouTubeServiceBase
    {
        /// <summary>
        /// Creates an instance of the ChannelsService.
        /// </summary>
        /// <param name="connection">The YouTube connection to use</param>
        public ChannelsService(YouTubeConnection connection) : base(connection) { }

        /// <summary>
        /// Gets the channel associated with the account.
        /// </summary>
        /// <returns>The channel information</returns>
        public async Task<Channel> GetMyChannel()
        {
            return await this.YouTubeServiceWrapper(async () =>
            {
                ChannelsResource.ListRequest request = this.connection.GoogleYouTubeService.Channels.List("snippet,statistics,contentDetails");
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

        /// <summary>
        /// Gets the channel associated with the specified username.
        /// </summary>
        /// <param name="username">The username to search for</param>
        /// <returns>The channel information</returns>
        public async Task<Channel> GetChannelByUsername(string username)
        {
            Validator.ValidateString(username, "username");
            return await this.YouTubeServiceWrapper(async () =>
            {
                ChannelsResource.ListRequest request = this.connection.GoogleYouTubeService.Channels.List("snippet,statistics");
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

        /// <summary>
        /// Gets the channel associated with the specified ID.
        /// </summary>
        /// <param name="id">The ID to search for</param>
        /// <returns>The channel information</returns>
        public async Task<Channel> GetChannelByID(string id)
        {
            Validator.ValidateString(id, "id");
            IEnumerable<Channel> results = await this.GetChannelsByID(new List<string>() { id });
            return (results != null && results.Count() > 0) ? results.First() : null;
        }

        /// <summary>
        /// Gets the channels associated with the specified IDs.
        /// </summary>
        /// <param name="ids">The IDs to search for</param>
        /// <returns>The channel information</returns>
        public async Task<IEnumerable<Channel>> GetChannelsByID(IEnumerable<string> ids)
        {
            Validator.ValidateList(ids, "ids");
            return await this.YouTubeServiceWrapper(async () =>
            {
                ChannelsResource.ListRequest request = this.connection.GoogleYouTubeService.Channels.List("snippet,statistics,contentDetails");
                request.Id = string.Join(",", ids);
                request.MaxResults = ids.Count();
                LogRequest(request);

                ChannelListResponse response = await request.ExecuteAsync();
                LogResponse(request, response);

                return response.Items;
            });
        }
    }
}
