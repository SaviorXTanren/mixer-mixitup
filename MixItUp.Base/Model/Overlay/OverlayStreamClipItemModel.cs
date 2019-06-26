using Mixer.Base.Model.Clips;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
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
    public class OverlayStreamClipItemModel : OverlayHTMLTemplateItemModelBase
    {
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public string URL { get; set; }
        [DataMember]
        public double Duration { get; set; }

        private ClipModel lastClip = null;

        public OverlayStreamClipItemModel() : base() { }

        public OverlayStreamClipItemModel(int width, int height, int volume, OverlayItemEffectEntranceAnimationTypeEnum entranceAnimation, OverlayItemEffectExitAnimationTypeEnum exitAnimation)
            : base(OverlayItemModelTypeEnum.StreamClip, "")
        {
            this.Width = width;
            this.Height = height;
            this.Volume = volume;
            this.Effects = new OverlayItemEffectsModel(entranceAnimation, OverlayItemEffectVisibleAnimationTypeEnum.None, exitAnimation, 0);
        }

        [DataMember]
        public double VolumeDecimal { get { return ((double)this.Volume / 100.0); } set { } }

        [JsonIgnore]
        public override bool SupportsTestData { get { return true; } }

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

        public override Task<JObject> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, bool encode = false)
        {
            if (this.lastClip != null)
            {
                ClipModel clip = this.lastClip;
                this.lastClip = null;

                ClipLocatorModel clipLocator = clip.contentLocators.FirstOrDefault(cl => cl.locatorType.Equals(MixerClipsAction.VideoFileContentLocatorType));
                if (clipLocator != null)
                {
                    this.URL = clipLocator.uri;
                    this.Duration = Math.Max(0, clip.durationInSeconds - 1);
                    return base.GetProcessedItem(user, arguments, extraSpecialIdentifiers, encode);
                }
            }
            return null;
        }

        private void GlobalEvents_OnMixerClipCreated(object sender, ClipModel clip)
        {
            this.lastClip = clip;
            this.SendUpdateRequired();
        }
    }
}
