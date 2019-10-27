using Mixer.Base;
using MixItUp.Base;
using MixItUp.Base.Services;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class JustGivingService : OAuthServiceBase, IJustGivingService, IDisposable
    {
        private const string BaseAddress = "https://api.justgiving.com/v1/";

        private const string ClientID = "1e30b383";
        private const string AuthorizationUrl = "https://identity.justgiving.com/connect/authorize?client_id={0}&response_type=code&scope=openid+profile+email+account+fundraise+offline_access&redirect_uri=http%3A%2F%2Flocalhost%3A8919%2F&nonce=ba3c9a58dff94a86aa633e71e6afc4e3";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private DateTimeOffset startTime;

        public JustGivingService() : base(JustGivingService.BaseAddress) { }

        public JustGivingService(OAuthTokenModel token) : base(JustGivingService.BaseAddress, token) { }

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

            string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(JustGivingService.AuthorizationUrl, JustGivingService.ClientID));
            if (!string.IsNullOrEmpty(authorizationCode))
            {
                JObject payload = new JObject();
                payload["grant_type"] = "authorization_code";
                payload["code"] = authorizationCode;
                payload["redirect_uri"] = MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL;

                using (AdvancedHttpClient client = await this.GetHttpClient(autoRefreshToken: false))
                {
                    client.BaseAddress = new Uri("https://identity.justgiving.com");
                    this.token = await this.PostAsync<OAuthTokenModel>("connect/token", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
                    if (this.token != null)
                    {
                        token.authorizationCode = authorizationCode;
                        return await this.InitializeInternal();
                    }
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

        public async Task GetCurrentAccount()
        {
            try
            {
                JObject jobj = await this.GetJObjectAsync("account");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                JObject payload = new JObject();
                payload["grant_type"] = "refresh_token";
                payload["refresh_token"] = this.token.refreshToken;
                payload["redirect_uri"] = MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL;

                using (AdvancedHttpClient client = await this.GetHttpClient(autoRefreshToken: false))
                {
                    client.BaseAddress = new Uri("https://identity.justgiving.com");
                    this.token = await this.PostAsync<OAuthTokenModel>("connect/token", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
                }
            }
        }

        protected override async Task<AdvancedHttpClient> GetHttpClient(bool autoRefreshToken = true)
        {
            AdvancedHttpClient client = await base.GetHttpClient(autoRefreshToken);
            client.SetBasicClientIDClientSecretAuthorizationHeader(JustGivingService.ClientID, ChannelSession.SecretManager.GetSecret("JustGivingSecret"));
            return client;
        }

        private async Task<bool> InitializeInternal()
        {
            this.cancellationTokenSource = new CancellationTokenSource();

            this.startTime = DateTimeOffset.Now;

            await this.GetCurrentAccount();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(this.BackgroundDonationCheck, this.cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return false;
        }

        private async Task BackgroundDonationCheck()
        {
            Dictionary<int, StreamlabsDonation> donationsReceived = new Dictionary<int, StreamlabsDonation>();
            while (!this.cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    //foreach (StreamlabsDonation slDonation in await this.GetDonations())
                    //{
                    //    if (!donationsReceived.ContainsKey(slDonation.ID))
                    //    {
                    //        donationsReceived[slDonation.ID] = slDonation;
                    //        UserDonationModel donation = slDonation.ToGenericDonation();
                    //        if (donation.DateTime > this.startTime)
                    //        {
                    //            await EventCommand.ProcessDonationEventCommand(donation, OtherEventTypeEnum.JustGivingDonation);
                    //        }
                    //    }
                    //}
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
