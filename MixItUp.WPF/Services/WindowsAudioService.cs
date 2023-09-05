using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using NAudio.Wave;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.WPF.Services
{
    public class WindowsAudioService : IAudioService
    {
        public string DefaultAudioDevice { get { return MixItUp.Base.Resources.DefaultOutput; } }
        public string MixItUpOverlay { get { return MixItUp.Base.Resources.MixItUpOverlay; } }

        public ISet<string> ApplicableAudioFileExtensions => new HashSet<string>() { ".mp3", ".wav", ".flac", ".mp4", ".m4a", ".aac" };

        private Dictionary<CancellationToken, CancellationTokenSource> audioTasks = new Dictionary<CancellationToken, CancellationTokenSource>();

        public async Task Play(string filePath, int volume, bool track = true, bool waitForFinish = false)
        {
            await this.Play(filePath, volume, null, track);
        }

        public async Task Play(string filePath, int volume, string deviceName, bool track = true, bool waitForFinish = false)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                if (string.IsNullOrEmpty(deviceName))
                {
                    deviceName = ChannelSession.Settings.DefaultAudioOutput;
                }

                if (this.MixItUpOverlay.Equals(deviceName))
                {
                    OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpointService();
                    if (overlay != null)
                    {
                        //var overlayItem = new OverlaySoundItemModel(filePath, volume);
                        //await overlay.ShowItem(overlayItem, new CommandParametersModel());
                    }
                }
                else
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    audioTasks[cancellationTokenSource.Token] = cancellationTokenSource;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(async () =>
                    {
                        using (WaveOutEvent waveStream = this.PlayWithOutput(filePath, volume, deviceName))
                        {
                            if (waveStream != null)
                            {
                                while (waveStream.PlaybackState == PlaybackState.Playing && !cancellationTokenSource.Token.IsCancellationRequested)
                                {
                                    await Task.Delay(500);
                                }
                                waveStream.Dispose();
                                cancellationTokenSource.Cancel();
                            }
                        }
                    }, cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }

        public Task PlayMP3(Stream stream, int volume, string deviceName, bool track = true, bool waitForFinish = false)
        {
            throw new NotImplementedException();
        }

        public Task PlayPCM(Stream stream, int volume, string deviceName, bool track = true, bool waitForFinish = false)
        {
            throw new NotImplementedException();
        }

        public async Task PlayNotification(string filePath, int volume, bool track = true)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                if (ChannelSession.Settings.NotificationCooldownAmount > 0)
                {
                    if (ChannelSession.Settings.NotificationLastTrigger.TotalSecondsFromNow() < ChannelSession.Settings.NotificationCooldownAmount)
                    {
                        return;
                    }
                    ChannelSession.Settings.NotificationLastTrigger = DateTimeOffset.Now;
                }
                await this.Play(filePath, volume, ChannelSession.Settings.NotificationsAudioOutput, track: track);
            }
        }

        public Task StopAllSounds()
        {
            foreach (var key in this.audioTasks.Keys.ToList())
            {
                if (this.audioTasks.TryGetValue(key, out CancellationTokenSource tokenSource))
                {
                    tokenSource.Cancel();
                }
                this.audioTasks.Remove(key);
            }
            return Task.CompletedTask;
        }

        public IEnumerable<string> GetSelectableAudioDevices(bool includeOverlay = false)
        {
            List<string> audioOptions = new List<string>();
            audioOptions.Add(this.DefaultAudioDevice);
            audioOptions.Add(this.MixItUpOverlay);
            audioOptions.AddRange(ServiceManager.Get<IAudioService>().GetOutputDevices());
            return audioOptions;
        }

        public IEnumerable<string> GetOutputDevices()
        {
            List<string> results = new List<string>();
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                try
                {
                    WaveOutCapabilities capabilities = WaveOut.GetCapabilities(i);
                    results.Add(capabilities.ProductName);
                }
                catch (Exception ex) { Logger.Log(ex); }
            }
            return results;
        }

        public WaveOutEvent PlayWithOutput(string filePath, int volume, string deviceName)
        {
            int deviceNumber = -1;
            if (!string.IsNullOrEmpty(deviceName))
            {
                deviceNumber = this.GetOutputDeviceID(deviceName);
            }

            float floatVolume = this.ConvertVolumeAmount(volume);

            WaveOutEvent outputEvent = (deviceNumber < 0) ? new WaveOutEvent() : new WaveOutEvent() { DeviceNumber = deviceNumber };
            WaveStream waveStream = null;
            if (filePath.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                waveStream = new MediaFoundationReader(filePath);
                outputEvent.Volume = floatVolume;
            }
            else if (File.Exists(filePath))
            {
                AudioFileReader audioFile = new AudioFileReader(filePath);
                audioFile.Volume = floatVolume;
                waveStream = audioFile;
            }

            if (waveStream != null)
            {
                outputEvent.Init(waveStream);
                outputEvent.Play();
                return outputEvent;
            }
            return null;
        }

        public float ConvertVolumeAmount(int amount) { return MathHelper.Clamp(amount, 0, 100) / 100.0f; }

        private int GetOutputDeviceID(string deviceName)
        {
            if (!string.IsNullOrEmpty(deviceName))
            {
                for (int i = 0; i < WaveOut.DeviceCount; i++)
                {
                    try
                    {
                        WaveOutCapabilities capabilities = WaveOut.GetCapabilities(i);
                        if (deviceName.Equals(capabilities.ProductName))
                        {
                            return i;
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                }
            }
            return -1;
        }
    }
}
