using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayClipPlaybackItemModel : OverlayFileItemModelBase
    {
        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public double Duration { get; set; }

        [DataMember]
        public StreamingPlatformTypeEnum Platform { get; set; } = StreamingPlatformTypeEnum.None;

        private Twitch.Base.Models.NewAPI.Clips.ClipModel lastTwitchClip = null;
        private string lastClipURL = null;

        public OverlayClipPlaybackItemModel() : base() { }

        public OverlayClipPlaybackItemModel(int width, int height, int volume, OverlayItemEffectEntranceAnimationTypeEnum entranceAnimation, OverlayItemEffectExitAnimationTypeEnum exitAnimation)
            : base(OverlayItemModelTypeEnum.ClipPlayback, string.Empty, width, height)
        {
            this.Width = width;
            this.Height = height;
            this.Volume = volume;
            this.Effects = new OverlayItemEffectsModel(entranceAnimation, OverlayItemEffectVisibleAnimationTypeEnum.None, exitAnimation, 0);
        }

        [DataMember]
        public override string FullLink { get { return this.lastClipURL; } set { } }

        [DataMember]
        public override string FileType { get { return "video"; } set { } }

        [DataMember]
        public double VolumeDecimal { get { return ((double)this.Volume / 100.0); } set { } }

        [DataMember]
        public string PlatformName { get { return this.Platform.ToString(); } set { } }

        [JsonIgnore]
        public override bool SupportsTestData { get { return true; } }

        public override Task LoadTestData()
        {
            this.Platform = StreamingPlatformTypeEnum.All;
            this.lastClipURL = "https://raw.githubusercontent.com/SaviorXTanren/mixer-mixitup/master/Wiki/Clip/TwitchTestClip.mp4";
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

        public override async Task<JObject> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, StreamingPlatformTypeEnum platform)
        {
            JObject jobj = null;
            if (this.lastTwitchClip != null)
            {
                this.Platform = StreamingPlatformTypeEnum.Twitch;
                this.lastClipURL = this.lastTwitchClip.embed_url;
                this.Effects.Duration = Math.Max(0, 30);

                this.lastTwitchClip = null;
            }

            if (!string.IsNullOrEmpty(this.lastClipURL))
            {
                jobj = await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers, StreamingPlatformTypeEnum.Twitch);
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
