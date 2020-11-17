using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayClipPlaybackItemModel : OverlayFileItemModelBase
    {
        [DataMember]
        public bool Muted { get; set; }

        [DataMember]
        public double Duration { get; set; }

        [DataMember]
        public StreamingPlatformTypeEnum Platform { get; set; } = StreamingPlatformTypeEnum.None;

        private Twitch.Base.Models.NewAPI.Clips.ClipModel lastTwitchClip = null;
        private string lastClipURL = null;

        public OverlayClipPlaybackItemModel() : base() { }

        public OverlayClipPlaybackItemModel(int width, int height, bool muted, OverlayItemEffectEntranceAnimationTypeEnum entranceAnimation, OverlayItemEffectExitAnimationTypeEnum exitAnimation)
            : base(OverlayItemModelTypeEnum.ClipPlayback, string.Empty, width, height)
        {
            this.Width = width;
            this.Height = height;
            this.Muted = muted;
            this.Effects = new OverlayItemEffectsModel(entranceAnimation, OverlayItemEffectVisibleAnimationTypeEnum.None, exitAnimation, 0);
        }

        [DataMember]
        public override string FullLink { get { return this.lastClipURL; } set { } }

        [DataMember]
        public override string FileType { get { return "video"; } set { } }

        [DataMember]
        public string PlatformName { get { return this.Platform.ToString(); } set { } }

        [JsonIgnore]
        public override bool SupportsTestData { get { return true; } }

        public override Task LoadTestData()
        {
            this.Platform = StreamingPlatformTypeEnum.All;
            this.Effects.Duration = this.Duration = 3;
            this.lastClipURL = "https://raw.githubusercontent.com/SaviorXTanren/mixer-mixitup/master/Wiki/Clips/TestClipImage.png";

            return Task.FromResult(0);
        }

        public override async Task Enable()
        {
            GlobalEvents.OnTwitchClipCreated += GlobalEvents_OnTwitchClipCreated;

            await base.Enable();
        }

        public override async Task Disable()
        {
            GlobalEvents.OnTwitchClipCreated -= GlobalEvents_OnTwitchClipCreated;

            await base.Disable();
        }

        public override async Task<JObject> GetProcessedItem(CommandParametersModel parameters)
        {
            JObject jobj = null;
            if (this.lastTwitchClip != null)
            {
                this.Platform = StreamingPlatformTypeEnum.Twitch;
                this.lastClipURL = this.lastTwitchClip.embed_url + "&parent=localhost";
                if (this.Muted)
                {
                    this.lastClipURL += "&muted=true";
                }
                this.Effects.Duration = this.Duration = 30;

                this.lastTwitchClip = null;
            }

            if (!string.IsNullOrEmpty(this.lastClipURL))
            {
                jobj = await base.GetProcessedItem(parameters);
            }

            this.Platform = StreamingPlatformTypeEnum.None;
            this.lastClipURL = null;

            return jobj;
        }

        private void GlobalEvents_OnTwitchClipCreated(object sender, Twitch.Base.Models.NewAPI.Clips.ClipModel clip)
        {
            this.lastTwitchClip = clip;
            this.SendUpdateRequired();
        }
    }
}
