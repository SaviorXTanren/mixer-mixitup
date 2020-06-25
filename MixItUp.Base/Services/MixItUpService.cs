using MixItUp.Base.Model.API;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IMixItUpService
    {
        Task<MixItUpUpdateModel> GetLatestUpdate();
        Task<MixItUpUpdateModel> GetLatestPreviewUpdate();
        Task<MixItUpUpdateModel> GetLatestTestUpdate();

        Task SendUserFeatureEvent(UserFeatureEvent feature);
        Task SendIssueReport(IssueReportModel report);
    }

    public class MixItUpService : IMixItUpService, IDisposable
    {
        public const string MixItUpAPIEndpoint = "https://mixitupapi.azurewebsites.net/api/";
        //public const string MixItUpAPIEndpoint = "http://localhost:33901/api/"; // Dev Endpoint

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public MixItUpService() { }

        public async Task<MixItUpUpdateModel> GetLatestUpdate() { return await this.GetAsync<MixItUpUpdateModel>("updates"); }
        public async Task<MixItUpUpdateModel> GetLatestPreviewUpdate() { return await this.GetAsync<MixItUpUpdateModel>("updates/preview"); }
        public async Task<MixItUpUpdateModel> GetLatestTestUpdate() { return await this.GetAsync<MixItUpUpdateModel>("updates/test"); }

        public async Task SendUserFeatureEvent(UserFeatureEvent feature)
        {
            if (!ChannelSession.Settings.OptOutTracking && ChannelSession.Settings.FeatureMe)
            {
                await this.PostAsync("userfeature", feature);
            }
        }

        public async Task SendIssueReport(IssueReportModel report) { await this.PostAsync("issuereport", report); }

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
                        return JSONSerializerHelper.DeserializeFromString<T>(content);
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
                    string content = JSONSerializerHelper.SerializeToString(data);
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
                    string content = JSONSerializerHelper.SerializeToString(data);
                    HttpResponseMessage response = await client.PostAsync(endpoint, new StringContent(content, Encoding.UTF8, "application/json"));
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string resultContent = await response.Content.ReadAsStringAsync();
                        return JSONSerializerHelper.DeserializeFromString<T>(resultContent);
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
                    string content = JSONSerializerHelper.SerializeToString(data);
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
                    string content = JSONSerializerHelper.SerializeToString(data);
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
