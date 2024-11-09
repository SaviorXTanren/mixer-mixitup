using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class MeldService : ServiceBase
    {
        public override string Name => throw new NotImplementedException();

        public override bool IsEnabled => throw new NotImplementedException();

        public override bool IsConnected => throw new NotImplementedException();

        public override Task<Result> Connect()
        {
            throw new NotImplementedException();
        }

        public override Task<Result> Disable()
        {
            throw new NotImplementedException();
        }

        public override Task<Result> Disconnect()
        {
            throw new NotImplementedException();
        }

        public override Task<Result> Enable()
        {
            throw new NotImplementedException();
        }
    }
}
