using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class LumiaStreamSettings
    {
        public LumiaStreamSettingsOptions options { get; set; }

        public List<LumiaStreamLight> lights { get; set; } = new List<LumiaStreamLight>();
    }

    public class LumiaStreamSettingsOptions
    {
        [JsonProperty("chat-command")]
        public LumiaStreamSettingsOptionsValue chatCommands { get; set; }

        [JsonProperty("trovo-spells")]
        public LumiaStreamSettingsOptionsValue trovoSpells { get; set; }

        [JsonProperty("twitch-extension")]
        public LumiaStreamSettingsOptionsValue twitchExtension { get; set; }

        [JsonProperty("twitch-points")]
        public LumiaStreamSettingsOptionsValue twitchPoints { get; set; }
    }

    public class LumiaStreamSettingsOptionsValue
    {
        public List<string> values { get; set; } = new List<string>();
    }

    public class LumiaStreamLight
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(name))
            {
                return this.name;
            }
            else
            {
                return this.type;
            }
        }
    }

    public class LumiaStreamService : IExternalService
    {
        private const string BaseAddress = "http://localhost:39231/api/";

        private LumiaStreamSettings settingsCached;
        private DateTimeOffset lastSettingsCached = DateTimeOffset.MinValue;

        public LumiaStreamService() { }

        public string Name { get { return MixItUp.Base.Resources.LumiaStream; } }

        public bool IsEnabled { get { return ChannelSession.Settings.LumiaStreamOAuthToken != null; } }

        public bool IsConnected { get; private set; }

        public async Task<Result> Connect()
        {
            this.IsConnected = false;
            LumiaStreamSettings settings = await this.GetSettings();
            if (settings != null)
            {
                ServiceManager.Get<ITelemetryService>().TrackService("Lumia Stream");
                this.IsConnected = true;
                return new Result();
            }
            return new Result(Resources.LumiaStreamConnectionFailed);
        }

        public async Task<LumiaStreamSettings> GetSettings()
        {
            if (this.settingsCached != null && this.lastSettingsCached.TotalMinutesFromNow() > 5)
            {
                return this.settingsCached;
            }

            string results = await this.SendGetRequest("retrieve");
            if (!string.IsNullOrEmpty(results))
            {
                JObject jobj = JObject.Parse(results);
                if (jobj != null && jobj.TryGetValue("status", out JToken value) && value != null && string.Equals(value.ToString(), "200") && jobj.ContainsKey("data"))
                {
                    this.settingsCached = jobj["data"].ToObject<LumiaStreamSettings>();
                    return this.settingsCached;
                }
            }
            return null;
        }

        public async Task TriggerCommand(string type, string name)
        {
            JObject jobj = new JObject();
            jobj["type"] = type;
            jobj["params"] = new JObject()
            {
                { "value", name }
            };
            await this.SendPostRequest("send", jobj);
        }

        public async Task SetColorAndBrightness(string color, int brightness, int transition, int duration, bool hold)
        {
            JObject jobj = new JObject();
            jobj["type"] = "hex-color";
            jobj["params"] = new JObject()
            {
                { "value", color },
                { "brightness", brightness },
                { "transition", transition },
                { "duration", duration },
            };

            if (hold)
            {
                jobj["params"]["hold"] = true;
            }

            await this.SendPostRequest("send", jobj);
        }

        private async Task<string> SendGetRequest(string request)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(LumiaStreamService.BaseAddress))
                {
                    return await client.GetStringAsync($"{request}?token={ChannelSession.Settings.LumiaStreamOAuthToken?.accessToken}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        private async Task SendPostRequest(string request, JObject body)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(LumiaStreamService.BaseAddress))
                {
                    await client.PostAsync($"{request}?token={ChannelSession.Settings.LumiaStreamOAuthToken?.accessToken}", AdvancedHttpClient.CreateContentFromObject(body));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public Task Disconnect()
        {
            this.IsConnected = false;
            return Task.CompletedTask;
        }
    }
}
