using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IAudioService
    {
        string DefaultAudioDevice { get; }
        string MixItUpOverlay { get; }

        ISet<string> ApplicableAudioFileExtensions { get; }

        Task Play(string filePath, int volume, string deviceName, bool waitForFinish = false);

        Task PlayMP3Stream(Stream stream, int volume, string deviceName, bool waitForFinish = false);

        Task PlayPCMStream(Stream stream, int volume, string deviceName, bool waitForFinish = false);

        Task PlayOnOverlay(string filePath, double volume, bool waitForFinish = false);

        Task PlayNotification(string filePath, int volume);

        Task StopAllSounds();

        void OverlaySoundFinished(Guid id);

        IEnumerable<string> GetSelectableAudioDevices(bool includeOverlay = false);

        IEnumerable<string> GetOutputDevices();

        string GetOutputDeviceName(string deviceName);
    }
}
