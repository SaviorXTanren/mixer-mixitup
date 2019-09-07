using Mixer.Base;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class StreamlabsService : OAuthServiceBase, IStreamlabsService, IDisposable
    {
        private const string BaseAddress = "https://streamlabs.com/api/v1.0/";

        private const string ClientID = "ioEmsqlMK8jj0NuJGvvQn4ijp8XkyJ552VJ7MiDX";
        private const string AuthorizationUrl = "https://www.streamlabs.com/api/v1.0/authorize?client_id={0}&redirect_uri=http://localhost:8919/&response_type=code&scope=donations.read+socket.token+points.read+alerts.create+jar.write+wheel.write+credits.write";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private DateTimeOffset startTime;

        public StreamlabsService() : base(StreamlabsService.BaseAddress) { }

        public StreamlabsService(OAuthTokenModel token) : base(StreamlabsService.BaseAddress, token) { }

        public async Task<bool> Connect()
        {
            if (this.token != null)
            {
                try
                {
                    await this.RefreshOAuthToken();

                    if (await this.InitializeInternal())
                    {
                        return true;
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }
            }

            string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(StreamlabsService.AuthorizationUrl, StreamlabsService.ClientID));
            if (!string.IsNullOrEmpty(authorizationCode))
            {
                JObject payload = new JObject();
                payload["grant_type"] = "authorization_code";
                payload["client_id"] = StreamlabsService.ClientID;
                payload["client_secret"] = ChannelSession.SecretManager.GetSecret("StreamlabsSecret");
                payload["code"] = authorizationCode;
                payload["redirect_uri"] = MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL;

                this.token = await this.PostAsync<OAuthTokenModel>("token", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
                if (this.token != null)
                {
                    token.authorizationCode = authorizationCode;

                    return await this.InitializeInternal();
                }
            }

            return false;
        }

        public Task Disconnect()
        {
            this.cancellationTokenSource.Cancel();
            this.token = null;
            return Task.FromResult(0);
        }

        public async Task<IEnumerable<StreamlabsDonation>> GetDonations(int maxAmount = 1)
        {
            List<StreamlabsDonation> results = new List<StreamlabsDonation>();
            try
            {
                int lastID = 0;
                while (results.Count < maxAmount)
                {
                    string beforeFilter = string.Empty;
                    if (lastID > 0)
                    {
                        beforeFilter = "?before=" + lastID;
                    }

                    HttpResponseMessage response = await this.GetAsync("donations" + beforeFilter);
                    JObject jobj = await response.ProcessJObjectResponse();
                    JArray data = (JArray)jobj["data"];

                    if (data.Count == 0)
                    {
                        break;
                    }

                    foreach (var d in data)
                    {
                        StreamlabsDonation donation = d.ToObject<StreamlabsDonation>();
                        lastID = donation.ID;
                        results.Add(donation);
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
        }

        public async Task SpinWheel()
        {
            await this.PostAsync("wheel/spin", new StringContent($"access_token={this.token.accessToken}"));
        }

        public async Task EmptyJar()
        {
            await this.PostAsync("jar/empty", new StringContent($"access_token={this.token.accessToken}"));
        }

        public async Task RollCredits()
        {
            await this.PostAsync("credits/roll", new StringContent($"access_token={this.token.accessToken}"));
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                JObject payload = new JObject();
                payload["grant_type"] = "refresh_token";
                payload["client_id"] = StreamlabsService.ClientID;
                payload["client_secret"] = ChannelSession.SecretManager.GetSecret("StreamlabsSecret");
                payload["refresh_token"] = this.token.refreshToken;
                payload["redirect_uri"] = MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL;

                this.token = await this.PostAsync<OAuthTokenModel>("token", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
            }
        }

        private Task<bool> InitializeInternal()
        {
            this.cancellationTokenSource = new CancellationTokenSource();

            this.startTime = DateTimeOffset.Now;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(this.BackgroundDonationCheck, this.cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return Task.FromResult(true);
        }

        private async Task BackgroundDonationCheck()
        {
            Dictionary<int, StreamlabsDonation> donationsReceived = new Dictionary<int, StreamlabsDonation>();
            while (!this.cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    foreach (StreamlabsDonation slDonation in await this.GetDonations())
                    {
                        if (!donationsReceived.ContainsKey(slDonation.ID))
                        {
                            donationsReceived[slDonation.ID] = slDonation;
                            UserDonationModel donation = slDonation.ToGenericDonation();
                            if (donation.DateTime > this.startTime)
                            {
                                await EventCommand.ProcessDonationEventCommand(donation, OtherEventTypeEnum.StreamlabsDonation);
                            }
                        }
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }

                await Task.Delay(10000);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.cancellationTokenSource.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
