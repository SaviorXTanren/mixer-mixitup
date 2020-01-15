using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public interface IIFTTTService : IOAuthExternalService
    {
        Task SendTrigger(string eventName, Dictionary<string, string> values);
    }

    public class IFTTTService : OAuthExternalServiceBase, IIFTTTService
    {
        private const string WebHookURLFormat = "https://maker.ifttt.com/trigger/{0}/with/key/{1}";

        public IFTTTService() : base("") { }

        public override string Name { get { return "IFTTT"; } }

        public override Task<ExternalServiceResult> Connect()
        {
            return Task.FromResult(new ExternalServiceResult(false));
        }

        public override Task Disconnect()
        {
            return Task.FromResult(0);
        }

        public async Task SendTrigger(string eventName, Dictionary<string, string> values)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    JObject jobj = new JObject();
                    foreach (var kvp in values)
                    {
                        jobj[kvp.Key] = kvp.Value;
                    }
                    HttpContent content = new StringContent(SerializerHelper.SerializeToString(jobj), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(string.Format(WebHookURLFormat, eventName, this.token.accessToken), content);
                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.Log(await response.Content.ReadAsStringAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        protected override Task<ExternalServiceResult> InitializeInternal()
        {
            return Task.FromResult(new ExternalServiceResult());
        }

        protected override Task RefreshOAuthToken()
        {
            return Task.FromResult(0);
        }
    }
}
