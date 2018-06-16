using Mixer.Base.Web;
using MixItUp.Base.Model.API;
using MixItUp.Base.Model.Store;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IMixItUpService
    {
        Task<MixItUpUpdateModel> GetLatestUpdate();

        Task SendLoginEvent(LoginEvent login);
        Task SendErrorEvent(ErrorEvent error);

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
        Task AddStoreListingReport(StoreListingReportModel report);
    }

    public class MixItUpService : IMixItUpService
    {
        public const string MixItUpAPIEndpoint = "http://localhost:33901/api/";

        public async Task<MixItUpUpdateModel> GetLatestUpdate() { return await this.GetAsync<MixItUpUpdateModel>("updates"); }

        public async Task SendLoginEvent(LoginEvent login) { await this.PostAsync("login", login); }
        public async Task SendErrorEvent(ErrorEvent error) { await this.PostAsync("error", error); }

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

        public async Task AddStoreListingDownload(StoreListingModel listing) { await this.PatchAsync("store/metadata", listing.ID); }
        public async Task AddStoreListingReport(StoreListingReportModel report) { await this.PostAsync("store/metadata", report); }

        private async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                using (HttpClientWrapper client = new HttpClientWrapper(MixItUpAPIEndpoint))
                {
                    HttpResponseMessage response = await client.GetAsync(endpoint);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        return SerializerHelper.DeserializeFromString<T>(content);
                    }
                }
            }
            catch (Exception) { }
            return default(T);
        }

        private async Task PostAsync(string endpoint, object data)
        {
            try
            {
                using (HttpClientWrapper client = new HttpClientWrapper(MixItUpAPIEndpoint))
                {
                    string content = SerializerHelper.SerializeToString(data);
                    HttpResponseMessage response = await client.PostAsync(endpoint, new StringContent(content, Encoding.UTF8, "application/json"));
                }
            }
            catch (Exception) { }
        }

        private async Task<T> PostAsyncWithResult<T>(string endpoint, object data)
        {
            try
            {
                using (HttpClientWrapper client = new HttpClientWrapper(MixItUpAPIEndpoint))
                {
                    string content = SerializerHelper.SerializeToString(data);
                    HttpResponseMessage response = await client.PostAsync(endpoint, new StringContent(content, Encoding.UTF8, "application/json"));
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string resultContent = await response.Content.ReadAsStringAsync();
                        return SerializerHelper.DeserializeFromString<T>(resultContent);
                    }
                }
            }
            catch (Exception) { }
            return default(T);
        }

        private async Task PutAsync(string endpoint, object data)
        {
            try
            {
                using (HttpClientWrapper client = new HttpClientWrapper(MixItUpAPIEndpoint))
                {
                    string content = SerializerHelper.SerializeToString(data);
                    HttpResponseMessage response = await client.PutAsync(endpoint, new StringContent(content, Encoding.UTF8, "application/json"));
                }
            }
            catch (Exception) { }
        }

        private async Task PatchAsync(string endpoint, object data)
        {
            try
            {
                using (HttpClientWrapper client = new HttpClientWrapper(MixItUpAPIEndpoint))
                {
                    string content = SerializerHelper.SerializeToString(data);
                    HttpRequestMessage message = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint);
                    message.Content = new StringContent(content, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.SendAsync(message);
                }
            }
            catch (Exception) { }
        }

        private async Task DeleteAsync(string endpoint)
        {
            try
            {
                using (HttpClientWrapper client = new HttpClientWrapper(MixItUpAPIEndpoint))
                {
                    HttpResponseMessage response = await client.DeleteAsync(endpoint);
                }
            }
            catch (Exception) { }
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
    }
}
