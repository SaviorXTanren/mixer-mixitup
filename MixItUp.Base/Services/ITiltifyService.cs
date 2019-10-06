using MixItUp.Base.Model.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Model.OAuth;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using StreamingClient.Base.Util;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class TiltifyUser
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("url")]
        public string URL { get; set; }
    }

    [DataContract]
    public class TiltifyTeam
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("url")]
        public string URL { get; set; }

        [JsonProperty("totalAmountRaised")]
        public double TotalAmountRaised { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }
    }

    [DataContract]
    public class TiltifyCampaign
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("startsAt")]
        public long StartsAtMilliseconds { get; set; }

        [JsonProperty("endsAt")]
        public long EndsAtMilliseconds { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("fundraiserGoalAmount")]
        public double FundraiserGoalAmount { get; set; }

        [JsonProperty("originalFundraiserGoal")]
        public double OriginalGoalAmount { get; set; }

        [JsonProperty("supportingAmountRaised")]
        public double SupportingAmountRaised { get; set; }

        [JsonProperty("amountRaised")]
        public double AmountRaised { get; set; }

        [JsonProperty("totalAmountRaised")]
        public double TotalAmountRaised { get; set; }

        [JsonProperty("supportable")]
        public bool Supportable { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("user")]
        public TiltifyUser User { get; set; }

        [JsonProperty("teamId")]
        public int TeamID { get; set; }

        [JsonProperty("team")]
        public TiltifyTeam Team { get; set; }

        [JsonIgnore]
        public string CampaignURL { get { return string.Format("https://tiltify.com/@{0}/{1}", this.User.Slug, this.Slug); } }

        [JsonIgnore]
        public string DonateURL { get { return string.Format("{0}/donate", this.CampaignURL); } }

        [JsonIgnore]
        public DateTimeOffset Starts { get { return DateTimeOffsetExtensions.FromUTCUnixTimeMilliseconds(this.StartsAtMilliseconds); } }

        [JsonIgnore]
        public DateTimeOffset Ends { get { return DateTimeOffsetExtensions.FromUTCUnixTimeMilliseconds(this.EndsAtMilliseconds); } }
    }

    [DataContract]
    public class TiltifyDonation
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("completedAt")]
        public long CompletedAtTimestamp { get; set; }

        public UserDonationModel ToGenericDonation()
        {
            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.Tiltify,

                ID = this.ID.ToString(),
                UserName = this.Name,
                Message = this.Comment,

                Amount = Math.Round(this.Amount, 2),

                DateTime = DateTimeOffsetExtensions.FromUTCUnixTimeMilliseconds(this.CompletedAtTimestamp),
            };
        }
    }

    [DataContract]
    public class TiltifyResult : TiltifyResultBase
    {
        [JsonProperty("data")]
        public JObject Data { get; set; }
    }

    [DataContract]
    public class TiltifyResultArray : TiltifyResultBase
    {
        [JsonProperty("data")]
        public JArray Data { get; set; }
    }

    [DataContract]
    public abstract class TiltifyResultBase
    {
        [JsonProperty("meta")]
        public JObject Meta { get; set; }

        [JsonProperty("links")]
        public JObject Links { get; set; }

        [JsonProperty("error")]
        public JObject Error { get; set; }

        [JsonProperty("errors")]
        public JObject Errors { get; set; }
    }

    public interface ITiltifyService
    {
        Task<bool> Connect();

        Task Disconnect();

        Task<TiltifyUser> GetUser();

        Task<IEnumerable<TiltifyCampaign>> GetUserCampaigns(TiltifyUser user);

        Task<IEnumerable<TiltifyTeam>> GetUserTeams(TiltifyUser user);

        Task<IEnumerable<TiltifyCampaign>> GetTeamCampaigns(TiltifyTeam team);

        Task<IEnumerable<TiltifyDonation>> GetCampaignDonations(TiltifyCampaign campaign);

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
