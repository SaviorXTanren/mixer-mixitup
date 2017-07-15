using Mixer.Base.ViewModel;
using NAudio.Wave;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class SoundAction : ActionBase
    {
        public string FilePath { get; set; }

        public float VolumeScale { get; set; }

        public SoundAction(string filePath, float volumeScale)
            : base(ActionTypeEnum.Sound)
        {
            this.FilePath = filePath;
            this.VolumeScale = volumeScale;
        }

        public override async Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (File.Exists(this.FilePath))
            {
                using (DirectSoundOut output = new DirectSoundOut())
                {
                    using (AudioFileReader audioFileReader = new AudioFileReader(this.FilePath))
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

        public override SerializableAction Serialize()
        {
            return new SerializableAction()
            {
                Type = this.Type,
                Values = new List<string>() { this.FilePath, this.VolumeScale.ToString() }
            };
        }
    }
}
