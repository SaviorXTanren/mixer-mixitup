using MixItUp.Base;
using MixItUp.Base.Model.Commands;
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

        public async Task Play(string filePath, int volume)
        {
            await ServiceManager.Get<IAudioService>().Play(filePath, volume, null);
        }

        public async Task Play(string filePath, int volume, string deviceName)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                if (string.IsNullOrEmpty(deviceName))
                {
                    deviceName = ChannelSession.Settings.DefaultAudioOutput;
                }

                if (this.MixItUpOverlay.Equals(deviceName))
                {
                    OverlayEndpointService overlay = ServiceManager.Get<OverlayService>().GetOverlay(ServiceManager.Get<OverlayService>().DefaultOverlayName);
                    if (overlay != null)
                    {
                        var overlayItem = new OverlaySoundItemModel(filePath, volume);
                        await overlay.ShowItem(overlayItem, new CommandParametersModel());
                    }
                }
                else
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(async () =>
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
                            if (File.Exists(filePath))
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
                            }
                        }
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
