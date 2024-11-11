using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IService
    {
        string Name { get; }

        bool IsEnabled { get; }

        bool IsConnected { get; }

        Task<Result> Connect();

        Task<Result> Disconnect();
    }

    public abstract class ServiceBase : IService
    {
        public abstract string Name { get; }

        public abstract bool IsEnabled { get; }

        public abstract bool IsConnected { get; }

        public abstract Task<Result> Connect();

        public abstract Task<Result> Disconnect();
    }
}
