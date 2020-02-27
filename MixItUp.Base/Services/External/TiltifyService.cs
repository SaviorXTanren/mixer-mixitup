using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;


namespace MixItUp.Base.Services.External
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
        public DateTimeOffset Starts { get { return StreamingClient.Base.Util.DateTimeOffsetExtensions.FromUTCUnixTimeMilliseconds(this.StartsAtMilliseconds); } }

        [JsonIgnore]
        public DateTimeOffset Ends { get { return StreamingClient.Base.Util.DateTimeOffsetExtensions.FromUTCUnixTimeMilliseconds(this.EndsAtMilliseconds); } }
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
                Username = this.Name,
                Message = this.Comment,

                Amount = Math.Round(this.Amount, 2),

                DateTime = StreamingClient.Base.Util.DateTimeOffsetExtensions.FromUTCUnixTimeMilliseconds(this.CompletedAtTimestamp),
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

    public interface ITiltifyService : IOAuthExternalService
    {
        Task<Result> Connect(string authorizationToken);

        Task<TiltifyUser> GetUser();

        Task<IEnumerable<TiltifyCampaign>> GetUserCampaigns(TiltifyUser user);

        Task<IEnumerable<TiltifyTeam>> GetUserTeams(TiltifyUser user);

        Task<IEnumerable<TiltifyCampaign>> GetTeamCampaigns(TiltifyTeam team);

        Task<IEnumerable<TiltifyDonation>> GetCampaignDonations(TiltifyCampaign campaign);
    }

    public class TiltifyService : OAuthExternalServiceBase, ITiltifyService
    {
        private const string BaseAddress = "https://tiltify.com/api/v3/";

        public const string ClientID = "aa6b19e3f472808a632fe5a1b26b8ab37e852c123f60fb431c8a15c40df07f25";

        public const string ListeningURL = "https://localhost:8919/";
        public const string AuthorizationURL = "https://tiltify.com/oauth/authorize?client_id={0}&redirect_uri={1}&response_type=code";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private TiltifyUser user;

        private int currentCampaign = 0;
        private TiltifyCampaign campaign = null;
        private Dictionary<int, TiltifyDonation> donationsReceived = new Dictionary<int, TiltifyDonation>();

        public TiltifyService() : base(TiltifyService.BaseAddress) { }

        public override string Name { get { return "Tiltify"; } }

        public override Task<Result> Connect()
        {
            return Task.FromResult(new Result(false));
        }

        public async Task<Result> Connect(string authorizationToken)
        {
            try
            {
                if (!string.IsNullOrEmpty(authorizationToken))
                {
                    JObject payload = new JObject();
                    payload["grant_type"] = "authorization_code";
                    payload["client_id"] = TiltifyService.ClientID;
                    payload["client_secret"] = ChannelSession.Services.Secrets.GetSecret("TiltifySecret");
                    payload["code"] = authorizationToken;
                    payload["redirect_uri"] = TiltifyService.ListeningURL;

                    this.token = await this.PostAsync<OAuthTokenModel>("https://tiltify.com/oauth/token", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
                    if (this.token != null)
                    {
                        token.authorizationCode = authorizationToken;
                        token.AcquiredDateTime = DateTimeOffset.Now;
                        token.expiresIn = int.MaxValue;

                        return await this.InitializeInternal();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
            return new Result(false);
        }

        public override Task Disconnect()
        {
            this.token = null;
            this.cancellationTokenSource.Cancel();
            return Task.FromResult(0);
        }

        public async Task<TiltifyUser> GetUser()
        {
            try
            {
                TiltifyResult result = await this.GetAsync<TiltifyResult>("user");
                return result.Data.ToObject<TiltifyUser>();
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<TiltifyCampaign>> GetUserCampaigns(TiltifyUser user)
        {
            List<TiltifyCampaign> results = new List<TiltifyCampaign>();
            try
            {
                TiltifyResultArray result = await this.GetAsync<TiltifyResultArray>("users/" + user.ID.ToString() + "/campaigns");
                foreach (JToken token in result.Data)
                {
                    results.Add(token.ToObject<TiltifyCampaign>());
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
        }

        public async Task<IEnumerable<TiltifyTeam>> GetUserTeams(TiltifyUser user)
        {
            List<TiltifyTeam> results = new List<TiltifyTeam>();
            try
            {
                TiltifyResultArray result = await this.GetAsync<TiltifyResultArray>("users/" + user.ID.ToString() + "/teams");
                foreach (JToken token in result.Data)
                {
                    results.Add(token.ToObject<TiltifyTeam>());
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
        }

        public async Task<IEnumerable<TiltifyCampaign>> GetTeamCampaigns(TiltifyTeam team)
        {
            List<TiltifyCampaign> results = new List<TiltifyCampaign>();
            try
            {
                TiltifyResultArray result = await this.GetAsync<TiltifyResultArray>("teams/" + team.ID.ToString() + "/campaigns");
                foreach (JToken token in result.Data)
                {
                    results.Add(token.ToObject<TiltifyCampaign>());
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
        }

        public async Task<IEnumerable<TiltifyDonation>> GetCampaignDonations(TiltifyCampaign campaign)
        {
            List<TiltifyDonation> results = new List<TiltifyDonation>();
            try
            {
                TiltifyResultArray result = await this.GetAsync<TiltifyResultArray>("campaigns/" + campaign.ID.ToString() + "/donations");
                foreach (JToken token in result.Data)
                {
                    results.Add(token.ToObject<TiltifyDonation>());
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
        }

        protected override Task RefreshOAuthToken()
        {
            return Task.FromResult(0);
        }

        protected override void DisposeInternal()
        {
            this.cancellationTokenSource.Dispose();
        }

        protected override async Task<Result> InitializeInternal()
        {
            this.cancellationTokenSource = new CancellationTokenSource();

            this.user = await this.GetUser();
            if (this.user != null)
            {
                AsyncRunner.RunBackgroundTask(this.cancellationTokenSource.Token, 30000, this.BackgroundDonationCheck);

                return new Result();
            }
            return new Result("Failed to get User data");
        }

        private async Task BackgroundDonationCheck(CancellationToken token)
        {
            if (ChannelSession.Settings.TiltifyCampaign != currentCampaign)
            {
                currentCampaign = ChannelSession.Settings.TiltifyCampaign;
                donationsReceived.Clear();

                IEnumerable<TiltifyCampaign> campaigns = await this.GetUserCampaigns(this.user);
                campaign = campaigns.FirstOrDefault(c => c.ID.Equals(currentCampaign));
                if (campaign != null)
                {
                    foreach (TiltifyDonation donation in await this.GetCampaignDonations(campaign))
                    {
                        donationsReceived[donation.ID] = donation;
                    }
                }
            }

            if (campaign != null)
            {
                foreach (TiltifyDonation tDonation in await this.GetCampaignDonations(campaign))
                {
                    if (!donationsReceived.ContainsKey(tDonation.ID))
                    {
                        donationsReceived[tDonation.ID] = tDonation;
                        await ChannelSession.Services.Events.PerformEvent(await EventService.ProcessDonationEvent(EventTypeEnum.TiltifyDonation, tDonation.ToGenericDonation()));
                    }
                }
            }
        }
    }
}
