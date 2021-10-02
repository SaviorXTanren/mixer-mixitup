using MixItUp.Base.Model.Commands;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class SoundActionModel : ActionModelBase
    {
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

#pragma warning disable CS0612 // Type or member is obsolete
        internal SoundActionModel(MixItUp.Base.Actions.SoundAction action)
            : base(ActionTypeEnum.Sound)
        {
            this.FilePath = action.FilePath;
            this.VolumeScale = action.VolumeScale;
            this.OutputDevice = action.OutputDevice;
        }
#pragma warning disable CS0612 // Type or member is obsolete

        private SoundActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string audioFilePath = await ReplaceStringWithSpecialModifiers(this.FilePath, parameters);
            await ChannelSession.Services.AudioService.Play(audioFilePath, this.VolumeScale, this.OutputDevice);
        }
    }
}
