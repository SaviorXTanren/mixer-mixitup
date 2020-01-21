using MixItUp.Base.Commands;
using MixItUp.Base.Model.API;
using MixItUp.Base.Model.Store;
using MixItUp.Base.Util;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IMixItUpService
    {
        Task RefreshOAuthToken();

        Task<MixItUpUpdateModel> GetLatestUpdate();
        Task<MixItUpUpdateModel> GetLatestPreviewUpdate();

        Task SendUserFeatureEvent(UserFeatureEvent feature);
        Task SendIssueReport(IssueReportModel report);

        Task<StoreDetailListingModel> GetStoreListing(Guid ID);
        Task AddStoreListing(StoreDetailListingModel listing);
        Task UpdateStoreListing(StoreDetailListingModel listing);
        Task DeleteStoreListing(Guid ID);

        Task<IEnumerable<StoreListingModel>> GetTopStoreListingsForTag(string tag);
        Task<StoreListingModel> GetTopRandomStoreListings();

        Task<IEnumerable<StoreListingModel>> SearchStoreListings(string search);

        Task AddStoreReview(StoreListingReviewModel review);
        Task UpdateStoreReview(StoreListingReviewModel review);

        Task AddStoreListingDownload(StoreListingModel listing);
        Task AddStoreListingUses(StoreListingUsesModel uses);
        Task AddStoreListingReport(StoreListingReportModel report);
    }

    public class MixItUpService : IMixItUpService, IDisposable
    {
        public const string MixItUpAPIEndpoint = "https://mixitupapi.azurewebsites.net/api/";
        //public const string MixItUpAPIEndpoint = "http://localhost:33901/api/"; // Dev Endpoint

        private OAuthTokenModel token = null;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public MixItUpService()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(this.BackgroundCommandUsesUpload, this.cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed            
        }

        public async Task RefreshOAuthToken()
        {
            if (ChannelSession.MixerUser != null)
            {
                this.token = await this.GetAsync<OAuthTokenModel>("authentication?userID=" + ChannelSession.MixerUser.id);
            }
        }

        public async Task<MixItUpUpdateModel> GetLatestUpdate() { return await this.GetAsync<MixItUpUpdateModel>("updates"); }
        public async Task<MixItUpUpdateModel> GetLatestPreviewUpdate() { return await this.GetAsync<MixItUpUpdateModel>("updates/preview"); }

        public async Task SendUserFeatureEvent(UserFeatureEvent feature)
        {
            if (!ChannelSession.Settings.OptOutTracking && ChannelSession.Settings.FeatureMe)
            {
                await this.PostAsync("userfeature", feature);
            }
        }

        public async Task SendIssueReport(IssueReportModel report) { await this.PostAsync("issuereport", report); }

        public async Task<StoreDetailListingModel> GetStoreListing(Guid ID)
        {
            StoreDetailListingModel listing = await this.GetAsync<StoreDetailListingModel>("store/details?id=" + ID);
            if (listing != null)
            {
                await listing.SetUser();
                await listing.SetReviewUsers();
            }
            return listing;
        }
        public async Task AddStoreListing(StoreDetailListingModel listing) { await this.PostAsync("store/details", listing); }
        public async Task UpdateStoreListing(StoreDetailListingModel listing) { await this.PutAsync("store/details", listing); }
        public async Task DeleteStoreListing(Guid ID) { await this.DeleteAsync("store/details?id=" + ID); }

        public async Task<IEnumerable<StoreListingModel>> GetTopStoreListingsForTag(string tag)
        {
            IEnumerable<StoreListingModel> listings = await this.GetAsync<IEnumerable<StoreListingModel>>("store/top?tag=" + tag);
            await this.SetStoreListingUsers(listings);
            return listings;
        }
        public async Task<StoreListingModel> GetTopRandomStoreListings()
        {
            IEnumerable<StoreListingModel> listings = await this.GetAsync<IEnumerable<StoreListingModel>>("store/top");
            if (listings != null && listings.Count() > 0)
            {
                StoreListingModel listing = listings.FirstOrDefault();
                await listing.SetUser();
                return listing;
            }
            return null;
        }

        public async Task<IEnumerable<StoreListingModel>> SearchStoreListings(string search)
        {
            IEnumerable<StoreListingModel> listings = await this.PostAsyncWithResult<IEnumerable<StoreListingModel>>("store/search?search=", search);
            await this.SetStoreListingUsers(listings);
            return listings;
        }

        public async Task AddStoreReview(StoreListingReviewModel review) { await this.PostAsync("store/reviews", review); }
        public async Task UpdateStoreReview(StoreListingReviewModel review) { await this.PutAsync("store/reviews", review); }

        public async Task AddStoreListingDownload(StoreListingModel listing) { await this.PostAsync("store/metadata/downloads", listing.ID); }
        public async Task AddStoreListingUses(StoreListingUsesModel uses)
        {
            if (!ChannelSession.Settings.OptOutTracking)
            {
                await this.PostAsync("store/metadata/uses", uses);
            }
        }
        public async Task AddStoreListingReport(StoreListingReportModel report) { await this.PostAsync("store/metadata/reports", report); }

        private async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(MixItUpAPIEndpoint))
                {
                    HttpResponseMessage response = await client.GetAsync(endpoint);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        return SerializerHelper.DeserializeFromString<T>(content);
                    }
                    else
                    {
                        await this.ProcessResponseIfError(response);
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return default(T);
        }

        private async Task PostAsync(string endpoint, object data, bool logException = true)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(MixItUpAPIEndpoint))
                {
                    string content = SerializerHelper.SerializeToString(data);
                    HttpResponseMessage response = await client.PostAsync(endpoint, new StringContent(content, Encoding.UTF8, "application/json"));
                    await this.ProcessResponseIfError(response);
                }
            }
            catch (Exception ex)
            {
                if (logException)
                {
                    Logger.Log(ex);
                }
            }
        }

        private async Task<T> PostAsyncWithResult<T>(string endpoint, object data)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(MixItUpAPIEndpoint))
                {
                    string content = SerializerHelper.SerializeToString(data);
                    HttpResponseMessage response = await client.PostAsync(endpoint, new StringContent(content, Encoding.UTF8, "application/json"));
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string resultContent = await response.Content.ReadAsStringAsync();
                        return SerializerHelper.DeserializeFromString<T>(resultContent);
                    }
                    else
                    {
                        await this.ProcessResponseIfError(response);
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return default(T);
        }

        private async Task PutAsync(string endpoint, object data)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(MixItUpAPIEndpoint))
                {
                    string content = SerializerHelper.SerializeToString(data);
                    HttpResponseMessage response = await client.PutAsync(endpoint, new StringContent(content, Encoding.UTF8, "application/json"));
                    await this.ProcessResponseIfError(response);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async Task PatchAsync(string endpoint, object data)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(MixItUpAPIEndpoint))
                {
                    string content = SerializerHelper.SerializeToString(data);
                    HttpRequestMessage message = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint);
                    message.Content = new StringContent(content, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.SendAsync(message);
                    await this.ProcessResponseIfError(response);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async Task DeleteAsync(string endpoint)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(MixItUpAPIEndpoint))
                {
                    await client.DeleteAsync(endpoint);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async Task SetStoreListingUsers(IEnumerable<StoreListingModel> listings)
        {
            if (listings != null)
            {
                foreach (StoreListingModel listing in listings)
                {
                    await listing.SetUser();
                }
            }
        }

        private async Task BackgroundCommandUsesUpload()
        {
            while (!this.cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    Dictionary<Guid, long> commandUses = CommandBase.GetCommandUses();
                    foreach (var kvp in commandUses)
                    {
                        await this.AddStoreListingUses(new StoreListingUsesModel() { ID = kvp.Key, Uses = kvp.Value });
                        await Task.Delay(2000);
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }

                await Task.Delay(60000);
            }
        }

        private async Task ProcessResponseIfError(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string resultContent = await response.Content.ReadAsStringAsync();
                Logger.Log(resultContent);
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
