using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class IFTTTService : OAuthExternalServiceBase
    {
        private const string WebHookURLFormat = "https://maker.ifttt.com/trigger/{0}/with/key/{1}";

        public IFTTTService() : base("") { }

        public override string Name { get { return MixItUp.Base.Resources.IFTTT; } }

        public override async Task<Result> Connect(OAuthTokenModel token)
        {
            Result result = await base.Connect(token);
            if (result.Success)
            {
                ServiceManager.Get<ITelemetryService>().TrackService("IFTTT");
            }
            return result;
        }

        public override Task<Result> Connect()
        {
            return Task.FromResult(new Result(false));
        }

        public override Task Disconnect()
        {
            this.token = null;
            return Task.CompletedTask;
        }

        public async Task SendTrigger(string eventName, Dictionary<string, string> values)
        {
            for (int i = 0; i < 3; i++) // Retry logic in case we fail due to a timeout issue
            {
                try
                {
                    using (AdvancedHttpClient client = new AdvancedHttpClient())
                    {
                        JObject jobj = new JObject();
                        foreach (var kvp in values)
                        {
                            jobj[kvp.Key] = kvp.Value;
                        }
                        HttpContent content = new StringContent(JSONSerializerHelper.SerializeToString(jobj), Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.PostAsync(string.Format(WebHookURLFormat, eventName, this.token.accessToken), content);

                        if (response.IsSuccessStatusCode)
                        {
                            return;
                        }
                        else
                        {
                            Logger.Log(await response.Content.ReadAsStringAsync());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

                await Task.Delay(1000);
            }
        }

        protected override Task<Result> InitializeInternal()
        {
            return Task.FromResult(new Result());
        }

        protected override Task RefreshOAuthToken()
        {
            return Task.CompletedTask;
        }
    }
}
