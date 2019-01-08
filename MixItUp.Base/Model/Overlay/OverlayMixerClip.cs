using Mixer.Base.Model.Clips;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayMixerClip : OverlayItemBase
    {
        public const string MixerClipItemType = "mixerclip";

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public OverlayEffectEntranceAnimationTypeEnum EntranceAnimation { get; set; }
        [DataMember]
        public string EntranceAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.EntranceAnimation); } set { } }
        [DataMember]
        public OverlayEffectExitAnimationTypeEnum ExitAnimation { get; set; }
        [DataMember]
        public string ExitAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.ExitAnimation); } set { } }

        [DataMember]
        public string URL { get; set; }
        [DataMember]
        public double Duration { get; set; }

        private ClipModel lastClip = null;

        public OverlayMixerClip() : base(OverlayMixerClip.MixerClipItemType) { }

        public OverlayMixerClip(int width, int height, int volume, OverlayEffectEntranceAnimationTypeEnum entrance, OverlayEffectExitAnimationTypeEnum exit)
            : base(OverlayMixerClip.MixerClipItemType)
        {
            this.Width = width;
            this.Height = height;
            this.Volume = volume;
            this.EntranceAnimation = entrance;
            this.ExitAnimation = exit;
        }

        [DataMember]
        public double VolumeDecimal { get { return ((double)this.Volume / 100.0); } set { } }

        [JsonIgnore]
        public override bool SupportsTestButton { get { return true; } }

        public override async Task LoadTestData()
        {
            this.lastClip = new ClipModel()
            {
                contentLocators = new List<ClipLocatorModel>()
                {
                    new ClipLocatorModel()
                    {
                        locatorType = MixerClipsAction.VideoFileContentLocatorType,
                        uri = "https://raw.githubusercontent.com/SaviorXTanren/mixer-mixitup/master/Wiki/MixerTestClip/manifest.m3u8"
                    }
                },
                durationInSeconds = 5
            };
            await Task.Delay(5000);
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnMixerClipCreated += GlobalEvents_OnMixerClipCreated;

            await base.Initialize();
        }

        public override Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (this.lastClip != null)
            {
                ClipModel clip = this.lastClip;
                this.lastClip = null;

                OverlayMixerClip item = this.Copy<OverlayMixerClip>();
                ClipLocatorModel clipLocator = clip.contentLocators.FirstOrDefault(cl => cl.locatorType.Equals(MixerClipsAction.VideoFileContentLocatorType));
                if (clipLocator != null)
                {
                    item.URL = clipLocator.uri;
                    item.Duration = Math.Max(0, clip.durationInSeconds - 1);
                    return Task.FromResult<OverlayItemBase>(item);
                }
            }
            return Task.FromResult<OverlayItemBase>(null);
        }

        private void GlobalEvents_OnMixerClipCreated(object sender, ClipModel clip) { this.lastClip = clip; }
    }
}
