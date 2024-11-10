using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.YouTube.API
{
    /// <summary>
    /// The APIs for Comments-based services.
    /// </summary>
    public class CommentsService : YouTubeServiceBase
    {
        /// <summary>
        /// Creates an instance of the CommentsService.
        /// </summary>
        /// <param name="connection">The YouTube connection to use</param>
        public CommentsService(YouTubeConnection connection) : base(connection) { }

        /// <summary>
        /// Gets the comment threads for a channel.
        /// </summary>
        /// <param name="channel">The channel to get comment threads for</param>
        /// <param name="maxResults">The maximum results to return</param>
        /// <returns>The list of comment threads</returns>
        public async Task<IEnumerable<CommentThread>> GetCommentThreadsForChannel(Channel channel, int maxResults = 1)
        {
            Validator.ValidateVariable(channel, "channel");
            return await this.GetCommentThreads(channel: channel, relatedTo: false, video: null, maxResults: maxResults);
        }

        /// <summary>
        /// Gets the comment threads related to a channel.
        /// </summary>
        /// <param name="channel">The channel to get comment threads related to</param>
        /// <param name="maxResults">The maximum results to return</param>
        /// <returns>The list of comment threads</returns>
        public async Task<IEnumerable<CommentThread>> GetCommentThreadsRelatedToChannel(Channel channel, int maxResults = 1)
        {
            Validator.ValidateVariable(channel, "channel");
            return await this.GetCommentThreads(channel: channel, relatedTo: true, video: null, maxResults: maxResults);
        }

        /// <summary>
        /// Gets the comment threads for a video.
        /// </summary>
        /// <param name="video">The video to get comment threads for</param>
        /// <param name="maxResults">The maximum results to return</param>
        /// <returns>The list of comment threads</returns>
        public async Task<IEnumerable<CommentThread>> GetCommentThreadsForVideo(Video video, int maxResults = 1)
        {
            Validator.ValidateVariable(video, "video");
            return await this.GetCommentThreads(channel: null, relatedTo: false, video: video, maxResults: maxResults);
        }

        /// <summary>
        /// Gets the comments for the specified comment thread.
        /// </summary>
        /// <param name="commentThread">The comment thread to get comments for</param>
        /// <param name="maxResults">The maximum results to return</param>
        /// <returns>The list of comments</returns>
        public async Task<IEnumerable<Comment>> GetCommentsForCommentThread(CommentThread commentThread, int maxResults = 1)
        {
            Validator.ValidateVariable(commentThread, "commentThread");
            return await this.YouTubeServiceWrapper(async () =>
            {
                List<Comment> results = new List<Comment>();
                string pageToken = null;
                do
                {
                    CommentsResource.ListRequest request = this.connection.GoogleYouTubeService.Comments.List("snippet");
                    request.ParentId = commentThread.Id;
                    request.MaxResults = Math.Min(maxResults, 50);
                    request.PageToken = pageToken;
                    LogRequest(request);

                    CommentListResponse response = await request.ExecuteAsync();
                    LogResponse(request, response);
                    results.AddRange(response.Items);
                    maxResults -= response.Items.Count;
                    pageToken = response.NextPageToken;

                } while (maxResults > 0 && !string.IsNullOrEmpty(pageToken));
                return results;
            });
        }

        internal async Task<IEnumerable<CommentThread>> GetCommentThreads(Channel channel = null, bool relatedTo = false, Video video = null, string id = null, int maxResults = 1)
        {
            return await this.YouTubeServiceWrapper(async () =>
            {
                List<CommentThread> results = new List<CommentThread>();
                string pageToken = null;
                do
                {
                    CommentThreadsResource.ListRequest request = this.connection.GoogleYouTubeService.CommentThreads.List("snippet,replies");
                    if (channel != null)
                    {
                        if (relatedTo)
                        {
                            request.AllThreadsRelatedToChannelId = channel.Id;
                        }
                        else
                        {
                            request.ChannelId = channel.Id;
                        }
                    }
                    else if (video != null)
                    {
                        request.VideoId = video.Id;
                    }
                    else if (!string.IsNullOrEmpty(id))
                    {
                        request.Id = id;
                    }
                    request.MaxResults = Math.Min(maxResults, 50);
                    request.PageToken = pageToken;
                    LogRequest(request);

                    CommentThreadListResponse response = await request.ExecuteAsync();
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
