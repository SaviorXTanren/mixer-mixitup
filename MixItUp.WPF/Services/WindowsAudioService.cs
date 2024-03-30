using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
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

        private HashSet<Guid> activeOverlaySounds = new HashSet<Guid>();

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
                        Guid audioID = Guid.NewGuid();
                        OverlaySoundV3Model overlayItem = new OverlaySoundV3Model(filePath, volume);
                        await overlay.Add(audioID.ToString(), await OverlayV3Service.PerformBasicOverlayItemProcessing(overlay, overlayItem));
                        this.activeOverlaySounds.Add(audioID);
                    }
                }
                else
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.PlayAndCleanUp(this.PlayWithOutput(filePath, volume, deviceName));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }

        public Task PlayMP3(Stream stream, int volume, string deviceName, bool track = true, bool waitForFinish = false)
        {
            float floatVolume = this.ConvertVolumeAmount(volume);
            WaveOutEvent outputEvent = this.CreateWaveOutEvent(deviceName);
            outputEvent.Volume = floatVolume;
            Mp3FileReader mp3Stream = new Mp3FileReader(stream);

            outputEvent.Init(mp3Stream);
            outputEvent.Play();

            return this.PlayAndCleanUp(new Tuple<WaveOutEvent, WaveStream>(outputEvent, mp3Stream));
        }

        public Task PlayPCM(Stream stream, int volume, string deviceName, bool track = true, bool waitForFinish = false)
        {
            float floatVolume = this.ConvertVolumeAmount(volume);
            WaveOutEvent outputEvent = this.CreateWaveOutEvent(deviceName);
            outputEvent.Volume = floatVolume;
            RawSourceWaveStream rawSourceStream = new RawSourceWaveStream(stream, new WaveFormat(88200, 16, 1));

            outputEvent.Init(rawSourceStream);
            outputEvent.Play();

            return this.PlayAndCleanUp(new Tuple<WaveOutEvent, WaveStream>(outputEvent, rawSourceStream));
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

        public async Task StopAllSounds()
        {
            foreach (var key in this.audioTasks.Keys.ToList())
            {
                if (this.audioTasks.TryGetValue(key, out CancellationTokenSource tokenSource))
                {
                    tokenSource.Cancel();
                }
                this.audioTasks.Remove(key);
            }

            OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpointService();
            if (overlay != null)
            {
                foreach (var id in this.activeOverlaySounds.ToList())
                {
                    await overlay.Function(id.ToString(), "remove", new Dictionary<string, object>());
                }
                this.activeOverlaySounds.Clear();
            }
        }

        public void OverlaySoundFinished(Guid id)
        {
            this.activeOverlaySounds.Remove(id);
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

        public Tuple<WaveOutEvent, WaveStream> PlayWithOutput(string filePath, int volume, string deviceName)
        {
            float floatVolume = this.ConvertVolumeAmount(volume);
            WaveOutEvent outputEvent = this.CreateWaveOutEvent(deviceName);
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
                return new Tuple<WaveOutEvent, WaveStream>(outputEvent, waveStream);
            }
            return null;
        }

        public float ConvertVolumeAmount(int amount) { return MathHelper.Clamp(amount, 0, 100) / 100.0f; }

        private WaveOutEvent CreateWaveOutEvent(string deviceName)
        {
            int deviceNumber = -1;
            if (!string.IsNullOrEmpty(deviceName))
            {
                deviceNumber = this.GetOutputDeviceID(deviceName);
            }
            return (deviceNumber < 0) ? new WaveOutEvent() : new WaveOutEvent() { DeviceNumber = deviceNumber };
        }

        private Task PlayAndCleanUp(Tuple<WaveOutEvent, WaveStream> output)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            audioTasks[cancellationTokenSource.Token] = cancellationTokenSource;
            return Task.Run(async () =>
            {
                try
                {
                    WaveOutEvent waveOut = output.Item1;
                    using (waveOut)
                    {
                        if (waveOut != null)
                        {
                            while (waveOut.PlaybackState == PlaybackState.Playing && !cancellationTokenSource.Token.IsCancellationRequested)
                            {
                                await Task.Delay(500);
                            }
                            waveOut.Dispose();
                            cancellationTokenSource.Cancel();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }, cancellationTokenSource.Token);
        }

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
