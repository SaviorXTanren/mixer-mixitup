using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class SAMMIService : IExternalService
    {
        public string Name { get { return Resources.SAMMI; } }

        public bool IsEnabled { get { return ChannelSession.Settings.EnableSAMMI; } }

        public bool IsConnected { get; private set; }

        public int SAMMIPortNumber { get { return ChannelSession.Settings.SAMMIPortNumber; } }

        public string BaseAddressURL { get { return $"http://localhost:{this.SAMMIPortNumber}/api"; } }

        public async Task<Result> Connect()
        {
            try
            {
                System.Net.ServicePointManager.Expect100Continue = false;

                using (var client = new AdvancedHttpClient())
                {
                    JObject result = await client.GetJObjectAsync($"{this.BaseAddressURL}?request=getVariable&name=Architecture");
                    if (result != null && result.ContainsKey("data"))
                    {
                        this.IsConnected = true;
                        ServiceManager.Get<ITelemetryService>().TrackService("SAMMI");
                        return new Result();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new Result(Resources.SAMMIFailedToConnect);
        }

        public Task Disconnect()
        {
            this.IsConnected = false;
            return Task.CompletedTask;
        }

        public async Task TriggerButton(string buttonID)
        {
            try
            {
                using (var client = new AdvancedHttpClient())
                {
                    JObject jobj = new JObject();
                    jobj["request"] = "triggerButton";
                    jobj["buttonID"] = buttonID;

                    HttpResponseMessage response = await client.PostAsync(this.BaseAddressURL, AdvancedHttpClient.CreateContentFromObject(jobj));
                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.Log("SAMMI Failure: " + await response.Content.ReadAsStringAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task ReleaseButton(string buttonID)
        {
            try
            {
                using (var client = new AdvancedHttpClient())
                {
                    JObject jobj = new JObject();
                    jobj["request"] = "releaseButton";
                    jobj["buttonID"] = buttonID;

                    HttpResponseMessage response = await client.PostAsync(this.BaseAddressURL, AdvancedHttpClient.CreateContentFromObject(jobj));
                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.Log("SAMMI Failure: " + await response.Content.ReadAsStringAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task SetVariable(string name, string value, string buttonID = null)
        {
            try
            {
                using (var client = new AdvancedHttpClient())
                {
                    JObject jobj = new JObject();
                    jobj["request"] = "setVariable";
                    jobj["name"] = name;
                    jobj["value"] = value;
                    if (!string.IsNullOrEmpty(buttonID))
                    {
                        jobj["buttonID"] = buttonID;
                    }

                    HttpResponseMessage response = await client.PostAsync(this.BaseAddressURL, AdvancedHttpClient.CreateContentFromObject(jobj));
                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.Log("SAMMI Failure: " + await response.Content.ReadAsStringAsync());
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
