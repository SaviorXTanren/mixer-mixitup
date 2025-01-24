using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum SoundActionTypeEnum
    {
        PlaySound,
        StopAllSounds,
    }

    [DataContract]
    public class SoundActionModel : ActionModelBase
    {
        [DataMember]
        public SoundActionTypeEnum ActionType { get; set; } = SoundActionTypeEnum.PlaySound;

        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public int VolumeScale { get; set; }
        [DataMember]
        public string OutputDevice { get; set; }

        public SoundActionModel(string filePath, int volumeScale, string outputDevice = null)
            : this(SoundActionTypeEnum.PlaySound)
        {
            this.FilePath = filePath;
            this.VolumeScale = volumeScale;
            this.OutputDevice = outputDevice;
        }

        public SoundActionModel(SoundActionTypeEnum actionType)
            : base(ActionTypeEnum.Sound)
        {
            this.ActionType = actionType;
        }

        [Obsolete]
        public SoundActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.ActionType == SoundActionTypeEnum.PlaySound)
            {
                string audioFilePath = RandomHelper.PickRandomFileFromDelimitedString(this.FilePath);
                audioFilePath = await ReplaceStringWithSpecialModifiers(audioFilePath, parameters);

                if (!ServiceManager.Get<IFileService>().IsURLPath(audioFilePath) && !ServiceManager.Get<IFileService>().FileExists(audioFilePath))
                {
                    Logger.Log(LogLevel.Error, $"Command: {parameters.InitialCommandID} - Sound Action - File does not exist: {audioFilePath}");
                }

                await ServiceManager.Get<IAudioService>().Play(audioFilePath, this.VolumeScale, this.OutputDevice);
            }
            else
            {
                await ServiceManager.Get<IAudioService>().StopAllSounds();
            }
        }
    }
}
