using Mixer.Base;
using MixItUp.Base.Model.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using StreamingClient.Base.Util;

namespace MixItUp.Base.Services.External
{
    [DataContract]
    public class StreamElementsChannel
    {
        [DataMember]
        public string _id { get; set; }
        [DataMember]
        public string username { get; set; }
        [DataMember]
        public string alias { get; set; }
        [DataMember]
        public string displayName { get; set; }

        [DataMember]
        public string providerId { get; set; }
        [DataMember]
        public string provider { get; set; }

        [DataMember]
        public string createdAt { get; set; }
        [DataMember]
        public string updatedAt { get; set; }
    }

    [DataContract]
    public class StreamElementsDonation
    {
        [DataMember]
        public string _id { get; set; }
        [DataMember]
        public string provider { get; set; }
        [DataMember]
        public string channel { get; set; }

        [DataMember]
        public string status { get; set; }
        [DataMember]
        public string approved { get; set; }
        [DataMember]
        public bool deleted { get; set; }

        [DataMember]
        public string createdAt { get; set; }

        [DataMember]
        public StreamElementsDonationDetails donation { get; set; }

        [JsonIgnore]
        public bool IsApproved { get { return string.Equals(this.approved, "allowed"); } }

        [JsonIgnore]
        public DateTimeOffset CreatedDate { get { return new DateTimeOffset(DateTime.Parse(this.createdAt), new TimeSpan()); } }

        public UserDonationModel ToGenericDonation()
        {
            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.StreamElements,

                ID = this._id.ToString(),
                Username = "",
                Message = (this.donation != null) ? this.donation.message : string.Empty,

                Amount = Math.Round((this.donation != null) ? this.donation.amount : 0, 2),

                DateTime = this.CreatedDate,
            };
        }
    }

    [DataContract]
    public class StreamElementsDonationDetails
    {
        [DataMember]
        public StreamElementsDonationDetailsUser user { get; set; }

        [DataMember]
        public string message { get; set; }

        [DataMember]
        public double amount { get; set; }
        [DataMember]
        public string currency { get; set; }
    }

    [DataContract]
    public class StreamElementsDonationDetailsUser
    {
        [DataMember]
        public string username { get; set; }
        [DataMember]
        public string geo { get; set; }
    }

    public interface IStreamElementsService : IOAuthExternalService
    {
        Task<StreamElementsChannel> GetCurrentChannel();

        Task<IEnumerable<StreamElementsDonation>> GetDonations();
    }

    public class StreamElementsService : OAuthExternalServiceBase, IStreamElementsService
    {
        private const string BaseAddress = "https://api.streamelements.com/kappa/v2/";

        private const string ClientID = "460928647d5469dd";
        private const string AuthorizationUrl = "https://api.streamelements.com/oauth2/authorize?client_id={0}&redirect_uri=http://localhost:8919/&response_type=code&state={1}&scope=tips:read";
        private const string TokenUrl = "https://api.streamelements.com/oauth2/token";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private Dictionary<string, StreamElementsDonation> donationsReceived = new Dictionary<string, StreamElementsDonation>();

        private StreamElementsChannel channel;

        public StreamElementsService() : base(StreamElementsService.BaseAddress) { }

        public override string Name { get { return "StreamElements"; } }

        public override async Task<ExternalServiceResult> Connect()
        {
            try
            {
                string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(StreamElementsService.AuthorizationUrl, StreamElementsService.ClientID, Guid.NewGuid().ToString()));
                if (!string.IsNullOrEmpty(authorizationCode))
                {
                    string clientSecret = ChannelSession.Services.Secrets.GetSecret("StreamElementsSecret");

                    List<KeyValuePair<string, string>> bodyContent = new List<KeyValuePair<string, string>>();
                    bodyContent.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
                    bodyContent.Add(new KeyValuePair<string, string>("client_id", StreamElementsService.ClientID));
                    bodyContent.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
                    bodyContent.Add(new KeyValuePair<string, string>("code", authorizationCode));
                    bodyContent.Add(new KeyValuePair<string, string>("redirect_uri", MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL));

                    this.token = await this.GetWWWFormUrlEncodedOAuthToken(StreamElementsService.TokenUrl, StreamElementsService.ClientID, clientSecret, bodyContent);
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

        public async Task<StreamElementsChannel> GetCurrentChannel()
        {
            return await this.GetAsync<StreamElementsChannel>("channels/me");
        }

        public async Task<IEnumerable<StreamElementsDonation>> GetDonations()
        {
            JObject jobj = await this.GetJObjectAsync(string.Format("tips/{0}?limit=25&sort=-createdAt", this.channel._id));
            if (jobj != null && jobj.ContainsKey("docs"))
            {
                JArray jarray = (JArray)jobj["docs"];
                if (jarray != null && jarray.Count > 0)
                {
                    return jarray.ToTypedArray<StreamElementsDonation>();
                }
            }
            return new List<StreamElementsDonation>();
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                string clientSecret = ChannelSession.Services.Secrets.GetSecret("StreamElementsSecret");

                List<KeyValuePair<string, string>> bodyContent = new List<KeyValuePair<string, string>>();
                bodyContent.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
                bodyContent.Add(new KeyValuePair<string, string>("client_id", StreamElementsService.ClientID));
                bodyContent.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
                bodyContent.Add(new KeyValuePair<string, string>("refresh_token", this.token.refreshToken));
                bodyContent.Add(new KeyValuePair<string, string>("redirect_uri", MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL));

                this.token = await this.GetWWWFormUrlEncodedOAuthToken(StreamElementsService.TokenUrl, StreamElementsService.ClientID, clientSecret, bodyContent);
            }
        }

        protected override async Task<ExternalServiceResult> InitializeInternal()
        {
            this.channel = await this.GetCurrentChannel();
            if (this.channel != null)
            {
                foreach (StreamElementsDonation seDonation in await this.GetDonations())
                {
                    donationsReceived[seDonation._id] = seDonation;
                }

                MixItUp.Base.Util.AsyncRunner.RunBackgroundTask(this.cancellationTokenSource.Token, 30000, this.BackgroundDonationCheck);

                return new ExternalServiceResult();
            }
            return new ExternalServiceResult("Failed to get user information");
        }

        protected override async Task<AdvancedHttpClient> GetHttpClient(bool autoRefreshToken = true)
        {
            AdvancedHttpClient client = await base.GetHttpClient(autoRefreshToken);
            if (this.token != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", this.token.accessToken);
            }
            return client;
        }

        protected override void DisposeInternal()
        {
            this.cancellationTokenSource.Dispose();
        }

        private async Task BackgroundDonationCheck(CancellationToken token)
        {
            foreach (StreamElementsDonation seDonation in await this.GetDonations())
            {
                if (!donationsReceived.ContainsKey(seDonation._id))
                {
                    donationsReceived[seDonation._id] = seDonation;
                    UserDonationModel donation = seDonation.ToGenericDonation();
                    await ChannelSession.Services.Events.PerformEvent(await EventService.ProcessDonationEvent(EventTypeEnum.StreamElementsDonation, donation));
                }
            }
        }
    }
}
