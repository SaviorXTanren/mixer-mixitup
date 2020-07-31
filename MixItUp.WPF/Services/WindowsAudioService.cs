using MixItUp.Base;
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
        public async Task Play(string filePath, int volume)
        {
            await ChannelSession.Services.AudioService.Play(filePath, volume, null);
        }

        public Task Play(string filePath, int volume, string deviceName)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                Task.Run(async () =>
                {
                    int deviceNumber = -1;
                    if (!string.IsNullOrEmpty(deviceName))
                    {
                        deviceNumber = this.GetOutputDeviceID(deviceName);
                    }

                    if (deviceNumber < 0 && !string.IsNullOrEmpty(ChannelSession.Settings.DefaultAudioOutput))
                    {
                        deviceNumber = this.GetOutputDeviceID(ChannelSession.Settings.DefaultAudioOutput);
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
            }
            return Task.FromResult(0);
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
