using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IAudioService
    {
        string DefaultAudioDevice { get; }
        string MixItUpOverlay { get; }

        Task Play(string filePath, int volume, string deviceName, bool waitForFinish = false);

        Task Play(Stream stream, int volume, string deviceName, bool waitForFinish = false);

        IEnumerable<string> GetSelectableAudioDevices();

        IEnumerable<string> GetOutputDevices();
    }
}
