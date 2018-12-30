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

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; }

        public PatreonUser() { }
    }

    [DataContract]
    public class PatreonCampaign
    {
        [DataMember]
        public string ID { get; set; }

        [JsonProperty("pledge_sum")]
        public int PledgeSum { get; set; }

        [JsonProperty("patron_count")]
        public int PatronCount { get; set; }

        [JsonProperty("creation_name")]
        public string CreationName { get; set; }

        [JsonProperty("creation_count")]
        public int CreationCount { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("published_at")]
        public DateTimeOffset PublishedAt { get; set; }

        [DataMember]
        public List<PatreonTier> Tiers { get; set; }

        public PatreonCampaign() { this.Tiers = new List<PatreonTier>(); }
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
        public bool published { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("published_at")]
        public DateTimeOffset PublishedAt { get; set; }

        [JsonIgnore]
        public double Amount { get { return ((double)this.AmountCents) / 100; } }

        public PatreonTier() { }
    }

    [DataContract]
    public class PatreonPledge
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string UserID { get; set; }

        [DataMember]
        public PatreonUser User { get; set; }

        [DataMember]
        public string TierID { get; set; }

        [DataMember]
        public int AmountCents { get; set; }

        [JsonIgnore]
        public double Amount { get { return ((double)this.AmountCents) / 100; } }

        public PatreonPledge() { }
    }

    public interface IPatreonService
    {
        Task<bool> Connect();

        Task Disconnect();

        Task<PatreonUser> GetCurrentUser();

        Task<PatreonCampaign> GetCampaign();

        Task<IEnumerable<PatreonPledge>> GetPledges();

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
