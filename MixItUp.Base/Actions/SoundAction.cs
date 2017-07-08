using Mixer.Base.ViewModel;
using NAudio.Wave;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class SoundAction : ActionBase
    {
        public string SoundFilePath { get; set; }

        public float VolumeScale { get; set; }

        public SoundAction() : base("Sound") { this.VolumeScale = 1.0f; }

        public override async Task Perform(UserViewModel user)
        {
            if (File.Exists(this.SoundFilePath))
            {
                using (DirectSoundOut output = new DirectSoundOut())
                {
                    using (AudioFileReader audioFileReader = new AudioFileReader(this.SoundFilePath))
                    {
                        output.Init(audioFileReader);
                        output.Volume = this.VolumeScale;
                        output.Play();

                        while (output.PlaybackState == PlaybackState.Playing)
                        {
                            await this.Wait500();
                        }

                        output.Stop();
                    }
                }
            }
        }
    }
}
