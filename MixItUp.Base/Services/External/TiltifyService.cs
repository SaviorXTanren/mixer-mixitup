using MixItUp.Base.Model.User;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    [DataContract]
    public class TiltifyUser
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string username { get; set; }
        [DataMember]
        public string url { get; set; }
    }

    [DataContract]
    public class TiltifyTeam
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string name { get; set; }
    }

    [DataContract]
    public class TiltifyCampaign
    {
        public const string RetiredStatus = "retired";

        [DataMember]
        public string id { get; set; }
        [DataMember]
        public int legacy_id { get; set; }
        [DataMember]
        public string team_id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string cause_id { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string donate_url { get; set; }
        [DataMember]
        public string fundraising_event_id { get; set; }
        [DataMember]
        public string status { get; set; }
        [DataMember]
        public JObject goal { get; set; }
        [DataMember]
        public JObject original_goal { get; set; }
        [DataMember]
        public JObject amount_raised { get; set; }
        [DataMember]
        public JObject total_amount_raised { get; set; }

        [JsonIgnore]
        public double AmountRaised { get { return TiltifyService.GetValueFromTiltifyJObject(this.amount_raised); } }
        [JsonIgnore]
        public double TotalAmountRaised { get { return TiltifyService.GetValueFromTiltifyJObject(this.total_amount_raised); } }
        [JsonIgnore]
        public double Goal { get { return TiltifyService.GetValueFromTiltifyJObject(this.goal); } }
        [JsonIgnore]
        public double OriginalGoal { get { return TiltifyService.GetValueFromTiltifyJObject(this.original_goal); } }

        [JsonIgnore]
        public bool IsPartOfTeam { get { return !string.IsNullOrWhiteSpace(this.team_id); } }
        [JsonIgnore]
        public bool IsRetired { get { return string.Equals(this.status, RetiredStatus); } }
    }

    [DataContract]
    public class TiltifyDonation
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string campaign_id { get; set; }
        [DataMember]
        public string cause_id { get; set; }
        [DataMember]
        public string created_at { get; set; }
        [DataMember]
        public string completed_at { get; set; }
        [DataMember]
        public string donor_name { get; set; }
        [DataMember]
        public string donor_comment { get; set; }
        [DataMember]
        public JObject amount { get; set; }

        [JsonIgnore]
        public double Amount { get { return TiltifyService.GetValueFromTiltifyJObject(this.amount); } }

        [JsonIgnore]
        public DateTimeOffset Timestamp { get { return DateTimeOffsetExtensions.FromGeneralString(this.completed_at); } }

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
        public JObject data { get; set; }
    }

    [DataContract]
    public class TiltifyResultArray
    {
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

        public TiltifyCampaign Campaign { get { return this.campaign; } }

        private DateTimeOffset startTime;

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
                    payload["redirect_uri"] = OAuthExternalServiceBase.DEFAULT_OAUTH_LOCALHOST_URL;

                    this.token = await this.PostAsync<OAuthTokenModel>("https://v5api.tiltify.com/oauth/token", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
                    if (this.token != null)
                    {
                        token.expiresIn = int.MaxValue;

                        this.startTime = DateTimeOffset.Now;

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
            return await this.GetObjectResult<TiltifyUser>("api/public/current-user");
        }

        public async Task<TiltifyCampaign> GetUserCampaign(string campaignID)
        {
            return await this.GetObjectResult<TiltifyCampaign>($"api/public/campaigns/{campaignID}");
        }

        public async Task<TiltifyCampaign> GetTeamCampaign(string campaignID)
        {
            return await this.GetObjectResult<TiltifyCampaign>($"api/public/team_campaigns/{campaignID}");
        }

        public async Task<IEnumerable<TiltifyCampaign>> GetUserCampaigns(TiltifyUser user)
        {
            return await this.GetArrayResult<TiltifyCampaign>($"api/public/users/{user.id}/campaigns?limit=100", usePageCursor: true);
        }

        public async Task<IEnumerable<TiltifyTeam>> GetUserTeams(TiltifyUser user)
        {
            return await this.GetArrayResult<TiltifyTeam>($"api/public/users/{user.id}/teams?limit=100", usePageCursor: true);
        }

        public async Task<IEnumerable<TiltifyCampaign>> GetTeamCampaigns(TiltifyTeam team)
        {
            return await this.GetArrayResult<TiltifyCampaign>($"api/public/teams/{team.id}/team_campaigns?limit=100", usePageCursor: true);
        }

        public async Task<IEnumerable<TiltifyDonation>> GetCampaignDonations(TiltifyCampaign campaign)
        {
            if (campaign.IsPartOfTeam)
            {
                return await this.GetArrayResult<TiltifyDonation>($"api/public/team_campaigns/{campaign.id}/donations?limit=20");
            }
            else
            {
                return await this.GetArrayResult<TiltifyDonation>($"api/public/campaigns/{campaign.id}/donations?limit=20");
            }
        }

        public async Task RefreshCampaign()
        {
            if (this.campaign != null)
            {
                TiltifyCampaign campaign;
                if (this.campaign.IsPartOfTeam)
                {
                    campaign = await this.GetUserCampaign(this.campaign.id);
                }
                else
                {
                    campaign = await this.GetTeamCampaign(this.campaign.id);
                }

                if (campaign != null)
                {
                    this.campaign = campaign;
                }
            }
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
            if (string.IsNullOrWhiteSpace(ChannelSession.Settings.TiltifyCampaignV5))
            {
                this.campaign = null;
            }
            else if (this.campaign == null || ChannelSession.Settings.TiltifyCampaignV5 != this.campaign.id)
            {
                Logger.Log(LogLevel.Debug, $"Initializing campaign donations...");

                donationsReceived.Clear();

                if (ChannelSession.Settings.TiltifyCampaignV5IsTeam)
                {
                    this.campaign = await this.GetTeamCampaign(ChannelSession.Settings.TiltifyCampaignV5);
                }
                else
                {
                    this.campaign = await this.GetUserCampaign(ChannelSession.Settings.TiltifyCampaignV5);
                }

                if (this.campaign != null)
                {
                    foreach (TiltifyDonation donation in await this.GetCampaignDonations(this.campaign))
                    {
                        donationsReceived[donation.id] = donation;
                    }
                }
            }

            if (this.campaign != null)
            {
                foreach (TiltifyDonation tDonation in await this.GetCampaignDonations(this.campaign))
                {
                    Logger.Log(LogLevel.Debug, $"Checking of donation {tDonation.id} at {tDonation.Timestamp} has already been processed...");
                    if (!donationsReceived.ContainsKey(tDonation.id))
                    {
                        Logger.Log(LogLevel.Debug, $"Donation {tDonation.id} is not known, checking timestamp...");
                        donationsReceived[tDonation.id] = tDonation;
                        if (tDonation.Timestamp > this.startTime)
                        {
                            Logger.Log(LogLevel.Debug, $"Donation {tDonation.id} is new, start processing...");
                            await EventService.ProcessDonationEvent(EventTypeEnum.TiltifyDonation, tDonation.ToGenericDonation());
                        }
                        else
                        {
                            Logger.Log(LogLevel.Debug, $"Donation {tDonation.id} at {tDonation.Timestamp} is earlier than {this.startTime}");
                        }
                    }
                    else
                    {
                        Logger.Log(LogLevel.Debug, $"Donation {tDonation.id} has already been processed, skipping");
                    }
                }
            }
        }

        private async Task<T> GetObjectResult<T>(string url)
        {
            try
            {
                JObject result = await this.GetJObjectAsync(url);
                if (result != null && result.ContainsKey("data"))
                {
                    return result["data"].ToObject<T>();
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return default(T);
        }

        private async Task<IEnumerable<T>> GetArrayResult<T>(string url, bool usePageCursor = false)
        {
            string afterCursor = null;
            List<T> results = new List<T>();
            try
            {
                do
                {
                    string queryUrl = url;
                    if (!string.IsNullOrEmpty(afterCursor))
                    {
                        queryUrl += $"&after={afterCursor}";
                    }

                    JObject result = await this.GetJObjectAsync(queryUrl);
                    afterCursor = null;

                    if (result != null)
                    {
                        if (result.ContainsKey("data"))
                        {
                            foreach (JToken token in (JArray)result["data"])
                            {
                                results.Add(token.ToObject<T>());
                            }
                        }

                        if (usePageCursor && result.ContainsKey("metadata"))
                        {
                            JObject metadata = (JObject)result["metadata"];
                            if (metadata.TryGetValue("after", out JToken after) && after != null)
                            {
                                afterCursor = after.ToString();
                            }
                        }
                    }
                } while (!string.IsNullOrEmpty(afterCursor));
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
        }
    }
}
