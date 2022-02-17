using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using StreamingClient.Base.Util;
using System;
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

        [Obsolete]
        public SoundActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string audioFilePath = await ReplaceStringWithSpecialModifiers(this.FilePath, parameters);

            if (!ServiceManager.Get<IFileService>().IsURLPath(audioFilePath) && !ServiceManager.Get<IFileService>().FileExists(audioFilePath))
            {
                Logger.Log(LogLevel.Error, $"Command: {parameters.InitialCommandID} - Sound Action - File does not exist: {audioFilePath}");
            }

            await ServiceManager.Get<IAudioService>().Play(audioFilePath, this.VolumeScale, this.OutputDevice);
        }
    }
}
