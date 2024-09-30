using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class DestreamService : IExternalService
    {
        private const string ClientID = "Urhm4REZuka1Naz2zHTKlA==";

        public string Name { get { return Resources.DeStream; } }

        public bool IsConnected { get { return false; } }

        public Task<Result> Connect()
        {
            throw new NotImplementedException();
        }

        public Task Disconnect()
        {
            throw new NotImplementedException();
        }
    }
}
