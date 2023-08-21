using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IAudioService
    {
        string DefaultAudioDevice { get; }
        string MixItUpOverlay { get; }

        Task Play(string filePath, int volume);

        Task Play(string filePath, int volume, string deviceName);

        Task PlayNotification(string filePath, int volume);

        IEnumerable<string> GetSelectableAudioDevices(bool includeOverlay = false);

        IEnumerable<string> GetOutputDevices();
    }
}
