using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.YouTube.API
{
    /// <summary>
    /// The APIs for Playlist-based services.
    /// </summary>
    public class PlaylistsService : YouTubeServiceBase
    {
        /// <summary>
        /// Creates an instance of the PlaylistsService.
        /// </summary>
        /// <param name="connection">The YouTube connection to use</param>
        public PlaylistsService(YouTubeConnection connection) : base(connection) { }

        /// <summary>
        /// Gets the playlists for the currently authenticated account.
        /// </summary>
        /// <param name="maxResults">The maximum results to return</param>
        /// <returns>The list of playlists</returns>
        public async Task<IEnumerable<Playlist>> GetMyPlaylists(int maxResults = 1)
        {
            return await this.GetPlaylists(channel: null, myPlaylists: true, maxResults: maxResults);
        }

        /// <summary>
        /// Gets the playlists for a channel.
        /// </summary>
        /// <param name="channel">The channel to get playlists for</param>
        /// <param name="maxResults">The maximum results to return</param>
        /// <returns>The list of playlists</returns>
        public async Task<IEnumerable<Playlist>> GetPlaylistsForChannel(Channel channel, int maxResults = 1)
        {
            Validator.ValidateVariable(channel, "channel");
            return await this.GetPlaylists(channel: channel, myPlaylists: false, maxResults: maxResults);
        }

        /// <summary>
        /// Gets the items for a playlist.
        /// </summary>
        /// <param name="playlist">The playlist to get items for</param>
        /// <param name="maxResults">The maximum results to return</param>
        /// <returns>The list of playlist items</returns>
        public async Task<IEnumerable<PlaylistItem>> GetPlaylistItems(Playlist playlist, int maxResults = 1)
        {
            Validator.ValidateVariable(playlist, "playlist");
            return await this.GetPlaylistItems(playlist.Id, maxResults);
        }

        /// <summary>
        /// Adds the specified video to the specified playlist
        /// </summary>
        /// <param name="playlist">The playlist to add to</param>
        /// <param name="video">The video to add</param>
        /// <returns>The new playlist item</returns>
        public async Task<PlaylistItem> AddVideoToPlaylist(Playlist playlist, Video video)
        {
            Validator.ValidateVariable(playlist, "playlist");
            Validator.ValidateVariable(video, "video");

            PlaylistItem newPlaylistItem = new PlaylistItem();
            newPlaylistItem.Snippet = new PlaylistItemSnippet();
            newPlaylistItem.Snippet.PlaylistId = playlist.Id;
            newPlaylistItem.Snippet.ResourceId = new ResourceId();
            newPlaylistItem.Snippet.ResourceId.Kind = video.Kind;
            newPlaylistItem.Snippet.ResourceId.VideoId = video.Id;

            PlaylistItemsResource.InsertRequest request = this.connection.GoogleYouTubeService.PlaylistItems.Insert(newPlaylistItem, "snippet");
            LogRequest(request);

            newPlaylistItem = await request.ExecuteAsync();
            LogResponse(request, newPlaylistItem);
            return newPlaylistItem;
        }

        internal async Task<IEnumerable<Playlist>> GetPlaylists(Channel channel = null, bool myPlaylists = false, string id = null, int maxResults = 1)
        {
            return await this.YouTubeServiceWrapper(async () =>
            {
                List<Playlist> results = new List<Playlist>();
                string pageToken = null;
                do
                {
                    PlaylistsResource.ListRequest request = this.connection.GoogleYouTubeService.Playlists.List("snippet,contentDetails");
                    if (channel != null)
                    {
                        request.ChannelId = channel.Id;
                    }
                    else if (myPlaylists)
                    {
                        request.Mine = true;
                    }
                    else if (!string.IsNullOrEmpty(id))
                    {
                        request.Id = id;
                    }
                    request.MaxResults = Math.Min(maxResults, 50);
                    request.PageToken = pageToken;
                    LogRequest(request);

                    PlaylistListResponse response = await request.ExecuteAsync();
                    LogResponse(request, response);
                    results.AddRange(response.Items);
                    maxResults -= response.Items.Count;
                    pageToken = response.NextPageToken;

                } while (maxResults > 0 && !string.IsNullOrEmpty(pageToken));
                return results;
            });
        }

        internal async Task<IEnumerable<PlaylistItem>> GetPlaylistItems(string playlistID, int maxResults = 1)
        {
            return await this.YouTubeServiceWrapper(async () =>
            {
                List<PlaylistItem> results = new List<PlaylistItem>();
                string pageToken = null;
                do
                {
                    PlaylistItemsResource.ListRequest request = this.connection.GoogleYouTubeService.PlaylistItems.List("snippet,contentDetails");
                    request.PlaylistId = playlistID;
                    request.MaxResults = Math.Min(maxResults, 50);
                    request.PageToken = pageToken;
                    LogRequest(request);

                    PlaylistItemListResponse response = await request.ExecuteAsync();
                    LogResponse(request, response);
                    results.AddRange(response.Items);
                    maxResults -= response.Items.Count;
                    pageToken = response.NextPageToken;

                } while (maxResults > 0 && !string.IsNullOrEmpty(pageToken));
                return results;
            });
        }
    }
}
