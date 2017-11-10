using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class SoundAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSempahore { get { return SoundAction.asyncSemaphore; } }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public int VolumeScale { get; set; }

        public SoundAction() : base(ActionTypeEnum.Sound) { }

        public SoundAction(string filePath, int volumeScale)
            : this()
        {
            this.FilePath = filePath;
            this.VolumeScale = volumeScale;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (File.Exists(this.FilePath))
            {
                await ChannelSession.Services.InitializeAudioService();
                await ChannelSession.Services.AudioService.Play(this.FilePath, this.VolumeScale);
            }
        }
    }
}
