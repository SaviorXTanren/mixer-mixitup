using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class SoundActionModel : ActionModelBase
    {
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

        internal SoundActionModel(MixItUp.Base.Actions.SoundAction action)
            : base(ActionTypeEnum.Sound)
        {
            this.FilePath = action.FilePath;
            this.VolumeScale = action.VolumeScale;
            this.OutputDevice = action.OutputDevice;
        }

        private SoundActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string audioFilePath = await this.ReplaceStringWithSpecialModifiers(this.FilePath, parameters);
            if (SoundActionModel.MixItUpOverlay.Equals(this.OutputDevice))
            {
                IOverlayEndpointService overlay = ChannelSession.Services.Overlay.GetOverlay(ChannelSession.Services.Overlay.DefaultOverlayName);
                if (overlay != null)
                {
                    var overlayItem = new OverlaySoundItemModel(audioFilePath, this.VolumeScale);
                    await overlay.ShowItem(overlayItem, parameters);
                }
            }
            else
            {
                await ChannelSession.Services.AudioService.Play(audioFilePath, this.VolumeScale, this.OutputDevice);
            }
        }
    }
}
