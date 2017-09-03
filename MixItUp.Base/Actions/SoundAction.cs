using MixItUp.Base.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class SoundAction : ActionBase
    {
        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public int VolumeScale { get; set; }

        private MediaPlayer mediaPlayer;

        public SoundAction(string filePath, int volumeScale)
            : base(ActionTypeEnum.Sound)
        {
            this.FilePath = filePath;
            this.VolumeScale = volumeScale;
        }

        public override Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (File.Exists(this.FilePath))
            {
                if (this.mediaPlayer == null)
                {
                    this.mediaPlayer = new MediaPlayer();
                    this.mediaPlayer.Open(new Uri(this.FilePath));
                }
                this.mediaPlayer.Volume = ((double)this.VolumeScale / 100.0);
                this.mediaPlayer.Play();
            }
            return Task.FromResult(0);
        }
    }
}
