using NAudio.Wave;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class SoundAction : ActionBase
    {
        public string SoundFilePath { get; private set; }

        public SoundAction(string soundFilePath)
            : base("Sound")
        {
            this.SoundFilePath = soundFilePath;
        }

        public override async Task Perform()
        {
            if (File.Exists(this.SoundFilePath))
            {
                using (IWavePlayer wavePlayer = new WaveOut())
                {
                    using (AudioFileReader audioFileReader = new AudioFileReader(this.SoundFilePath))
                    {
                        wavePlayer.Init(audioFileReader);
                        wavePlayer.Play();

                        while (wavePlayer.PlaybackState == PlaybackState.Playing)
                        {
                            await this.Wait500();
                        }

                        wavePlayer.Stop();
                    }
                }
            }
        }
    }
}
