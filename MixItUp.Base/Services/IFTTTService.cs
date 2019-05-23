using Mixer.Base.Model.OAuth;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IIFTTTService
    {
        OAuthTokenModel Token { get; }

        Task SendTrigger(string eventName, Dictionary<string, string> values);
    }

    public class IFTTTService : IIFTTTService
    {
        private const string WebHookURLFormat = "https://maker.ifttt.com/trigger/{0}/with/key/{1}";

        private string key;

        public OAuthTokenModel Token { get { return new OAuthTokenModel() { accessToken = this.key }; } }

        public IFTTTService(OAuthTokenModel token) : this(token.accessToken) { }

        public IFTTTService(string key)
        {
            this.key = key;
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

                    HttpResponseMessage response = await client.PostAsync(string.Format(WebHookURLFormat, eventName, this.key), content);
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
    }
}
