using Mixer.Base.Model.OAuth;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    #region Game Wisp Classes

    public class GameWispResponse<T>
    {
        [JsonProperty("result")]
        public GameWispResult Result { get; set; }

        [JsonProperty("data")]
        public JObject Data { get; set; }

        [JsonProperty("meta")]
        public JObject Meta { get; set; }

        public bool HasData { get { return this.Result != null && this.Result.Status == 1 && this.Data != null; } }

        public T GetResponseData() { return this.Data.ToObject<T>(); }

        public GameWispCursor GetCursor() { return (this.Meta != null && this.Meta["cursor"] != null) ? this.Meta["cursor"].ToObject<GameWispCursor>() : null; }
    }

    public class GameWispArrayResponse<T>
    {
        [JsonProperty("result")]
        public GameWispResult Result { get; set; }

        [JsonProperty("data")]
        public JArray Data { get; set; }

        [JsonProperty("meta")]
        public JObject Meta { get; set; }

        public bool HasData { get { return this.Result != null && this.Result.Status == 1 && this.Data != null; } }

        public IEnumerable<T> GetResponseData()
        {
            List<T> results = new List<T>();
            foreach (JToken token in this.Data)
            {
                results.Add(token.ToObject<T>());
            }
            return results;
        }

        public GameWispCursor GetCursor() { return (this.Meta != null && this.Meta["cursor"] != null) ? this.Meta["cursor"].ToObject<GameWispCursor>() : null; }
    }

    public class GameWispResult
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class GameWispDataWrapper<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; }
    }

    public class GameWispDataArrayWrapper<T>
    {
        [JsonProperty("data")]
        public List<T> Data { get; set; }
    }

    public class GameWispCursor
    {
        [JsonProperty("current")]
        public string Current { get; set; }

        [JsonProperty("prev")]
        public string Previous { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class GameWispChannelInformation
    {
        [JsonProperty("id")]
        public uint ID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("owner_user_id")]
        public uint UserID { get; set; }

        [JsonProperty("blurb")]
        public string Blurb { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("links")]
        public GameWispLinks Links { get; set; }

        [JsonProperty("tiers")]
        public GameWispDataArrayWrapper<GameWispTier> Tiers { get; set; }

        [JsonProperty("sponsor_counts")]
        public GameWispDataWrapper<GameWispSponsorCounts> SponsorCounts { get; set; }

        public IEnumerable<GameWispTier> GetActiveTiers() { return this.Tiers.Data.Where(t => t.Published); }
    }

    public class GameWispLinks
    {
        [JsonProperty("rel")]
        public string Relative { get; set; }

        [JsonProperty("uri")]
        public string URI { get; set; }
    }

    public class GameWispSponsorCounts
    {
        [JsonProperty("active")]
        public int Active { get; set; }

        [JsonProperty("trial")]
        public int Trial { get; set; }

        [JsonProperty("grace_period")]
        public int GracePeriod { get; set; }

        [JsonProperty("inactive")]
        public int Inactive { get; set; }

        [JsonProperty("billing_grace_period")]
        public int BillingGracePeriod { get; set; }
    }

    public class GameWispTier
    {
        public const string MIURolePrefix = "GW - ";

        [JsonProperty("id")]
        public uint ID { get; set; }

        [JsonProperty("level")]
        public uint Level { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("cost")]
        public string Cost { get; set; }

        [JsonProperty("published")]
        public bool Published { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonIgnore]
        public string MIURoleName { get { return string.Format("{0}{1}", GameWispTier.MIURolePrefix, this.Title); } }
    }

    public class GameWispSubscriber
    {
        [JsonProperty("id")]
        public uint ID { get; set; }

        [JsonProperty("user_id")]
        public uint UserID { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("subscribed_at")]
        public string SubscribedAt { get; set; }

        [JsonProperty("tier_id")]
        public string TierID { get; set; }

        [JsonProperty("user")]
        public GameWispDataWrapper<GameWispUser> User { get; set; }

        [JsonProperty("benefits")]
        public GameWispArrayResponse<GameWispBenefitFulfillmentPair> BenefitFullfillmentPairs { get; set; }

        [JsonProperty("anniversaries")]
        public GameWispDataArrayWrapper<GameWispAnniversary> Anniversaries { get; set; }

        [JsonIgnore]
        public string UserName { get { return this.User.Data.Username; } }
    }

    public class GameWispUser
    {
        [JsonProperty("id")]
        public uint ID { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("banned")]
        public bool Banned { get; set; }

        [JsonProperty("deactivated")]
        public bool Deactivated { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("links")]
        public GameWispLinks Links { get; set; }
    }

    public class GameWispAnniversary
    {
        [JsonProperty("id")]
        public uint ID { get; set; }

        [JsonProperty("subscriber_id")]
        public uint SubscriberID { get; set; }

        [JsonProperty("fired")]
        public bool Fired { get; set; }

        [JsonProperty("month_count")]
        public int MonthCount { get; set; }

        [JsonProperty("subscribed_at")]
        public string SubscribedAt { get; set; }

        [JsonProperty("renewed_at")]
        public string RenewedAt { get; set; }

        [JsonProperty("activation_url")]
        public string ActivationUrl { get; set; }
    }

    public class GameWispBenefitFulfillmentPair
    {
        [JsonProperty("benefit")]
        public GameWispBenefit Benefit { get; set; }

        [JsonProperty("fulfillment")]
        public GameWispFullfillment Fullfillment { get; set; }
    }

    public class GameWispBenefit
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("delivery")]
        public string Delivery { get; set; }

        [JsonProperty("channel_data")]
        public string ChannelData { get; set; }

        [JsonProperty("month_delay")]
        public string MonthDelay { get; set; }

        [JsonProperty("recurring")]
        public bool Recurring { get; set; }

        [JsonProperty("recurring_input")]
        public bool RecurringInput { get; set; }

        [JsonProperty("receieve_immediately")]
        public bool ReceiveImmediately { get; set; }

        [JsonProperty("removed_at")]
        public string RemovedAt { get; set; }

        [JsonProperty("subscriber_limit")]
        public string SubscriberLimit { get; set; }

        [JsonProperty("tier_bonus")]
        public bool TierBonus { get; set; }

        [JsonProperty("quantity")]
        public string Quantity { get; set; }

        [JsonProperty("multiplier")]
        public string Multiplier { get; set; }
    }

    public class GameWispFullfillment
    {
        [JsonProperty("id")]
        public uint ID { get; set; }

        [JsonProperty("benefit_id")]
        public uint BenefitID { get; set; }

        [JsonProperty("tier_id")]
        public uint TierID { get; set; }

        [JsonProperty("channel_fulfillment_response")]
        public string ChannelFulfillmentResponse { get; set; }

        [JsonProperty("fulfilled_at")]
        public string FulfilledAt { get; set; }

        [JsonProperty("previously_fulfilled_at")]
        public string PreviouslyFulfilledAt { get; set; }

        [JsonProperty("disabled_at")]
        public string DisabledAt { get; set; }

        [JsonProperty("user_input_provided_at")]
        public string UserInputProvidedAt { get; set; }

        [JsonProperty("recurring")]
        public string Recurring { get; set; }

        [JsonProperty("granted_at")]
        public string GrantedAt { get; set; }

        [JsonProperty("channel_cancelled_at")]
        public string ChannelCancelledAt { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    [DataContract]
    public class GameWispSubscribeEvent
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Amount { get; set; }
        [DataMember]
        public string TierID { get; set; }
        [DataMember]
        public string SubscribedAt { get; set; }
        [DataMember]
        public string Active { get; set; }

        public GameWispSubscribeEvent() { }

        public GameWispSubscribeEvent(JObject jobj)
        {
            this.ID = jobj["ids"]["gamewisp"].ToString();
            this.Username = jobj["usernames"]["gamewisp"].ToString();
            this.Amount = jobj["amount"].ToString();
            if (jobj["tier_id"] != null)
            {
                this.TierID = jobj["tier_id"].ToString();

            }
            else
            {
                this.TierID = jobj["tier"]["id"].ToString();
            }
            this.SubscribedAt = jobj["subscribed_at"].ToString();
            this.Active = jobj["status"].ToString();
        }

        [JsonIgnore]
        public int SubscribeMonths
        {
            get
            {
                if (DateTimeOffset.TryParse(this.SubscribedAt, out DateTimeOffset subDate))
                {
                    return subDate.TotalMonthsFromNow();
                }
                return 1;
            }
        }
    }

    [DataContract]
    public class GameWispResubscribeEvent : GameWispSubscribeEvent
    {
        [DataMember]
        public int AlertID { get; set; }

        public GameWispResubscribeEvent() { }

        public GameWispResubscribeEvent(JObject jobj)
            : base(jobj)
        {
            this.AlertID = (int)jobj["resubscribe_alert_id"];
        }
    }

    [DataContract]
    public class GameWispBenefitsChangeEvent : GameWispSubscribeEvent
    {
        [DataMember]
        public List<GameWispBenefitFulfillmentPair> BenefitFullfillmentPairs { get; set; }

        public GameWispBenefitsChangeEvent()
        {
            this.BenefitFullfillmentPairs = new List<GameWispBenefitFulfillmentPair>();
        }

        public GameWispBenefitsChangeEvent(JObject jobj)
        {
            this.BenefitFullfillmentPairs = new List<GameWispBenefitFulfillmentPair>();
            foreach (JToken token in (JArray)jobj["benefits"])
            {
                this.BenefitFullfillmentPairs.Add(token.ToObject<GameWispBenefitFulfillmentPair>());
            }
        }
    }

    [DataContract]
    public class GameWispAnniversaryEvent : GameWispSubscribeEvent
    {
        [DataMember]
        public int Months { get; set; }

        public GameWispAnniversaryEvent() { }

        public GameWispAnniversaryEvent(JObject jobj)
            : base((JObject)jobj["subscriber"])
        {
            this.Months = (int)jobj["month_count"];
        }
    }

    #endregion Game Wisp Classes

    public interface IGameWispService
    {
        GameWispChannelInformation ChannelInfo { get; }

        bool WebSocketConnectedAndAuthenticated { get; }

        event EventHandler OnWebSocketConnectedOccurred;
        event EventHandler OnWebSocketDisconnectedOccurred;

        event EventHandler<GameWispSubscribeEvent> OnSubscribeOccurred;
        event EventHandler<GameWispResubscribeEvent> OnResubscribeOccurred;
        event EventHandler<GameWispBenefitsChangeEvent> OnSubscriberBenefitsChangeOccurred;
        event EventHandler<GameWispSubscribeEvent> OnSubscriberStatusChangeOccurred;
        event EventHandler<GameWispAnniversaryEvent> OnSubscriberAnniversaryOccurred;

        Task<bool> Connect();
        Task Disconnect();

        Task<GameWispChannelInformation> GetChannelInformation();

        Task<IEnumerable<GameWispSubscriber>> GetSubscribers();
        Task<IEnumerable<GameWispSubscriber>> GetCachedSubscribers();

        Task<GameWispSubscriber> GetSubscriber(string username);
        Task<GameWispSubscriber> GetSubscriber(uint userID);

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
