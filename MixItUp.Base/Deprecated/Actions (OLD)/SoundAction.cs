using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [Obsolete]
    [DataContract]
    public class SoundAction : ActionBase
    {
        public static readonly string DefaultAudioDevice = MixItUp.Base.Resources.DefaultOutput;
        public static readonly string MixItUpOverlay = MixItUp.Base.Resources.MixItUpOverlay;

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return SoundAction.asyncSemaphore; } }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public int VolumeScale { get; set; }

        [DataMember]
        public string OutputDevice { get; set; }

        public SoundAction() : base(ActionTypeEnum.Sound) { this.OutputDevice = null; }

        public SoundAction(string filePath, int volumeScale, string outputDevice = null)
            : this()
        {
            this.FilePath = filePath;
            this.VolumeScale = volumeScale;
            this.OutputDevice = outputDevice;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            string audioFilePath = await ReplaceStringWithSpecialModifiers(this.FilePath, null);

            if (SoundAction.MixItUpOverlay.Equals(this.OutputDevice))
            {
                IOverlayEndpointService overlay = ChannelSession.Services.Overlay.GetOverlay(ChannelSession.Services.Overlay.DefaultOverlayName);
                if (overlay != null)
                {
                    var overlayItem = new OverlaySoundItemModel(audioFilePath, this.VolumeScale);
                    //await overlay.ShowItem(overlayItem, user, arguments, this.extraSpecialIdentifiers, this.platform);
                }
            }
            else
            {
                await ChannelSession.Services.AudioService.Play(audioFilePath, this.VolumeScale, this.OutputDevice);
            }
        }
    }
}
