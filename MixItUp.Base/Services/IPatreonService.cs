using Mixer.Base.Model.OAuth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class PatreonUser
    {
        [DataMember]
        public string ID { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("vanity")]
        public string Vanity { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; }

        public PatreonUser() { }
    }

    [DataContract]
    public class PatreonCampaign
    {
        [DataMember]
        public string ID { get; set; }

        [JsonProperty("patron_count")]
        public int PatronCount { get; set; }

        [JsonProperty("creation_name")]
        public string CreationName { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("published_at")]
        public DateTimeOffset PublishedAt { get; set; }

        [DataMember]
        public Dictionary<string, PatreonTier> Tiers { get; set; }

        [DataMember]
        public Dictionary<string, PatreonBenefit> Benefits { get; set; }

        public PatreonCampaign()
        {
            this.Tiers = new Dictionary<string, PatreonTier>();
            this.Benefits = new Dictionary<string, PatreonBenefit>();
        }
    }

    [DataContract]
    public class PatreonTier
    {
        [DataMember]
        public string ID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("amount_cents")]
        public int AmountCents { get; set; }

        [JsonProperty("patron_count")]
        public int PatronCount { get; set; }

        [JsonProperty("published")]
        public bool Published { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("published_at")]
        public DateTimeOffset PublishedAt { get; set; }

        [DataMember]
        public HashSet<string> BenefitIDs { get; set; }

        [JsonIgnore]
        public double Amount { get { return ((double)this.AmountCents) / 100; } }

        public PatreonTier()
        {
            this.BenefitIDs = new HashSet<string>();
        }
    }

    [DataContract]
    public class PatreonBenefit
    {
        [DataMember]
        public string ID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("benefit_type")]
        public string BenefitType { get; set; }

        [JsonProperty("rule_type")]
        public string RuleType { get; set; }

        [JsonProperty("is_published")]
        public bool Published { get; set; }

        [JsonProperty("is_deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        public PatreonBenefit() { }
    }

    [DataContract]
    public class PatreonCampaignMember
    {
        [DataMember]
        public string ID { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("patron_status")]
        public string PatronStatus { get; set; }

        [JsonProperty("will_pay_amount_cents")]
        public int AmountToPay { get; set; }

        [DataMember]
        public string UserID { get; set; }

        [DataMember]
        public PatreonUser User { get; set; }

        [DataMember]
        public string TierID { get; set; }

        [JsonIgnore]
        public double Amount { get { return ((double)this.AmountToPay) / 100; } }

        public PatreonCampaignMember() { }
    }

    public interface IPatreonService
    {
        Task<bool> Connect();

        Task Disconnect();

        Task<PatreonUser> GetCurrentUser();

        Task<PatreonCampaign> GetCampaign();

        Task<IEnumerable<PatreonCampaignMember>> GetCampaignMembers();

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
