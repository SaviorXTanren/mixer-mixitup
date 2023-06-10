using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using NAudio.Wave;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.WPF.Services
{
    public class WindowsAudioService : IAudioService
    {
        public string DefaultAudioDevice { get { return MixItUp.Base.Resources.DefaultOutput; } }
        public string MixItUpOverlay { get { return MixItUp.Base.Resources.MixItUpOverlay; } }

        public async Task Play(string filePath, int volume, string deviceName, bool waitForFinish = false)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                await this.PlayInternal(filePath, null, volume, deviceName, waitForFinish);
            }
        }

        public async Task Play(Stream stream, int volume, string deviceName, bool waitForFinish = false)
        {
            if (stream != null && stream.CanRead)
            {
                await this.PlayInternal(null, stream, volume, deviceName, waitForFinish);
            }
        }

        private async Task PlayInternal(string filePath, Stream stream, int volume, string deviceName, bool waitForFinish)
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
                    var overlayItem = new OverlaySoundItemModel(filePath, volume);
                    //await overlay.ShowItem(overlayItem, new CommandParametersModel());
                }
            }
            else
            {
                Task task = Task.Run(async () =>
                {
                    int deviceNumber = -1;
                    if (!string.IsNullOrEmpty(deviceName))
                    {
                        deviceNumber = this.GetOutputDeviceID(deviceName);
                    }

                    float floatVolume = MathHelper.Clamp(volume, 0, 100) / 100.0f;

                    using (WaveOutEvent outputDevice = (deviceNumber < 0) ? new WaveOutEvent() : new WaveOutEvent() { DeviceNumber = deviceNumber })
                    {
                        WaveStream waveStream = null;
                        if (stream != null)
                        {
                            waveStream = new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(stream)));
                            outputDevice.Volume = floatVolume;
                        }
                        else if (File.Exists(filePath))
                        {
                            AudioFileReader audioFile = new AudioFileReader(filePath);
                            audioFile.Volume = floatVolume;
                            waveStream = audioFile;
                        }
                        else if (filePath.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                        {
                            waveStream = new MediaFoundationReader(filePath);
                            outputDevice.Volume = floatVolume;
                        }

                        if (waveStream != null)
                        {
                            outputDevice.Init(waveStream);
                            outputDevice.Play();

                            while (outputDevice.PlaybackState == PlaybackState.Playing)
                            {
                                await Task.Delay(500);
                            }

                            waveStream.Dispose();
                            if (stream != null)
                            {
                                stream.Dispose();
                            }
                        }
                    }
                });

                if (waitForFinish)
                {
                    await task;
                }
            }
        }

        public IEnumerable<string> GetSelectableAudioDevices()
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
