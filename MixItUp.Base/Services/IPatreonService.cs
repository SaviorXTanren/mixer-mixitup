using Newtonsoft.Json;
using StreamingClient.Base.Model.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class PatreonUser : IEquatable<PatreonUser>
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
        public DateTimeOffset? Created { get; set; }

        [JsonIgnore]
        public string LookupName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Vanity))
                {
                    return this.Vanity;
                }
                return this.FullName;
            }
        }

        public PatreonUser() { }

        public override bool Equals(object obj)
        {
            if (obj is PatreonUser)
            {
                return this.Equals((PatreonUser)obj);
            }
            return false;
        }

        public bool Equals(PatreonUser other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }

    [DataContract]
    public class PatreonCampaign : IEquatable<PatreonCampaign>
    {
        [DataMember]
        public string ID { get; set; }

        [JsonProperty("patron_count")]
        public int PatronCount { get; set; }

        [JsonProperty("creation_name")]
        public string CreationName { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("published_at")]
        public DateTimeOffset? PublishedAt { get; set; }

        [DataMember]
        public Dictionary<string, PatreonTier> Tiers { get; set; }

        [DataMember]
        public Dictionary<string, PatreonBenefit> Benefits { get; set; }

        public PatreonCampaign()
        {
            this.Tiers = new Dictionary<string, PatreonTier>();
            this.Benefits = new Dictionary<string, PatreonBenefit>();
        }

        [JsonProperty]
        public IEnumerable<PatreonTier> ActiveTiers { get { return this.Tiers.Values.Where(t => t.Published); } }

        public PatreonTier GetTier(string tierID)
        {
            if (!string.IsNullOrEmpty(tierID) && this.Tiers.ContainsKey(tierID) && this.Tiers[tierID].Published)
            {
                return this.Tiers[tierID];
            }
            return null;
        }

        public PatreonBenefit GetBenefit(string benefitID)
        {
            if (!string.IsNullOrEmpty(benefitID) && this.Benefits.ContainsKey(benefitID) && this.Benefits[benefitID].Published && !this.Benefits[benefitID].Deleted)
            {
                return this.Benefits[benefitID];
            }
            return null;
        }

        public override bool Equals(object obj)
        {
            if (obj is PatreonCampaign)
            {
                return this.Equals((PatreonCampaign)obj);
            }
            return false;
        }

        public bool Equals(PatreonCampaign other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }

    [DataContract]
    public class PatreonTier : IEquatable<PatreonTier>
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
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("published_at")]
        public DateTimeOffset? PublishedAt { get; set; }

        [DataMember]
        public HashSet<string> BenefitIDs { get; set; }

        [JsonIgnore]
        public double Amount { get { return Math.Round(((double)this.AmountCents) / 100.0, 2); } }

        public PatreonTier()
        {
            this.BenefitIDs = new HashSet<string>();
        }

        public override bool Equals(object obj)
        {
            if (obj is PatreonTier)
            {
                return this.Equals((PatreonTier)obj);
            }
            return false;
        }

        public bool Equals(PatreonTier other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }

    [DataContract]
    public class PatreonBenefit : IEquatable<PatreonBenefit>
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
        public DateTimeOffset? CreatedAt { get; set; }

        public PatreonBenefit() { }

        public override bool Equals(object obj)
        {
            if (obj is PatreonBenefit)
            {
                return this.Equals((PatreonBenefit)obj);
            }
            return false;
        }

        public bool Equals(PatreonBenefit other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }

    [DataContract]
    public class PatreonCampaignMember : IEquatable<PatreonCampaignMember>
    {
        [DataMember]
        public string ID { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("patron_status")]
        public string PatronStatus { get; set; }

        [JsonProperty("will_pay_amount_cents")]
        public int AmountToPay { get; set; }

        [JsonProperty("currently_entitled_amount_cents")]
        public int CurrentAmountPaying { get; set; }

        [JsonProperty("lifetime_support_cents")]
        public int LifetimeAmountPaid { get; set; }

        [DataMember]
        public string UserID { get; set; }

        [DataMember]
        public PatreonUser User { get; set; }

        [DataMember]
        public string TierID { get; set; }

        [JsonIgnore]
        public double Amount { get { return Math.Round(((double)this.AmountToPay) / 100.0, 2); } }

        public PatreonCampaignMember() { }

        public override bool Equals(object obj)
        {
            if (obj is PatreonCampaignMember)
            {
                return this.Equals((PatreonCampaignMember)obj);
            }
            return false;
        }

        public bool Equals(PatreonCampaignMember other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }

    public interface IPatreonService
    {
        PatreonCampaign Campaign { get; }
        IEnumerable<PatreonCampaignMember> CampaignMembers { get; }

        Task<bool> Connect();

        Task Disconnect();

        Task<PatreonUser> GetCurrentUser();

        Task<PatreonCampaign> GetCampaign();

        Task<IEnumerable<PatreonCampaignMember>> GetCampaignMembers();

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
