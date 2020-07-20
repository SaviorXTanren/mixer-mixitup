using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IAudioService
    {
        Task Play(string filePath, int volume);

        Task Play(string filePath, int volume, string deviceName);

        IEnumerable<string> GetOutputDevices();
    }
}
