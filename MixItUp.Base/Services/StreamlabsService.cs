using Mixer.Base;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Util;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class StreamlabsDonation
    {
        public int donation_id { get; set; }

        public string name { get; set; }
        public string message { get; set; }
        public string email { get; set; }

        public double amount { get; set; }
        public string currency { get; set; }

        public long created_at { get; set; }

        public UserDonationViewModel ToGenericDonation()
        {
            return new UserDonationViewModel()
            {
                ID = this.donation_id.ToString(),
                Username = this.name,
                Message = this.message,

                Amount = this.amount,
                CurrencyName = this.currency,

                DateTime = DateTimeHelper.UnixTimestampToDateTimeOffset(this.created_at),
            };
        }
    }

    [DataContract]
    public class StreamlabsService : OAuthServiceBase
    {
        private const string BaseAddress = "https://streamlabs.com/api/v1.0/";

        private const string ClientID = "ioEmsqlMK8jj0NuJGvvQn4ijp8XkyJ552VJ7MiDX";
        private const string ClientSecret = "ddcgpzlzeRAE7dAUvs87DJgAIQJSwiq5p4AkufWN";
        private const string AuthorizationUrl = "https://www.streamlabs.com/api/v1.0/authorize?client_id=ioEmsqlMK8jj0NuJGvvQn4ijp8XkyJ552VJ7MiDX&redirect_uri=http://localhost:8919/&response_type=code&scope=donations.read+socket.token+points.read";

        public event EventHandler<UserDonationViewModel> OnDonationReceivedOccurred;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public StreamlabsService() : base(StreamlabsService.BaseAddress) { }

        public StreamlabsService(OAuthTokenModel token) : base(StreamlabsService.BaseAddress, token) { }

        public async Task<bool> Connect()
        {
            if (this.token != null)
            {
                try
                {
                    await this.RefreshOAuthToken();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(this.BackgroundDonationCheck, this.cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    return true;
                }
                catch (Exception ex) { Logger.Log(ex); }
            }

            string authorizationCode = await this.ConnectViaOAuthRedirect(StreamlabsService.AuthorizationUrl);
            if (!string.IsNullOrEmpty(authorizationCode))
            {
                JObject payload = new JObject();
                payload["grant_type"] = "authorization_code";
                payload["client_id"] = StreamlabsService.ClientID;
                payload["client_secret"] = StreamlabsService.ClientSecret;
                payload["code"] = authorizationCode;
                payload["redirect_uri"] = MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL;

                this.token = await this.PostAsync<OAuthTokenModel>("token", this.CreateContentFromObject(payload));
                if (this.token != null)
                {
                    token.authorizationCode = authorizationCode;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(this.BackgroundDonationCheck, this.cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    return true;
                }
            }

            return false;
        }

        public Task Disconnect()
        {
            this.token = null;
            this.cancellationTokenSource.Cancel();
            return Task.FromResult(0);
        }

        public async Task<IEnumerable<StreamlabsDonation>> GetDonations()
        {
            List<StreamlabsDonation> results = new List<StreamlabsDonation>();

            HttpResponseMessage result = await this.GetAsync("donations");
            string resultJson = await result.Content.ReadAsStringAsync();
            JObject jobj = JObject.Parse(resultJson);
            JArray donations = (JArray)jobj["data"];
            foreach (var donation in donations)
            {
                results.Add(donation.ToObject<StreamlabsDonation>());
            }

            return results;
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                JObject payload = new JObject();
                payload["grant_type"] = "refresh_token";
                payload["client_id"] = StreamlabsService.ClientID;
                payload["client_secret"] = StreamlabsService.ClientSecret;
                payload["refresh_token"] = this.token.refreshToken;
                payload["redirect_uri"] = MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL;

                this.token = await this.PostAsync<OAuthTokenModel>("token", this.CreateContentFromObject(payload));
            }
        }

        private async Task BackgroundDonationCheck()
        {
            long lastCheckUnixTimestamp = DateTimeHelper.DateTimeOffsetToUnixTimestamp(DateTimeOffset.Now);
            Dictionary<int, StreamlabsDonation> donationsReceived = new Dictionary<int, StreamlabsDonation>();
            while (!this.cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    IEnumerable<StreamlabsDonation> donations = await this.GetDonations();
                    donations = donations.Where(d => d.created_at >= lastCheckUnixTimestamp);

                    foreach (StreamlabsDonation donation in donations)
                    {
                        if (!donationsReceived.ContainsKey(donation.donation_id))
                        {
                            donationsReceived[donation.donation_id] = donation;
                            if (this.OnDonationReceivedOccurred != null)
                            {
                                this.OnDonationReceivedOccurred(this, donation.ToGenericDonation());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                await Task.Delay(10000);
            }
        }
    }
}
