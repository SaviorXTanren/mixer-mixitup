using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayGameQueueV3Model : OverlayVisualTextV3ModelBase
    {
        public static readonly string DefaultHTML = OverlayResources.OverlayGameQueueDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayGameQueueDefaultCSS + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayGameQueueDefaultJavascript;

        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string BorderColor { get; set; }

        [DataMember]
        public int TotalToShow { get; set; }

        [DataMember]
        public OverlayAnimationV3Model ItemAddedAnimation { get; set; } = new OverlayAnimationV3Model();
        [DataMember]
        public OverlayAnimationV3Model ItemRemovedAnimation { get; set; } = new OverlayAnimationV3Model();

        public OverlayGameQueueV3Model() : base(OverlayItemV3Type.GameQueue) { }

        public override async Task Initialize()
        {
            await base.Initialize();

            GameQueueService.OnGameQueueUpdated -= GameQueueService_OnGameQueueUpdated;
            GameQueueService.OnGameQueueUpdated += GameQueueService_OnGameQueueUpdated;
        }

        public override async Task Uninitialize()
        {
            await base.Uninitialize();

            GameQueueService.OnGameQueueUpdated -= GameQueueService_OnGameQueueUpdated;
        }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            properties[nameof(this.BackgroundColor)] = this.BackgroundColor;
            properties[nameof(this.BorderColor)] = this.BorderColor;

            this.ItemAddedAnimation.AddAnimationProperties(properties, nameof(this.ItemAddedAnimation));
            this.ItemRemovedAnimation.AddAnimationProperties(properties, nameof(this.ItemRemovedAnimation));

            return properties;
        }

        public async Task ClearGameQueue()
        {
            await this.CallFunction("clear", new Dictionary<string, object>());
        }

        public async Task UpdateGameQueue(IEnumerable<UserV2ViewModel> users)
        {
            if (users == null || users.Count() == 0)
            {
                return;
            }

            JArray jarr = new JArray();

            foreach (UserV2ViewModel user in users.Take(this.TotalToShow))
            {
                JObject jobj = new JObject();
                jobj[UserProperty] = JObject.FromObject(user);
                jarr.Add(jobj);
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["Items"] = jarr;
            await this.CallFunction("update", data);
        }

        protected override async Task Loaded()
        {
            await this.UpdateGameQueue(ServiceManager.Get<GameQueueService>().Queue.ToList().Select(p => p.User));
        }

        private async void GameQueueService_OnGameQueueUpdated(object sender, EventArgs e)
        {
            await this.UpdateGameQueue(ServiceManager.Get<GameQueueService>().Queue.ToList().Select(p => p.User));
        }
    }
}
