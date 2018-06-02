using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IAudioService
    {
        Task Play(string filePath, int volume, int deviceNumber = -1);

        Task<Dictionary<int, string>> GetOutputDevices();

        Task<int> GetOutputDevice(string deviceName);
    }
}
