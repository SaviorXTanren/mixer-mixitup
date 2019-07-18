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
    public class OverlayStreamClipItemModel : OverlayFileItemModelBase
    {
        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public double Duration { get; set; }

        private ClipModel lastClip = null;
        private string lastClipURL = null;

        public OverlayStreamClipItemModel() : base() { }

        public OverlayStreamClipItemModel(int width, int height, int volume, OverlayItemEffectEntranceAnimationTypeEnum entranceAnimation, OverlayItemEffectExitAnimationTypeEnum exitAnimation)
            : base(OverlayItemModelTypeEnum.StreamClip, string.Empty, width, height)
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

        [JsonIgnore]
        public override bool SupportsTestData { get { return true; } }

        public override async Task LoadTestData()
        {
            this.GlobalEvents_OnMixerClipCreated(this, new ClipModel()
            {
                contentLocators = new List<ClipLocatorModel>()
                {
                    new ClipLocatorModel()
                    {
                        locatorType = MixerClipsAction.VideoFileContentLocatorType,
                        uri = "https://raw.githubusercontent.com/SaviorXTanren/mixer-mixitup/master/Wiki/MixerTestClip/manifest.m3u8"
                    }
                },
                durationInSeconds = 10
            });

            await Task.Delay(5000);
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnMixerClipCreated += GlobalEvents_OnMixerClipCreated;

            await base.Initialize();
        }

        public override async Task Disable()
        {
            GlobalEvents.OnMixerClipCreated += GlobalEvents_OnMixerClipCreated;

            await base.Disable();
        }

        public override async Task<JObject> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (this.lastClip != null)
            {
                return await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
            }
            return null;
        }

        protected override async Task PerformReplacements(JObject jobj, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (this.lastClip != null)
            {
                ClipModel clip = this.lastClip;
                this.lastClip = null;

                ClipLocatorModel clipLocator = clip.contentLocators.FirstOrDefault(cl => cl.locatorType.Equals(MixerClipsAction.VideoFileContentLocatorType));
                if (clipLocator != null)
                {
                    this.lastClipURL = clipLocator.uri;
                    this.Effects.Duration = Math.Max(0, clip.durationInSeconds - 1);
                    await base.PerformReplacements(jobj, user, arguments, extraSpecialIdentifiers);
                }
            }
        }

        private void GlobalEvents_OnMixerClipCreated(object sender, ClipModel clip)
        {
            this.lastClip = clip;
            this.SendUpdateRequired();
        }
    }
}
