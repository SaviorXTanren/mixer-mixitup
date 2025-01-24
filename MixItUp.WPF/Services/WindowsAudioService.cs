using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using NAudio.Wave;
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

        public async Task Play(string filePath, int volume, string deviceName, bool waitForFinish = false)
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    deviceName = this.GetOutputDeviceName(deviceName);
                    if (this.MixItUpOverlay.Equals(deviceName))
                    {
                        await this.PlayOnOverlay(filePath, volume, waitForFinish);
                    }
                    else
                    {
                        await this.PlayAndCleanUp(this.PlayWithOutput(filePath, volume, deviceName), waitForFinish);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task PlayMP3Stream(Stream stream, int volume, string deviceName, bool waitForFinish = false)
        {
            try
            {
                if (stream != null && stream.Length > 0)
                {
                    deviceName = this.GetOutputDeviceName(deviceName);
                    if (this.MixItUpOverlay.Equals(deviceName))
                    {
                        string tempFilePath = Path.Combine(ServiceManager.Get<IFileService>().GetTempFolder(), $"{Guid.NewGuid()}.mp3");
                        await ServiceManager.Get<IFileService>().SaveFile(tempFilePath, stream);
                        await this.PlayOnOverlay(tempFilePath, volume, waitForFinish);
                    }
                    else
                    {
                        await this.PlayWaveStream(new StreamMediaFoundationReader(stream), deviceName, volume, waitForFinish);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task PlayPCMStream(Stream stream, int volume, string deviceName, bool waitForFinish = false)
        {
            try
            {
                if (stream != null && stream.Length > 0)
                {
                    deviceName = this.GetOutputDeviceName(deviceName);
                    if (this.MixItUpOverlay.Equals(deviceName))
                    {
                        Logger.ForceLog(LogLevel.Error, "Mix It Up Overlay not supported for playing PCM streams");
                        if (ChannelSession.IsDebug())
                        {
                            throw new Exception("Mix It Up Overlay not supported for playing PCM streams");
                        }
                        return;
                    }
                    await this.PlayWaveStream(new RawSourceWaveStream(stream, new WaveFormat(88200, 16, 1)), deviceName, volume, waitForFinish);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task PlayOnOverlay(string filePath, double volume, bool waitForFinish = false)
        {
            OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpointService();
            if (overlay != null)
            {
                Guid id = await overlay.PlayAudio(filePath, volume);
                lock (this.activeOverlaySounds)
                {
                    this.activeOverlaySounds.Add(id);
                }

                if (waitForFinish)
                {
                    bool contains = false;
                    do
                    {
                        await Task.Delay(100);

                        lock (this.activeOverlaySounds)
                        {
                            contains = this.activeOverlaySounds.Contains(id);
                        }

                    } while (contains);
                }
            }
        }

        public async Task PlayNotification(string filePath, int volume)
        {
            try
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
                    await this.Play(filePath, volume, ChannelSession.Settings.NotificationsAudioOutput);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
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

                lock (this.activeOverlaySounds)
                {
                    this.activeOverlaySounds.Clear();
                }
            }
        }

        public void OverlaySoundFinished(Guid id)
        {
            lock (this.activeOverlaySounds)
            {
                this.activeOverlaySounds.Remove(id);
            }
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
            try
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
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public float ConvertVolumeAmount(int amount) { return MathHelper.Clamp(amount, 0, 100) / 100.0f; }

        public string GetOutputDeviceName(string deviceName)
        {
            if (string.IsNullOrEmpty(deviceName))
            {
                return ChannelSession.Settings.DefaultAudioOutput;
            }
            return deviceName;
        }

        private WaveOutEvent CreateWaveOutEvent(string deviceName)
        {
            int deviceNumber = -1;
            if (!string.IsNullOrEmpty(deviceName))
            {
                deviceNumber = this.GetOutputDeviceID(deviceName);
            }
            return (deviceNumber < 0) ? new WaveOutEvent() : new WaveOutEvent() { DeviceNumber = deviceNumber };
        }

        private async Task PlayAndCleanUp(Tuple<WaveOutEvent, WaveStream> output, bool waitForFinish = false)
        {
            if (output != null)
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                audioTasks[cancellationTokenSource.Token] = cancellationTokenSource;
                Task t = Task.Run(async () =>
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

                if (waitForFinish)
                {
                    await t;
                }
            }
        }

        private async Task PlayWaveStream(WaveStream stream, string deviceName, int volume, bool waitForFinish = false)
        {
            float floatVolume = this.ConvertVolumeAmount(volume);
            WaveOutEvent outputEvent = this.CreateWaveOutEvent(deviceName);
            outputEvent.Volume = floatVolume;

            outputEvent.Init(stream);
            outputEvent.Play();

            await this.PlayAndCleanUp(new Tuple<WaveOutEvent, WaveStream>(outputEvent, stream), waitForFinish);
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
