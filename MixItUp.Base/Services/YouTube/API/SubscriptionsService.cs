using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.YouTube.API
{
    /// <summary>
    /// The APIs for Subscription-based services.
    /// </summary>
    public class SubscriptionsService : YouTubeServiceBase
    {
        /// <summary>
        /// Creates an instance of the SubscriptionsService.
        /// </summary>
        /// <param name="connection">The YouTube connection to use</param>
        public SubscriptionsService(YouTubeConnection connection) : base(connection) { }

        /// <summary>
        /// Gets the subscriptions for the currently authenticated account.
        /// </summary>
        /// <param name="maxResults">The maximum results to return</param>
        /// <returns>The list of subscriptions</returns>
        public async Task<IEnumerable<Subscription>> GetMySubscriptions(int maxResults = 1)
        {
            return await this.GetSubscriptions(mySubscriptions: true, maxResults: maxResults);
        }

        /// <summary>
        /// Gets the channels that are subscribing to the currently authenticated account.
        /// </summary>
        /// <param name="maxResults">The maximum results to return</param>
        /// <returns>The list of subscriptions</returns>
        public async Task<IEnumerable<Subscription>> GetMySubscribers(int maxResults = 1)
        {
            return await this.GetSubscriptions(mySubscribers: true, maxResults: maxResults);
        }

        /// <summary>
        /// Gets the channels that have recently subscribed to the currently authenticated account.
        /// </summary>
        /// <param name="maxResults">The maximum results to return</param>
        /// <returns>The list of subscriptions</returns>
        public async Task<IEnumerable<Subscription>> GetMyRecentSubscribers(int maxResults = 1)
        {
            return await this.GetSubscriptions(myRecentSubscribers: true, maxResults: maxResults);
        }

        /// <summary>
        /// Gets the subscription information for the subscriber channel to the subscribed channel if it exists.
        /// </summary>
        /// <param name="subscribedChannel">The channel to check if they are subscribed to</param>
        /// <param name="subscriberChannel">The channel to check if they are subscribed</param>
        /// <returns>The subscription, if it exists</returns>
        public async Task<Subscription> CheckIfSubscribed(string subscribedChannel, string subscriberChannel)
        {
            var subscriptions = await this.GetSubscriptions(forChannelId: subscribedChannel, channelId: subscriberChannel, maxResults: 1);
            if (subscriptions != null)
            {
                return subscriptions.FirstOrDefault();
            }
            return null;
        }

        internal async Task<IEnumerable<Subscription>> GetSubscriptions(bool mySubscriptions = false, bool myRecentSubscribers = false, bool mySubscribers = false, string forChannelId = null, string channelId = null, int maxResults = 1)
        {
            return await this.YouTubeServiceWrapper(async () =>
            {
                List<Subscription> results = new List<Subscription>();
                string pageToken = null;
                do
                {
                    SubscriptionsResource.ListRequest request = this.connection.GoogleYouTubeService.Subscriptions.List("snippet,contentDetails");
                    if (mySubscriptions)
                    {
                        request.Mine = true;
                    }
                    else if (myRecentSubscribers)
                    {
                        request.MyRecentSubscribers = myRecentSubscribers;
                    }
                    else if (mySubscribers)
                    {
                        request.MySubscribers = mySubscribers;
                    }
                    else if (!string.IsNullOrEmpty(forChannelId) && !string.IsNullOrEmpty(channelId))
                    {
                        request.ForChannelId = forChannelId;
                        request.ChannelId = channelId;
                    }
                    request.MaxResults = Math.Min(maxResults, 50);
                    request.PageToken = pageToken;
                    LogRequest(request);

                    SubscriptionListResponse response = await request.ExecuteAsync();
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
