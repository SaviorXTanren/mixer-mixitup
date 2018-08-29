using MixItUp.Base.Model.Serial;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface ISerialService
    {
        Task<IEnumerable<string>> GetCurrentPortNames();

        Task SendMessage(SerialDeviceModel serialDevice, string message);
    }
}
