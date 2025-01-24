using MixItUp.Base.Util;
using MixItUp.Base.Model.Web;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Mock
{
    public class MockPlatformService : StreamingPlatformServiceBase
    {
        public static Task<Result<MockPlatformService>> Connect(OAuthTokenModel token)
        {
            return Task.FromResult(new Result<MockPlatformService>(new MockPlatformService()));
        }

        public static async Task<Result<MockPlatformService>> ConnectUser()
        {
            return await MockPlatformService.Connect();
        }

        public static async Task<Result<MockPlatformService>> ConnectBot()
        {
            return await MockPlatformService.Connect();
        }

        public static Task<Result<MockPlatformService>> Connect()
        {
            return Task.FromResult(new Result<MockPlatformService>(new MockPlatformService()));
        }

        public override string Name { get { return MixItUp.Base.Resources.MockConnection; } }

        public MockPlatformService() { }
    }
}
