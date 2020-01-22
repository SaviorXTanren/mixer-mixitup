using Mixer.Base;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class JustGivingUser
    {
        public Guid userId { get; set; }
        public uint accountId { get; set; }
        public JArray profileImageUrls { get; set; }
    }

    public class JustGivingFundraiser
    {
        public uint charityId { get; set; }
        public uint pageId { get; set; }
        public uint eventId { get; set; }

        public string pageTitle { get; set; }
        public string pageShortName { get; set; }
        public string eventName { get; set; }

        public string pageStatus { get; set; }

        public double raisedAmount { get; set; }
        public double targetAmount { get; set; }

        public string currencyCode { get; set; }
        public string currencySymbol { get; set; }

        public bool IsActive { get { return (!string.IsNullOrEmpty(this.pageStatus) && this.pageStatus.Equals("Active")); } }
    }

    public class JustGivingDonationGroup
    {
        public string id { get; set; }
        public string pageShortName { get; set; }
        public List<JustGivingDonation> donations { get; set; }
        public JObject pagination { get; set; }
    }

    public class JustGivingDonation
    {
        public uint id { get; set; }

        public string donorDisplayName { get; set; }
        public string donorRealName { get; set; }

        public string image { get; set; }
        public string message { get; set; }
        public string donationDate { get; set; }

        public string amount { get; set; }
        public string currencyCode { get; set; }
        public string donorLocalAmount { get; set; }
        public string donorLocalCurrencyCode { get; set; }

        public DateTimeOffset DateTime
        {
            get
            {
                if (!string.IsNullOrEmpty(this.donationDate))
                {
                    string date = this.donationDate.Replace("/Date(", "");
                    date = date.Substring(0, date.IndexOf("+"));
                    if (long.TryParse(date, out long dateLong))
                    {
                        return DateTimeOffset.FromUnixTimeMilliseconds(dateLong);
                    }
                }
                return DateTimeOffset.MinValue;
            }
        }

        public UserDonationModel ToGenericDonation()
        {
            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.JustGiving,

                ID = this.id.ToString(),
                Username = this.donorDisplayName,
                Message = this.message,

                Amount = double.Parse(this.donorLocalAmount),

                DateTime = this.DateTime,
            };
        }
    }

    public interface IJustGivingService : IOAuthExternalService
    {
        void SetFundraiser(JustGivingFundraiser fundraiser);

        Task<JustGivingUser> GetCurrentAccount();

        Task<IEnumerable<JustGivingFundraiser>> GetCurrentFundraisers();

        Task<IEnumerable<JustGivingDonation>> GetRecentDonations(JustGivingFundraiser fundraiser);
    }

    public class JustGivingService : OAuthExternalServiceBase, IJustGivingService, IDisposable
    {
        private const string BaseAddress = "https://api.justgiving.com/v1/";

        private const string ClientID = "1e30b383";
        private const string AuthorizationUrl = "https://identity.justgiving.com/connect/authorize?client_id={0}&response_type=code&scope=openid+profile+email+account+fundraise+offline_access&redirect_uri=http%3A%2F%2Flocalhost%3A8919%2F&nonce=ba3c9a58dff94a86aa633e71e6afc4e3";

        private JustGivingUser user;
        private JustGivingFundraiser fundraiser;
        private Dictionary<uint, JustGivingDonation> donationsReceived = new Dictionary<uint, JustGivingDonation>();

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private DateTimeOffset startTime;

        public JustGivingService() : base(JustGivingService.BaseAddress) { }

        public override string Name { get { return "JustGiving"; } }

        public override async Task<ExternalServiceResult> Connect()
        {
            try
            {
                string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(JustGivingService.AuthorizationUrl, JustGivingService.ClientID));
                if (!string.IsNullOrEmpty(authorizationCode))
                {
                    var body = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("redirect_uri", MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL),
                    new KeyValuePair<string, string>("code", authorizationCode),
                };
                    this.token = await this.GetWWWFormUrlEncodedOAuthToken("https://identity.justgiving.com/connect/token", JustGivingService.ClientID,
                        ChannelSession.Services.Secrets.GetSecret("JustGivingSecret"), body);
                    if (this.token != null)
                    {
                        token.authorizationCode = authorizationCode;
                        return await this.InitializeInternal();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new ExternalServiceResult(ex);
            }
            return new ExternalServiceResult(false);
        }

        public override Task Disconnect()
        {
            this.cancellationTokenSource.Cancel();
            this.token = null;
            return Task.FromResult(0);
        }

        public void SetFundraiser(JustGivingFundraiser fundraiser)
        {
            this.fundraiser = fundraiser;
        }

        public async Task<JustGivingUser> GetCurrentAccount()
        {
            try
            {
                return await this.GetAsync<JustGivingUser>("account");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public async Task<IEnumerable<JustGivingFundraiser>> GetCurrentFundraisers()
        {
            try
            {
                return await this.GetAsync<IEnumerable<JustGivingFundraiser>>("fundraising/pages");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new List<JustGivingFundraiser>();
        }

        public async Task<IEnumerable<JustGivingDonation>> GetRecentDonations(JustGivingFundraiser fundraiser)
        {
            try
            {
                JustGivingDonationGroup group = await this.GetAsync<JustGivingDonationGroup>($"fundraising/pages/{fundraiser.pageShortName}/donations");
                if (group != null && group.donations != null)
                {
                    return group.donations;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new List<JustGivingDonation>();
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                var body = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("redirect_uri", MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL),
                    new KeyValuePair<string, string>("refresh_token", this.token.refreshToken),
                };
                this.token = await this.GetWWWFormUrlEncodedOAuthToken("https://identity.justgiving.com/connect/token", JustGivingService.ClientID,
                    ChannelSession.Services.Secrets.GetSecret("JustGivingSecret"), body);
            }
        }

        protected override async Task<AdvancedHttpClient> GetHttpClient(bool autoRefreshToken = true)
        {
            AdvancedHttpClient client = await base.GetHttpClient(autoRefreshToken);
            client.DefaultRequestHeaders.Add("x-app-id", JustGivingService.ClientID);
            client.DefaultRequestHeaders.Add("x-application-key", ChannelSession.Services.Secrets.GetSecret("JustGivingSecret"));
            return client;
        }

        protected override async Task<ExternalServiceResult> InitializeInternal()
        {
            this.cancellationTokenSource = new CancellationTokenSource();

            this.startTime = DateTimeOffset.Now;

            this.user = await this.GetCurrentAccount();
            if (this.user != null)
            {
                if (!string.IsNullOrEmpty(ChannelSession.Settings.JustGivingPageShortName))
                {
                    IEnumerable<JustGivingFundraiser> fundraisers = await this.GetCurrentFundraisers();
                    this.fundraiser = fundraisers.FirstOrDefault(f => f.pageShortName.Equals(ChannelSession.Settings.JustGivingPageShortName));
                }

                AsyncRunner.RunBackgroundTask(this.cancellationTokenSource.Token, 30000, this.BackgroundDonationCheck);

                return new ExternalServiceResult();
            }
            return new ExternalServiceResult("Unable to get User data");
        }

        protected override void DisposeInternal()
        {
            this.cancellationTokenSource.Dispose();
        }

        private async Task BackgroundDonationCheck(CancellationToken token)
        {
            if (!token.IsCancellationRequested)
            {
                if (this.fundraiser != null)
                {
                    foreach (JustGivingDonation jgDonation in await this.GetRecentDonations(this.fundraiser))
                    {
                        if (!donationsReceived.ContainsKey(jgDonation.id))
                        {
                            donationsReceived[jgDonation.id] = jgDonation;
                            UserDonationModel donation = jgDonation.ToGenericDonation();
                            if (donation.DateTime > this.startTime)
                            {
                                await ChannelSession.Services.Events.PerformEvent(await EventService.ProcessDonationEvent(EventTypeEnum.JustGivingDonation, donation));

                            }
                        }
                    }
                }
            }
        }
    }
}
