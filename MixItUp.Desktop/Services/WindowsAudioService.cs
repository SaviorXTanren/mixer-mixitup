using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MixItUp.Desktop.Audio
{
    public class WindowsAudioService : IAudioService
    {
        public Task Play(string filePath, int volume)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                volume = MathHelper.Clamp(volume, 0, 100);
                Task.Run(async () =>
                {
                    bool mediaEnded = false;

                    MediaPlayer mediaPlayer = new MediaPlayer();
                    mediaPlayer.MediaEnded += (object sender, EventArgs e) =>
                    {
                        mediaEnded = true;
                    };

                    if (!Path.IsPathRooted(filePath))
                    {
                        filePath = Path.Combine(ChannelSession.Services.FileService.GetApplicationDirectory(), filePath);
                    }

                    mediaPlayer.Open(new Uri(filePath));
                    mediaPlayer.Volume = ((double)volume / 100.0);
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
