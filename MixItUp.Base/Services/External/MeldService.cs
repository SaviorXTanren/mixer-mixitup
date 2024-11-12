using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class MeldService : ServiceBase
    {
        public override string Name => throw new NotImplementedException();

        public override bool IsEnabled => throw new NotImplementedException();

        public override bool IsConnected { get; protected set; }

        public override Task<Result> Connect()
        {
            throw new NotImplementedException();
        }

        public override Task Disable()
        {
            throw new NotImplementedException();
        }

        public override Task Disconnect()
        {
            throw new NotImplementedException();
        }
    }
}
