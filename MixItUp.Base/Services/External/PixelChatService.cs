using MixItUp.Base.Model;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class PixelChatUserModel
    {
        public string username { get; set; }
        public List<string> linkedAccounts { get; set; } = new List<string>();
        public Dictionary<string, PixelChatUserAccountModel> accountInfo { get; set; } = new Dictionary<string, PixelChatUserAccountModel>();
    }

    public class PixelChatUserAccountModel
    {
        public string username { get; set; }
        public string userId { get; set; }
        public string channelId { get; set; }
    }

    public class PixelChatOverlayModel
    {
        public const string GiveawayOverlayType = "PixelGive";
        public const string CreditsOverlayType = "PixelCredits";
        public const string ShoutoutOverlayType = "PixelShout";
        public const string TimerOverlayType = "PixelTime";
        public const string StreamathonOverlayType = "PixelThon";

        public string id { get; set; }
        public string type { get; set; }
        public JObject settings { get; set; } = new JObject();

        public string Name
        {
            get
            {
                if (settings.TryGetValue("title", out JToken setting) && setting is JObject)
                {
                    if (((JObject)setting).TryGetValue("value", out JToken settingValue))
                    {
                        return settingValue.ToString();
                    }
                }
                return "Unknown";
            }
        }
    }

    public class PixelChatSceneModel
    {
        public string id { get; set; }
        public JObject settings { get; set; } = new JObject();
        public Dictionary<string, PixelChatSceneComponentModel> components { get; set; } = new Dictionary<string, PixelChatSceneComponentModel>();

        public string Name
        {
            get
            {
                if (settings.TryGetValue("title", out JToken setting))
                {
                    return setting.ToString();
                }
                return "Unknown";
            }
        }
    }

    public class PixelChatSceneComponentModel
    {
        public string type { get; set; }
        public bool hidden { get; set; }
        public JToken component { get; set; }
        public string olid { get; set; }

        public string ID { get; set; }
        public string Name { get; set; }
        public string OverlayID { get; set; }

        public void SetProperties()
        {
            if (string.Equals(this.type, "image") && this.component is JObject && ((JObject)this.component).TryGetValue("name", out JToken imageName))
            {
                this.Name = imageName.ToString();
            }
            else if (string.Equals(this.type, "url") && this.component != null)
            {
                this.Name = this.component.ToString();
            }
            else if (string.Equals(this.type, "overlay"))
            {
                if (this.component != null)
                {
                    this.OverlayID = this.component.ToString();
                }
                else
                {
                    this.OverlayID = this.olid;
                }
            }
        }
    }

    public class PixelChatSendMessageModel
    {
        public string type { get; set; }
        public object data { get; set; }

        public PixelChatSendMessageModel() { }

        public PixelChatSendMessageModel(string type)
        {
            this.type = type;
        }

        public PixelChatSendMessageModel(string type, int amount)
            : this(type)
        {
            this.data = amount;
        }

        public PixelChatSendMessageModel(string type, string username)
            : this(type)
        {
            this.data = new JObject()
            {
                { "username", username }
            };
        }

        public PixelChatSendMessageModel(string type, string username, StreamingPlatformTypeEnum platform)
            : this(type)
        {
            this.data = new JObject()
            {
                { "username", username },
                { "platform", platform.ToString().ToLower() }
            };
        }
    }

    /// <summary>
    /// https://docs.pixelchat.tv/
    /// </summary>
    public class PixelChatService : OAuthExternalServiceBase
    {
        private const string BaseAddress = "https://api2.pixelchat.tv/public/";

        public PixelChatService() : base(PixelChatService.BaseAddress) { }

        public override string Name { get { return MixItUp.Base.Resources.PixelChat; } }

        public override Task<Result> Connect()
        {
            this.TrackServiceTelemetry("PixelChat");
            return Task.FromResult(new Result(false));
        }

        public override Task Disconnect()
        {
            return Task.CompletedTask;
        }

        public async Task<PixelChatUserModel> GetUser()
        {
            try
            {
                return await this.GetAsync<PixelChatUserModel>("user");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public async Task<IEnumerable<PixelChatSceneModel>> GetScenes()
        {
            List<PixelChatSceneModel> scenes = new List<PixelChatSceneModel>();
            try
            {
                JObject dictionaryOverlays = await this.GetJObjectAsync("scenes");
                if (dictionaryOverlays != null)
                {
                    foreach (var kvp in dictionaryOverlays)
                    {
                        scenes.Add(kvp.Value.ToObject<PixelChatSceneModel>());
                    }
                }

                foreach (PixelChatSceneModel scene in scenes)
                {
                    foreach (var kvp in scene.components)
                    {
                        kvp.Value.ID = kvp.Key;
                        kvp.Value.SetProperties();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return scenes;
        }

        public async Task<IEnumerable<PixelChatOverlayModel>> GetOverlays()
        {
            List<PixelChatOverlayModel> overlays = new List<PixelChatOverlayModel>();
            try
            {
                JObject dictionaryOverlays = await this.GetJObjectAsync("overlays");
                if (dictionaryOverlays != null)
                {
                    foreach (var kvp in dictionaryOverlays)
                    {
                        overlays.Add(kvp.Value.ToObject<PixelChatOverlayModel>());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return overlays;
        }

        public async Task<Result> EditSceneComponent(string sceneID, string componentID, bool visible)
        {
            try
            {
                JObject jobj = new JObject();
                jobj["hidden"] = !visible;

                HttpResponseMessage response = await this.PatchAsync($"scenes/{sceneID}/components/{componentID}", AdvancedHttpClient.CreateContentFromObject(jobj));
                if (response.IsSuccessStatusCode)
                {
                    return new Result();
                }
                else
                {
                    return new Result(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
        }

        public async Task<Result> SendMessageToOverlay(string overlayID, PixelChatSendMessageModel message)
        {
            try
            {
                HttpResponseMessage response = await this.PutAsync($"overlays/{overlayID}/message", AdvancedHttpClient.CreateContentFromObject(message));
                if (response.IsSuccessStatusCode)
                {
                    return new Result();
                }
                else
                {
                    return new Result(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
        }

        protected override async Task<Result> InitializeInternal()
        {
            PixelChatUserModel user = await this.GetUser();
            if (user != null)
            {
                return new Result();
            }
            return new Result(MixItUp.Base.Resources.PixelChatFailedToGetUserData);
        }

        protected override Task RefreshOAuthToken()
        {
            return Task.CompletedTask;
        }

        protected override Task<AdvancedHttpClient> GetHttpClient(bool autoRefreshToken = true)
        {
            AdvancedHttpClient httpClient = new AdvancedHttpClient(PixelChatService.BaseAddress);
            httpClient.DefaultRequestHeaders.Add("x-api-key", this.token.accessToken);
            httpClient.DefaultRequestHeaders.Add("x-application", "Mix It Up");
            return Task.FromResult(httpClient);
        }
    }
}
