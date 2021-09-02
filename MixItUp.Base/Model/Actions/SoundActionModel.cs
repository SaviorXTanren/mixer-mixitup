using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
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

        private SoundActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string audioFilePath = await this.ReplaceStringWithSpecialModifiers(this.FilePath, parameters);
            await ServiceManager.Get<IAudioService>().Play(audioFilePath, this.VolumeScale, this.OutputDevice);
        }
    }
}
