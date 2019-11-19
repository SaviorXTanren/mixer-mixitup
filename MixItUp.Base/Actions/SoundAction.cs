using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
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
        public static readonly string DefaultAudioDevice = MixItUp.Base.Resources.DefaultOutput;

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
            string audioFilePath = await this.ReplaceStringWithSpecialModifiers(this.FilePath, user, arguments);
            int audioDevice = -1;
            if (!string.IsNullOrEmpty(this.OutputDevice))
            {
                audioDevice = await ChannelSession.Services.AudioService.GetOutputDevice(this.OutputDevice);
            }

            await ChannelSession.Services.AudioService.Play(audioFilePath, this.VolumeScale, audioDevice);
        }
    }
}
