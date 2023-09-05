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

        Task Play(string filePath, int volume, bool track = true);

        Task Play(string filePath, int volume, string deviceName, bool track = true);

        Task Play(string filePath, int volume, string deviceName, bool waitForFinish = false);

        Task PlayNotification(string filePath, int volume, bool track = true);

        Task PlayMP3(Stream stream, int volume, string deviceName, bool waitForFinish = false);

        Task PlayPCM(Stream stream, int volume, string deviceName, bool waitForFinish = false);

        Task StopAllSounds();

        IEnumerable<string> GetSelectableAudioDevices(bool includeOverlay = false);

        IEnumerable<string> GetOutputDevices();
    }
}
