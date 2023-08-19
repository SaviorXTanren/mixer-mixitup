using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class LumiaStreamService : IExternalService
    {
        private const string BaseAddress = "http://localhost:39231/api/";

        public LumiaStreamService() { }

        public string Name { get { return MixItUp.Base.Resources.LumiaStream; } }

        public bool IsConnected { get; private set; }

        public Task<Result> Connect()
        {
            ServiceManager.Get<ITelemetryService>().TrackService("Lumia Stream");
            return Task.FromResult(new Result());
        }

        public Task Disconnect()
        {
            return Task.CompletedTask;
        }
    }
}
