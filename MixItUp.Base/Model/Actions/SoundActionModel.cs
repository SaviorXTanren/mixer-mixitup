using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class SoundActionModel : ActionModelBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return SoundActionModel.asyncSemaphore; } }

        public static readonly string DefaultAudioDevice = MixItUp.Base.Resources.DefaultOutput;
        public static readonly string MixItUpOverlay = MixItUp.Base.Resources.MixItUpOverlay;

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public int VolumeScale { get; set; }

        [DataMember]
        public string OutputDevice { get; set; }

        public SoundActionModel(string filePath, int volumeScale, string outputDevice = null)
            : base(ActionTypeEnum.Sound)
        {
            this.FilePath = filePath;
            this.VolumeScale = volumeScale;
            this.OutputDevice = outputDevice;
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            string audioFilePath = await this.ReplaceStringWithSpecialModifiers(this.FilePath, user, platform, arguments, specialIdentifiers);
            if (SoundActionModel.MixItUpOverlay.Equals(this.OutputDevice))
            {
                IOverlayEndpointService overlay = ChannelSession.Services.Overlay.GetOverlay(ChannelSession.Services.Overlay.DefaultOverlayName);
                if (overlay != null)
                {
                    var overlayItem = new OverlaySoundItemModel(audioFilePath, this.VolumeScale);
                    await overlay.ShowItem(overlayItem, user, arguments, specialIdentifiers, platform);
                }
            }
            else
            {
                await ChannelSession.Services.AudioService.Play(audioFilePath, this.VolumeScale, this.OutputDevice);
            }
        }
    }
}
