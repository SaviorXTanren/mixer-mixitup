using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model.YouTube;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.YouTube.API
{
    /// <summary>
    /// The APIs for Live Chat-based services.
    /// </summary>
    public class LiveChatService : YouTubeServiceBase
    {
        /// <summary>
        /// Creates an instance of the LiveChatService.
        /// </summary>
        /// <param name="connection">The YouTube connection to use</param>
        public LiveChatService(YouTubeConnection connection) : base(connection) { }

        /// <summary>
        /// Gets the messages for a live chat.
        /// </summary>
        /// <param name="broadcast">The broadcast of the live chat</param>
        /// <param name="nextResultsToken">The token for querying the next set of results from a previous query</param>
        /// <param name="maxResults">The maximum results to return</param>
        /// <returns>The list of chat messages</returns>
        public async Task<LiveChatMessagesResultModel> GetMessages(LiveBroadcast broadcast, string nextResultsToken = null, int maxResults = 0)
        {
            Validator.ValidateVariable(broadcast, "broadcast");
            return await this.YouTubeServiceWrapper(async () =>
            {
                LiveChatMessagesResource.ListRequest request = this.connection.GoogleYouTubeService.LiveChatMessages.List(broadcast.Snippet.LiveChatId, "id,snippet,authorDetails");

                if (maxResults > 0)
                {
                    request.MaxResults = maxResults;
                }

                if (!string.IsNullOrEmpty(nextResultsToken))
                {
                    request.PageToken = nextResultsToken;
                }

                LogRequest(request);

                LiveChatMessageListResponse response = await request.ExecuteAsync();
                LogResponse(request, response);

                return new LiveChatMessagesResultModel(response);
            });
        }

        /// <summary>
        /// Sends a message to the live chat.
        /// </summary>
        /// <param name="broadcast">The broadcast of the live chat</param>
        /// <param name="message">The message to send</param>
        /// <returns>The resulting message</returns>
        public async Task<LiveChatMessage> SendMessage(LiveBroadcast broadcast, string message)
        {
            Validator.ValidateVariable(broadcast, "broadcast");
            Validator.ValidateString(message, "message");
            return await this.YouTubeServiceWrapper(async () =>
            {
                LiveChatMessage newMessage = new LiveChatMessage();
                newMessage.Snippet = new LiveChatMessageSnippet();
                newMessage.Snippet.LiveChatId = broadcast.Snippet.LiveChatId;
                newMessage.Snippet.Type = "textMessageEvent";
                newMessage.Snippet.TextMessageDetails = new LiveChatTextMessageDetails();
                newMessage.Snippet.TextMessageDetails.MessageText = message;

                LiveChatMessagesResource.InsertRequest request = this.connection.GoogleYouTubeService.LiveChatMessages.Insert(newMessage, "snippet");
                LogRequest(request);

                LiveChatMessage liveChatMessage = await request.ExecuteAsync();
                LogResponse(request, liveChatMessage);
                return liveChatMessage;
            });
        }

        /// <summary>
        /// Deletes the specified message from the live chat.
        /// </summary>
        /// <param name="message">The message to delete</param>
        /// <returns>An awaitable Task</returns>
        public async Task DeleteMessage(LiveChatMessage message)
        {
            Validator.ValidateVariable(message, "message");
            await this.YouTubeServiceWrapper<object>(async () =>
            {
                LiveChatMessagesResource.DeleteRequest request = this.connection.GoogleYouTubeService.LiveChatMessages.Delete(message.Id);
                LogRequest(request);

                string response = await request.ExecuteAsync();
                LogResponse(request, response);
                return null;
            });
        }

        /// <summary>
        /// Gets the most recent super chat events for a live chat.
        /// </summary>
        /// <param name="broadcast">The broadcast of the live chat</param>
        /// <param name="maxResults">The maximum results to return</param>
        /// <returns>The list of super chat events</returns>
        public async Task<IEnumerable<SuperChatEvent>> GetRecentSuperChatEvents(LiveBroadcast broadcast, int maxResults = 1)
        {
            Validator.ValidateVariable(broadcast, "broadcast");
            return await this.YouTubeServiceWrapper(async () =>
            {
                List<SuperChatEvent> results = new List<SuperChatEvent>();
                string pageToken = null;
                do
                {
                    SuperChatEventsResource.ListRequest request = this.connection.GoogleYouTubeService.SuperChatEvents.List("id,snippet");
                    request.MaxResults = Math.Min(maxResults, 50);
                    request.PageToken = pageToken;
                    LogRequest(request);

                    SuperChatEventListResponse response = await request.ExecuteAsync();
                    LogResponse(request, response);
                    results.AddRange(response.Items);
                    maxResults -= response.Items.Count;
                    pageToken = response.NextPageToken;

                } while (maxResults > 0 && !string.IsNullOrEmpty(pageToken));
                return results;
            });
        }

        /// <summary>
        /// Gets the list of channel memberships.
        /// </summary>
        /// <param name="maxResults">The maximum results to return</param>
        /// <returns>The list of channel memberships</returns>
        public async Task<IEnumerable<Member>> GetChannelMemberships(int maxResults = 1)
        {
            return await this.YouTubeServiceWrapper(async () =>
            {
                List<Member> results = new List<Member>();
                string pageToken = null;
                do
                {
                    MembersResource.ListRequest request = this.connection.GoogleYouTubeService.Members.List("id,snippet");
                    request.MaxResults = Math.Min(maxResults, 50);
                    request.PageToken = pageToken;
                    LogRequest(request);

                    MemberListResponse response = await request.ExecuteAsync();
                    LogResponse(request, response);
                    results.AddRange(response.Items);
                    maxResults -= response.Items.Count;
                    pageToken = response.NextPageToken;

                } while (maxResults > 0 && !string.IsNullOrEmpty(pageToken));
                return results;
            });
        }

        /// <summary>
        /// Gets the list of moderators.
        /// </summary>
        /// <param name="broadcast">The broadcast of the live chat</param>
        /// <param name="maxResults">The maximum results to return</param>
        /// <returns>The list of moderators</returns>
        public async Task<IEnumerable<LiveChatModerator>> GetModerators(LiveBroadcast broadcast, int maxResults = 1)
        {
            Validator.ValidateVariable(broadcast, "broadcast");
            return await this.YouTubeServiceWrapper(async () =>
            {
                List<LiveChatModerator> results = new List<LiveChatModerator>();
                string pageToken = null;
                do
                {
                    LiveChatModeratorsResource.ListRequest request = this.connection.GoogleYouTubeService.LiveChatModerators.List(broadcast.Snippet.LiveChatId, "id,snippet");
                    request.MaxResults = Math.Min(maxResults, 50);
                    request.PageToken = pageToken;
                    LogRequest(request);

                    LiveChatModeratorListResponse response = await request.ExecuteAsync();
                    LogResponse(request, response);
                    results.AddRange(response.Items);
                    maxResults -= response.Items.Count;
                    pageToken = response.NextPageToken;

                } while (maxResults > 0 && !string.IsNullOrEmpty(pageToken));
                return results;
            });
        }

        /// <summary>
        /// Makes the specified user a moderator.
        /// </summary>
        /// <param name="broadcast">The broadcast of the live chat</param>
        /// <param name="user">The user to mod</param>
        /// <returns>Information about the modded user</returns>
        public async Task<LiveChatModerator> ModUser(LiveBroadcast broadcast, Channel user)
        {
            Validator.ValidateVariable(broadcast, "broadcast");
            Validator.ValidateVariable(user, "user");
            return await this.YouTubeServiceWrapper(async () =>
            {
                LiveChatModerator moderator = new LiveChatModerator();
                moderator.Snippet = new LiveChatModeratorSnippet();
                moderator.Snippet.LiveChatId = broadcast.Snippet.LiveChatId;
                moderator.Snippet.ModeratorDetails = new ChannelProfileDetails();
                moderator.Snippet.ModeratorDetails.ChannelId = user.Id;

                LiveChatModeratorsResource.InsertRequest request = this.connection.GoogleYouTubeService.LiveChatModerators.Insert(moderator, "snippet");
                LogRequest(request);

                moderator = await request.ExecuteAsync();
                LogResponse(request, moderator);
                return moderator;
            });
        }

        /// <summary>
        /// Removes moderator privileges from the specified user.
        /// </summary>
        /// <param name="moderator">The user to unmod</param>
        /// <returns>An awaitable Task</returns>
        public async Task UnmodUser(LiveChatModerator moderator)
        {
            Validator.ValidateVariable(moderator, "moderator");
            await this.YouTubeServiceWrapper<object>(async () =>
            {
                LiveChatModeratorsResource.DeleteRequest request = this.connection.GoogleYouTubeService.LiveChatModerators.Delete(moderator.Id);
                LogRequest(request);

                string response = await request.ExecuteAsync();
                LogResponse(request, response);
                return null;
            });
        }

        /// <summary>
        /// Times out the specified user from the channel.
        /// </summary>
        /// <param name="broadcast">The broadcast of the live chat</param>
        /// <param name="user">The user to timeout</param>
        /// <param name="duration">The length of the timeout in seconds</param>
        /// <returns>The timeout result</returns>
        public async Task<LiveChatBan> TimeoutUser(LiveBroadcast broadcast, Channel user, ulong duration)
        {
            Validator.ValidateVariable(broadcast, "broadcast");
            Validator.ValidateVariable(user, "user");
            return await this.BanUserInternal(broadcast, user, "temporary", banDuration: duration);
        }

        /// <summary>
        /// Bans the specified user from the channel.
        /// </summary>
        /// <param name="broadcast">The broadcast of the live chat</param>
        /// <param name="user">The user to ban</param>
        /// <returns>The ban result</returns>
        public async Task<LiveChatBan> BanUser(LiveBroadcast broadcast, Channel user)
        {
            Validator.ValidateVariable(broadcast, "broadcast");
            Validator.ValidateVariable(user, "user");
            return await this.BanUserInternal(broadcast, user, "permanent", banDuration: 0);
        }

        /// <summary>
        /// Unbans the specified user from the channel.
        /// </summary>
        /// <param name="ban">The ban to remove</param>
        /// <returns>An awaitable Task</returns>
        public async Task UnbanUser(LiveChatBan ban)
        {
            Validator.ValidateVariable(ban, "ban");
            await this.YouTubeServiceWrapper<object>(async () =>
            {
                LiveChatBansResource.DeleteRequest request = this.connection.GoogleYouTubeService.LiveChatBans.Delete(ban.Id);
                LogRequest(request);

                string response = await request.ExecuteAsync();
                LogResponse(request, response);
                return null;
            });
        }

        private async Task<LiveChatBan> BanUserInternal(LiveBroadcast broadcast, Channel user, string banType, ulong banDuration = 0)
        {
            return await this.YouTubeServiceWrapper(async () =>
            {
                LiveChatBan ban = new LiveChatBan();
                ban.Snippet = new LiveChatBanSnippet();
                ban.Snippet.LiveChatId = broadcast.Snippet.LiveChatId;
                ban.Snippet.Type = banType;
                if (banDuration > 0)
                {
                    ban.Snippet.BanDurationSeconds = banDuration;
                }
                ban.Snippet.BannedUserDetails = new ChannelProfileDetails();
                ban.Snippet.BannedUserDetails.ChannelId = user.Id;

                LiveChatBansResource.InsertRequest request = this.connection.GoogleYouTubeService.LiveChatBans.Insert(ban, "snippet");
                LogRequest(request);

                ban = await request.ExecuteAsync();
                LogResponse(request, ban);
                return ban;
            });
        }
    }
}
