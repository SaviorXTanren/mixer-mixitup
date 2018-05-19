using Mixer.Base.Web;
using MixItUp.Base.Model.API;
using MixItUp.Base.Util;
using System;
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
    }

    public class MixItUpService : IMixItUpService
    {
        public const string MixItUpAPIEndpoint = "https://mixitupapi.azurewebsites.net/api/";

        public async Task<MixItUpUpdateModel> GetLatestUpdate() { return await this.GetAsync<MixItUpUpdateModel>("updates"); }

        public async Task SendLoginEvent(LoginEvent login) { await this.PostAsync("login", login); }

        public async Task SendErrorEvent(ErrorEvent error) { await this.PostAsync("error", error); }

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
    }
}
