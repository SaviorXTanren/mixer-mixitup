using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public interface IExternalService
    {
        string Name { get; }

        bool IsConnected { get; }

        Task<Result> Connect();

        Task Disconnect();
    }
}