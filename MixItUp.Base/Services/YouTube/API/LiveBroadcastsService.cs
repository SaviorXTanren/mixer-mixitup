using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.YouTubePartner.v1;
using Google.Apis.YouTubePartner.v1.Data;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Google.Apis.YouTube.v3.LiveBroadcastsResource.ListRequest;

namespace MixItUp.Base.Services.YouTube.API
{
    /// <summary>
    /// The APIs for Live Broadcast-based services.
    /// </summary>
    public class LiveBroadcastsService : YouTubeServiceBase
    {
        /// <summary>
        /// Creates an instance of the LiveBroadcastsService.
        /// </summary>
        /// <param name="connection">The YouTube connection to use</param>
        public LiveBroadcastsService(YouTubeConnection connection) : base(connection) { }

        /// <summary>
        /// Gets the broadcasts associated with the currently authenticated account.
        /// </summary>
        /// <param name="maxResults">The maximum results to return</param>
        /// <returns>The list of live broadcasts</returns>
        public async Task<IEnumerable<LiveBroadcast>> GetMyBroadcasts(int maxResults = 1)
        {
            return await this.YouTubeServiceWrapper(async () =>
            {
                LiveBroadcastsResource.ListRequest request = this.connection.GoogleYouTubeService.LiveBroadcasts.List("snippet,contentDetails,status");
                request.BroadcastType = BroadcastTypeEnum.All;
                request.Mine = true;
                request.MaxResults = maxResults;
                LogRequest(request);

                LiveBroadcastListResponse response = await request.ExecuteAsync();
                LogResponse(request, response);
                if (response.Items.Count > 0)
                {
                    return response.Items;
                }
                return new List<LiveBroadcast>();
            });
        }

        /// <summary>
        /// Gets the broadcast associated with the specified ID.
        /// </summary>
        /// <returns>The live broadcast</returns>
        public async Task<LiveBroadcast> GetBroadcastByID(string id)
        {
            return await this.YouTubeServiceWrapper(async () =>
            {
                LiveBroadcastsResource.ListRequest request = this.connection.GoogleYouTubeService.LiveBroadcasts.List("snippet,contentDetails,status");
                request.BroadcastType = BroadcastTypeEnum.All;
                request.Id = id;
                request.MaxResults = 10;
                LogRequest(request);

                LiveBroadcastListResponse response = await request.ExecuteAsync();
                LogResponse(request, response);
                return response.Items.FirstOrDefault();
            });
        }

        /// <summary>
        /// Gets the currently active broadcast associated with the currently authenticated account.
        /// </summary>
        /// <returns>The current live broadcast</returns>
        public async Task<LiveBroadcast> GetMyActiveBroadcast()
        {
            return await this.YouTubeServiceWrapper(async () =>
            {
                LiveBroadcastsResource.ListRequest request = this.connection.GoogleYouTubeService.LiveBroadcasts.List("snippet,contentDetails,status");
                request.BroadcastType = BroadcastTypeEnum.All;
                request.BroadcastStatus = BroadcastStatusEnum.Active;
                request.MaxResults = 5;
                LogRequest(request);

                LiveBroadcastListResponse response = await request.ExecuteAsync();
                LogResponse(request, response);
                return response.Items.FirstOrDefault(b => string.Equals(b.Status.LifeCycleStatus, "live"));
            });
        }

        /// <summary>
        /// Gets the currently active broadcast associated with the specified channel.
        /// </summary>
        /// <param name="channel">The channel to get the live broadcast for</param>
        /// <returns>The current live broadcast</returns>
        public async Task<LiveBroadcast> GetChannelActiveBroadcast(Channel channel)
        {
            return await this.YouTubeServiceWrapper(async () =>
            {
                IEnumerable<Video> videos = await connection.Videos.GetVideosForChannel(channel, 5);
                if (videos != null && videos.Count() > 0)
                {
                    Video video = videos.FirstOrDefault(v => string.Equals(v.Snippet.LiveBroadcastContent, "live", StringComparison.InvariantCultureIgnoreCase));
                    if (video != null && !string.IsNullOrEmpty(video.LiveStreamingDetails?.ActiveLiveChatId))
                    {
                        LiveBroadcast broadcast = await this.GetBroadcastByID(video.Id);
                        if (broadcast == null)
                        {
                            broadcast = new LiveBroadcast() { Snippet = new LiveBroadcastSnippet() { LiveChatId = video.LiveStreamingDetails.ActiveLiveChatId } };
                        }
                        return broadcast;
                    }
                }
                return null;
            });
        }

        /// <summary>
        /// Updates the specified broadcast.
        /// </summary>
        /// <param name="broadcast">The broadcast to update</param>
        /// <param name="title">The title to set</param>
        /// <param name="description">The description to set</param>
        /// <returns>The updated broadcast</returns>
        public async Task<LiveBroadcast> UpdateBroadcast(LiveBroadcast broadcast, string title = null, string description = null)
        {
            return await this.YouTubeServiceWrapper(async () =>
            {
                LiveBroadcastsResource.UpdateRequest request = this.connection.GoogleYouTubeService.LiveBroadcasts.Update(new LiveBroadcast()
                {
                    Id = broadcast.Id,
                    Snippet = new LiveBroadcastSnippet()
                    {
                        Title = title ?? broadcast.Snippet.Title,
                        Description = description ?? broadcast.Snippet.Description
                    },
                    Status = broadcast.Status,
                    ContentDetails = broadcast.ContentDetails
                }, "snippet");
                LogRequest(request);

                LiveBroadcast response = await request.ExecuteAsync();
                LogResponse(request, response);
                return broadcast;
            });
        }

        /// <summary>
        /// Starts an ad break for the live broadcast.
        /// </summary>
        /// <param name="broadcast">The broadcast of the live broadcast</param>
        /// <param name="duration">The duration of the ad break</param>
        /// <returns>Information about the ad break</returns>
        public async Task<LiveCuepoint> StartAdBreak(LiveBroadcast broadcast, long duration)
        {
            Validator.ValidateVariable(broadcast, "broadcast");
            return await this.YouTubeServiceWrapper(async () =>
            {
                LiveCuepoint liveCuepoint = new LiveCuepoint();
                liveCuepoint.BroadcastId = broadcast.Id;
                liveCuepoint.Settings = new CuepointSettings();
                liveCuepoint.Settings.CueType = "ad";
                liveCuepoint.Settings.DurationSecs = duration;

                LiveCuepointsResource.InsertRequest request = this.connection.GoogleYouTubePartnerService.LiveCuepoints.Insert(liveCuepoint, broadcast.Snippet.ChannelId);
                LogRequest(request);

                liveCuepoint = await request.ExecuteAsync();
                LogResponse(request, liveCuepoint);
                return liveCuepoint;
            });
        }
    }
}
