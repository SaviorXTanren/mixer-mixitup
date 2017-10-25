using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class SoundAction : ActionBase
    {
        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public int VolumeScale { get; set; }

        public SoundAction(string filePath, int volumeScale)
            : base(ActionTypeEnum.Sound)
        {
            this.FilePath = filePath;
            this.VolumeScale = volumeScale;
        }

        public override async Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (File.Exists(this.FilePath))
            {
                await ChannelSession.Services.InitializeAudioService();
                await ChannelSession.Services.AudioService.Play(this.FilePath, this.VolumeScale);
            }
        }
    }
}
