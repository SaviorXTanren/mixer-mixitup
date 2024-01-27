using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    /// <summary>
    /// https://docs.pulsoid.net
    /// </summary>
    public class PulsoidService : ExternalServiceBase
    {
        public override string ClientID { get { return "21c57a62-d890-4f3d-8879-4b1171d51858"; } }

        public override string AuthorizationURL { get { return $"https://pulsoid.net/oauth2/authorize?response_type=token&client_id={this.ClientID}&redirect_uri=http://localhost:8919/&scope=data:heart_rate:read&state={Guid.NewGuid()}"; } }

        public override string Name => throw new NotImplementedException();

        public override bool IsEnabled => throw new NotImplementedException();

        public override bool IsConnected => throw new NotImplementedException();

        public override Task<Result> Connect()
        {
            throw new NotImplementedException();
        }

        public override Task Disconnect()
        {
            throw new NotImplementedException();
        }
    }
}
