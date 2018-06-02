using Mixer.Base.Model.OAuth;
using Mixer.Base.Util;
using MixItUp.Base.Model.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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

        [JsonProperty("fundraiserGoalAmount")]
        public double FundraiserGoalAmount { get; set; }

        [JsonProperty("originalFundraiserGoal")]
        public double OriginalGoalAmount { get; set; }

        [JsonProperty("supportingAmountRaised")]
        public double SupportingAmountRaised { get; set; }

        [JsonProperty("totalAmountRaised")]
        public double TotalAmountRaised { get; set; }

        [JsonProperty("supportable")]
        public bool Supportable { get; set; }

        [JsonProperty("user")]
        public TiltifyUser User { get; set; }

        [JsonIgnore]
        public string CampaignURL { get { return string.Format("https://tiltify.com/@{0}/{1}", this.User.Slug, this.Slug); } }

        [JsonIgnore]
        public string DonateURL { get { return string.Format("{0}/donate", this.CampaignURL); } }
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

                Amount = this.Amount,
                AmountText = string.Format("{0:C}", this.Amount),

                DateTime = DateTimeHelper.UnixTimestampToDateTimeOffset(this.CompletedAtTimestamp),
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

        Task<IEnumerable<TiltifyCampaign>> GetCampaigns(TiltifyUser user);

        Task<IEnumerable<TiltifyDonation>> GetCampaignDonations(TiltifyCampaign campaign);

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
