using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MixItUp.Desktop.Audio
{
    public class AudioService : IAudioService
    {
        public Task Play(string filePath, int volume)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                Task.Run(async () =>
                {
                    float floatVolume = MathHelper.Clamp(volume, 0, 100) / 100.0f;
                    using (AudioFileReader audioFileReader = new AudioFileReader(filePath))
                    {
                        using (WaveOutEvent outputDevice = new WaveOutEvent())
                        {
                            outputDevice.Init(audioFileReader);
                            outputDevice.Volume = floatVolume;
                            outputDevice.Play();

                            while (outputDevice.PlaybackState == PlaybackState.Playing)
                            {
                                await Task.Delay(500);
                            }
                        }
                    }
                });
            }
            return Task.FromResult(0);
        }
    }
}
