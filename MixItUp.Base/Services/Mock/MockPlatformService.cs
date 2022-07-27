using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using StreamingClient.Base.Model.OAuth;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.Users;

namespace MixItUp.Base.Services.Mock
{
    public class MockPlatformService : TwitchPlatformService
    {
        public new static Task<Result<MockPlatformService>> Connect(OAuthTokenModel token)
        {
            return Task.FromResult(new Result<MockPlatformService>(new MockPlatformService()));
        }

        public new static async Task<Result<MockPlatformService>> ConnectUser()
        {
            return await MockPlatformService.Connect();
        }

        public new static async Task<Result<MockPlatformService>> ConnectBot()
        {
            return await MockPlatformService.Connect();
        }

        public static Task<Result<MockPlatformService>> Connect()
        {
            return Task.FromResult(new Result<MockPlatformService>(new MockPlatformService()));
        }

        public override string Name { get { return MixItUp.Base.Resources.MockConnection; } }

        public MockPlatformService() : base(null) { }

        public new Task<UserModel> GetNewAPICurrentUser() { return Task.FromResult(MockSessionService.user); }

        public new Task<UserModel> GetNewAPIUserByID(string userID)
        {
            UserModel user = null;
            if (string.Equals(MockSessionService.user.id, userID))
            {
                user = MockSessionService.user;
            }
            else if (string.Equals(MockSessionService.bot.id, userID))
            {
                user = MockSessionService.bot;
            }
            return Task.FromResult(user);
        }

        public new Task<UserModel> GetNewAPIUserByLogin(string login)
        {
            UserModel user = null;
            if (string.Equals(MockSessionService.user.login, login))
            {
                user = MockSessionService.user;
            }
            else if (string.Equals(MockSessionService.bot.login, login))
            {
                user = MockSessionService.bot;
            }
            return Task.FromResult(user);
        }
    }
}
