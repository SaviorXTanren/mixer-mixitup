using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IAudioService
    {
        Task Play(string filePath, int volume, int deviceNumber = -1);

        Dictionary<int, string> GetOutputDevices();

        int GetOutputDevice(string deviceName);
    }
}
