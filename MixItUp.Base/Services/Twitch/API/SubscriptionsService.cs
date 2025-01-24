using MixItUp.Base.Model.Twitch.Subscriptions;
using MixItUp.Base.Model.Twitch.User;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch.API
{
    /// <summary>
    /// The APIs for Subscription-based services.
    /// </summary>
    public class SubscriptionsService : NewTwitchAPIServiceBase
    {
        /// <summary>
        /// Creates an instance of the SubscriptionsService.
        /// </summary>
        /// <param name="connection">The Twitch connection to use</param>
        public SubscriptionsService(TwitchConnection connection) : base(connection) { }

        /// <summary>
        /// Gets the subscriptions for a broadcaster. The broadcaster specified must match the user in the auth token being used.
        /// </summary>
        /// <param name="broadcaster">The broadcaster to get subscriptions for</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <returns>The broadcaster's subscriptions</returns>
        public async Task<IEnumerable<SubscriptionModel>> GetBroadcasterSubscriptions(UserModel broadcaster, int maxResults = 1)
        {
            Validator.ValidateVariable(broadcaster, "broadcaster");
            return await this.GetPagedDataResultAsync<SubscriptionModel>("subscriptions?broadcaster_id=" + broadcaster.id, maxResults);
        }

        /// <summary>
        /// Gets the subscription for a specific user to the broadcaster. The broadcaster specified must match the user in the auth token being used.
        /// </summary>
        /// <param name="broadcaster">The broadcaster to get subscriptions for</param>
        /// <param name="user">The user that is subscribed to the broadcaster</param>
        /// <returns>The user's subscription to the broadcaster</returns>
        public async Task<SubscriptionModel> GetBroadcasterSubscription(UserModel broadcaster, UserModel user)
        {
            Validator.ValidateVariable(broadcaster, "broadcaster");
            Validator.ValidateVariable(user, "user");
            IEnumerable<SubscriptionModel> subscriptions = await this.GetPagedDataResultAsync<SubscriptionModel>($"subscriptions?broadcaster_id={broadcaster.id}&user_id={user.id}");
            return (subscriptions != null) ? subscriptions.FirstOrDefault() : null;
        }

        /// <summary>
        /// Gets the subscriptions for set of users to the broadcaster. The broadcaster specified must match the user in the auth token being used.
        /// </summary>
        /// <param name="broadcaster">The broadcaster to get subscriptions for</param>
        /// <param name="userIDs">The user IDs that are subscribed to the broadcaster</param>
        /// <returns>The user subscriptions to the broadcaster</returns>
        public async Task<IEnumerable<SubscriptionModel>> GetBroadcasterSubscriptions(UserModel broadcaster, IEnumerable<string> userIDs)
        {
            Validator.ValidateVariable(broadcaster, "broadcaster");
            Validator.ValidateList(userIDs, "userIDs");
            return await this.GetPagedDataResultAsync<SubscriptionModel>($"subscriptions?broadcaster_id={broadcaster.id}&user_id={string.Join("&user_id=", userIDs)}", userIDs.Count());
        }

        /// <summary>
        /// Gets the total subscriber points for a broadcaster's channel. The broadcaster specified must match the user in the auth token being used.
        /// </summary>
        /// <param name="broadcaster">The broadcaster to get the total number of subscribers for</param>
        /// <returns>The total number of subscriber points for the broadcaster's channel</returns>
        public async Task<long> GetBroadcasterSubscriptionPoints(UserModel broadcaster)
        {
            Validator.ValidateVariable(broadcaster, "broadcaster");

            JObject data = await this.GetJObjectAsync($"subscriptions?broadcaster_id={broadcaster.id}&first=1");
            if (data != null && data.ContainsKey("points"))
            {
                return (long)data["points"];
            }
            return 0;
        }

        /// <summary>
        /// Gets the total number of subscribers for a broadcaster's channel. The broadcaster specified must match the user in the auth token being used.
        /// </summary>
        /// <param name="broadcaster">The broadcaster to get the total number of subscribers for</param>
        /// <returns>The total number of subscribers for the broadcaster's channel</returns>
        public async Task<long> GetBroadcasterSubscriptionsCount(UserModel broadcaster)
        {
            Validator.ValidateVariable(broadcaster, "broadcaster");
            return await this.GetPagedResultTotalCountAsync("subscriptions?broadcaster_id=" + broadcaster.id);
        }

        /// <summary>
        /// Gets the subscription that the specified user for the specified broadcaster. The user specified must match the user in the auth token being used. Useful for checking if the authenticated is subbed to another user.
        /// </summary>
        /// <param name="user">The user to get subscription for. Must match the user of the auth token being used.</param>
        /// <param name="broadcaster">The broadcaster to whom to get that the user is subscribed to</param>
        /// <returns>The user's subscriptions to the broadcaster.</returns>
        public async Task<SubscriptionModel> GetSubscription(UserModel user, UserModel broadcaster)
        {
            Validator.ValidateVariable(user, "user");
            Validator.ValidateVariable(broadcaster, "broadcaster");
            IEnumerable<SubscriptionModel> subscriptions = await this.GetPagedDataResultAsync<SubscriptionModel>($"subscriptions/user?user_id={user.id}&broadcaster_id={broadcaster.id}");
            return (subscriptions != null) ? subscriptions.FirstOrDefault() : null;
        }
    }
}
