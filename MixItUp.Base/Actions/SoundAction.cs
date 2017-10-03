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
                Task.Run(async () =>
                {
                    bool mediaEnded = false;

                    MediaPlayer mediaPlayer = new MediaPlayer();
                    mediaPlayer.MediaEnded += (object sender, EventArgs e) =>
                    {
                        mediaEnded = true;
                    };
                    mediaPlayer.Open(new Uri(this.FilePath));
                    mediaPlayer.Volume = ((double)this.VolumeScale / 100.0);
                    mediaPlayer.Play();

                    while (!mediaEnded)
                    {
                        await Task.Delay(500);
                    }

                    mediaPlayer.Close();
               });
            }
            return Task.FromResult(0);
        }
    }
}
