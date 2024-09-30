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

    public abstract class ExternalServiceBase : IExternalService
    {
        public abstract string ClientID { get; }

        public abstract string AuthorizationURL { get; }

        public abstract string Name { get; }

        public abstract bool IsEnabled { get; }
        public abstract bool IsConnected { get; }

        public abstract Task<Result> Connect();

        public abstract Task Disconnect();
    }
}