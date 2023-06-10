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
                if (this.MixItUpOverlay.Equals(deviceName))
                {
                    OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpointService();
                    if (overlay != null)
                    {
                        var overlayItem = new OverlaySoundItemModel(filePath, volume);
                        //await overlay.ShowItem(overlayItem, new CommandParametersModel());
                    }
                }
                else if (File.Exists(filePath))
                {
                    //audioFile.Volume = this.GetAdjustedVolume(volume);
                    await this.PlayInternal(new AudioFileReader(filePath), volume, deviceName, waitForFinish);
                }
                else if (filePath.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                {
                    await this.PlayInternal(new MediaFoundationReader(filePath), volume, deviceName, waitForFinish);
                }
            }
        }

        public async Task PlayMP3(Stream stream, int volume, string deviceName, bool waitForFinish = false)
        {
            if (stream != null && stream.CanRead)
            {
                await this.PlayInternal(new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(stream))), volume, deviceName, waitForFinish, stream);
            }
        }

        public async Task PlayPCM(Stream stream, int volume, string deviceName, bool waitForFinish = false)
        {
            if (stream != null && stream.CanRead)
            {
                await this.PlayInternal(new BlockAlignReductionStream(new RawSourceWaveStream(stream, new WaveFormat())), volume, deviceName, waitForFinish, stream);
            }
        }

        private async Task PlayInternal(WaveStream stream, int volume, string deviceName, bool waitForFinish, Stream initialStream = null)
        {
            if (stream != null)
            {
                if (string.IsNullOrEmpty(deviceName))
                {
                    deviceName = ChannelSession.Settings.DefaultAudioOutput;
                }

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
                        outputDevice.Volume = floatVolume;
                        outputDevice.Init(stream);
                        outputDevice.Play();

                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            await Task.Delay(500);
                        }

                        stream.Dispose();
                        if (initialStream != null)
                        {
                            stream.Dispose();
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

        private float GetAdjustedVolume(int volume) { return ((float)MathHelper.Clamp(volume, 0, 100)) / 100.0f; }
    }
}
