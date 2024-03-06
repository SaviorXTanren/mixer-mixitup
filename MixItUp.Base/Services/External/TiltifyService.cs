using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;


namespace MixItUp.Base.Services.External
{
    [DataContract]
    public class TiltifyUser
    {
        public string id { get; set; }

        public string username { get; set; }

        public string url { get; set; }
    }

    [DataContract]
    public class TiltifyTeam
    {
        public string id { get; set; }

        public string name { get; set; }
    }

    [DataContract]
    public class TiltifyCampaign
    {
        public string id { get; set; }

        public int legacy_id { get; set; }

        public string name { get; set; }

        public string cause_id { get; set; }

        public string description { get; set; }

        public string url { get; set; }

        public string donate_url { get; set; }

        public string fundraising_event_id { get; set; }

        public string status { get; set; }

        public JObject goal { get; set; }

        public JObject original_goal { get; set; }

        public JObject amount_raised { get; set; }

        public JObject total_amount_raised { get; set; }

        [JsonIgnore]
        public double AmountRaised { get { return TiltifyService.GetValueFromTiltifyJObject(this.amount_raised); } }
        [JsonIgnore]
        public double TotalAmountRaised { get { return TiltifyService.GetValueFromTiltifyJObject(this.total_amount_raised); } }
        [JsonIgnore]
        public double Goal { get { return TiltifyService.GetValueFromTiltifyJObject(this.goal); } }
        [JsonIgnore]
        public double OriginalGoal { get { return TiltifyService.GetValueFromTiltifyJObject(this.original_goal); } }
    }

    [DataContract]
    public class TiltifyDonation
    {
        public string id { get; set; }

        public string campaign_id { get; set; }

        public string cause_id { get; set; }

        public string completed_at { get; set; }

        public string donor_name { get; set; }

        public string donor_comment { get; set; }

        public JObject amount { get; set; }

        [JsonIgnore]
        public double Amount { get { return TiltifyService.GetValueFromTiltifyJObject(this.amount); } }

        [JsonIgnore]
        public DateTimeOffset Timestamp
        {
            get
            {
                if (DateTimeOffset.TryParse(this.completed_at, out DateTimeOffset value))
                {
                    return value;
                }
                return DateTimeOffset.MinValue;
            }
        }

        public UserDonationModel ToGenericDonation()
        {
            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.Tiltify,

                ID = this.id.ToString(),
                Username = this.donor_name,
                Message = this.donor_comment,

                Amount = Math.Round(this.Amount, 2),

                DateTime = this.Timestamp,
            };
        }
    }

    [DataContract]
    public class TiltifyResult
    {
        [JsonProperty("data")]
        public JObject data { get; set; }
    }

    [DataContract]
    public class TiltifyResultArray
    {
        [JsonProperty("data")]
        public JArray data { get; set; }
    }

    public class TiltifyService : OAuthExternalServiceBase
    {
        private const string BaseAddress = "https://v5api.tiltify.com/";

        public const string ClientID = "74ea7c434fe177e560fb7c6262728e5c3cae9ddb494c179d4c9c35e924e27342";

        public const string AuthorizationURL = "https://v5api.tiltify.com/oauth/authorize?client_id={0}&redirect_uri={1}&response_type=code&scope=public";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private TiltifyUser user;

        private TiltifyCampaign campaign = null;
        private Dictionary<string, TiltifyDonation> donationsReceived = new Dictionary<string, TiltifyDonation>();

        public TiltifyService() : base(TiltifyService.BaseAddress) { }

        public override string Name { get { return MixItUp.Base.Resources.Tiltify; } }

        public static double GetValueFromTiltifyJObject(JObject jobj)
        {
            if (jobj != null && jobj.ContainsKey("value") && double.TryParse(jobj["value"].ToString(), out double value))
            {
                return value;
            }
            return 0;
        }

        public override async Task<Result> Connect()
        {
            try
            {
                string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(TiltifyService.AuthorizationURL, TiltifyService.ClientID, OAuthExternalServiceBase.DEFAULT_OAUTH_LOCALHOST_URL));
                if (!string.IsNullOrEmpty(authorizationCode))
                {
                    JObject payload = new JObject();
                    payload["grant_type"] = "authorization_code";
                    payload["client_id"] = TiltifyService.ClientID;
                    payload["client_secret"] = ServiceManager.Get<SecretsService>().GetSecret("TiltifySecret");
                    payload["code"] = authorizationCode;
                    payload["scope"] = "public";

                    this.token = await this.PostAsync<OAuthTokenModel>("https://v5api.tiltify.com/oauth/token", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
                    if (this.token != null)
                    {
                        token.authorizationCode = authorizationCode;
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
            return Task.CompletedTask;
        }

        public async Task<TiltifyUser> GetUser()
        {
            try
            {
                TiltifyResult result = await this.GetAsync<TiltifyResult>("api/public/current-user");
                return result.data.ToObject<TiltifyUser>();
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<TiltifyCampaign> GetCampaign(string campaignID)
        {
            try
            {
                TiltifyResult result = await this.GetAsync<TiltifyResult>("api/public/campaigns/" + campaignID.ToString());
                return result.data.ToObject<TiltifyCampaign>();
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<TiltifyCampaign>> GetUserCampaigns(TiltifyUser user)
        {
            List<TiltifyCampaign> results = new List<TiltifyCampaign>();
            try
            {
                TiltifyResultArray result = await this.GetAsync<TiltifyResultArray>($"api/public/users/{user.id}/campaigns");
                foreach (JToken token in result.data)
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
                TiltifyResultArray result = await this.GetAsync<TiltifyResultArray>($"api/public/users/{user.id}/teams");
                foreach (JToken token in result.data)
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
                TiltifyResultArray result = await this.GetAsync<TiltifyResultArray>($"api/public/teams/{team.id}/team_campaigns");
                foreach (JToken token in result.data)
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
                TiltifyResultArray result = await this.GetAsync<TiltifyResultArray>($"api/public/campaigns/{campaign.id}/donations?limit=20");
                foreach (JToken token in result.data)
                {
                    results.Add(token.ToObject<TiltifyDonation>());
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                JObject payload = new JObject();
                payload["grant_type"] = "refresh_token";
                payload["client_id"] = TiltifyService.ClientID;
                payload["client_secret"] = ServiceManager.Get<SecretsService>().GetSecret("TiltifySecret");
                payload["refresh_token"] = this.token.refreshToken;
                payload["scope"] = "public";

                this.token = await this.PostAsync<OAuthTokenModel>("https://v5api.tiltify.com/oauth/token", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
            }
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(this.BackgroundDonationCheck, this.cancellationTokenSource.Token, 60000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                this.TrackServiceTelemetry("Tiltify");
                return new Result();
            }
            return new Result(Resources.TiltifyUserDataFailed);
        }

        private async Task BackgroundDonationCheck(CancellationToken token)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            if (ChannelSession.Settings.TiltifyCampaign > 0)
            {
                // Legacy upgrade to new V5 API
                campaign = await this.GetCampaign(ChannelSession.Settings.TiltifyCampaign.ToString());
                if (campaign != null)
                {
                    ChannelSession.Settings.TiltifyCampaignV5 = campaign.id;
                    ChannelSession.Settings.TiltifyCampaign = 0;
                }
            }
#pragma warning restore CS0612 // Type or member is obsolete

            if (string.IsNullOrWhiteSpace(ChannelSession.Settings.TiltifyCampaignV5))
            {
                campaign = null;
            }
            else if (campaign == null || ChannelSession.Settings.TiltifyCampaignV5 != this.campaign.id)
            {
                donationsReceived.Clear();
                campaign = await this.GetCampaign(ChannelSession.Settings.TiltifyCampaignV5);
                if (campaign != null)
                {
                    foreach (TiltifyDonation donation in await this.GetCampaignDonations(campaign))
                    {
                        donationsReceived[donation.id] = donation;
                    }
                }
            }

            if (campaign != null)
            {
                foreach (TiltifyDonation tDonation in await this.GetCampaignDonations(campaign))
                {
                    if (!donationsReceived.ContainsKey(tDonation.id))
                    {
                        donationsReceived[tDonation.id] = tDonation;
                        await EventService.ProcessDonationEvent(EventTypeEnum.TiltifyDonation, tDonation.ToGenericDonation());
                    }
                }
            }
        }
    }
}
