using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum ServiceState
    {
        Disabled,
        Enabled,
        Connected,
        Disconnected,
    }

    public interface IService
    {
        ServiceState State { get; }

        string Name { get; }

        Task<Result> Enable();

        Task<Result> Connect();

        Task<Result> Disconnect();

        Task<Result> Disable();
    }

    public abstract class ServiceBase : IService
    {
        public ServiceState State { get; protected set; }

        public abstract string Name { get; }

        public abstract Task<Result> Enable();

        public abstract Task<Result> Connect();

        public abstract Task<Result> Disconnect();

        public abstract Task<Result> Disable();
    }
}
