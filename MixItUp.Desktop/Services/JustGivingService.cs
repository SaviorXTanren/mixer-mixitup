using MixItUp.Base.Services;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class JustGivingService : OAuthServiceBase, IJustGivingService, IDisposable
    {
        public const string ClientID = "1e30b383";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public JustGivingService() : base(string.Empty) { }

        public JustGivingService(OAuthTokenModel token) : base(string.Empty, token) { }

        public async Task<bool> Connect()
        {
            try
            {
                //var client = new JustGiving.Api.Sdk.JustGivingClient(ClientID);
                //var page = client.OneSearch.OneSearchIndex("Utopia's 24 Hour Charity Stream");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        public Task Disconnect()
        {
            this.token = null;
            this.cancellationTokenSource.Cancel();
            return Task.FromResult(0);
        }

        protected override Task RefreshOAuthToken()
        {
            return Task.FromResult(0);
        }

        private async Task<bool> InitializeInternal()
        {
            this.cancellationTokenSource = new CancellationTokenSource();

            if (null != null)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(this.BackgroundDonationCheck, this.cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                return true;
            }
            return false;
        }

        private Task BackgroundDonationCheck()
        {
            return Task.FromResult(0);
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
