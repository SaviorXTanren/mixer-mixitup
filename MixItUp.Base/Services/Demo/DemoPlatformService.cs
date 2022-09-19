using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using StreamingClient.Base.Model.OAuth;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.Users;

namespace MixItUp.Base.Services.Mock
{
    public class DemoPlatformService : StreamingPlatformServiceBase
    {
        public new static Task<Result<DemoPlatformService>> Connect(OAuthTokenModel token)
        {
            return Task.FromResult(new Result<DemoPlatformService>(new DemoPlatformService()));
        }

        public new static async Task<Result<DemoPlatformService>> ConnectUser()
        {
            return await DemoPlatformService.Connect();
        }

        public new static async Task<Result<DemoPlatformService>> ConnectBot()
        {
            return await DemoPlatformService.Connect();
        }

        public static Task<Result<DemoPlatformService>> Connect()
        {
            return Task.FromResult(new Result<DemoPlatformService>(new DemoPlatformService()));
        }

        public override string Name { get { return MixItUp.Base.Resources.MockConnection; } }

        public DemoPlatformService() : base(null) { }

        public new Task<UserModel> GetNewAPICurrentUser() { return Task.FromResult(DemoSessionService.user); }

        public new Task<UserModel> GetNewAPIUserByID(string userID)
        {
            UserModel user = null;
            if (string.Equals(DemoSessionService.user.id, userID))
            {
                user = DemoSessionService.user;
            }
            else if (string.Equals(DemoSessionService.bot.id, userID))
            {
                user = DemoSessionService.bot;
            }
            return Task.FromResult(user);
        }

        public new Task<UserModel> GetNewAPIUserByLogin(string login)
        {
            UserModel user = null;
            if (string.Equals(DemoSessionService.user.login, login))
            {
                user = DemoSessionService.user;
            }
            else if (string.Equals(DemoSessionService.bot.login, login))
            {
                user = DemoSessionService.bot;
            }
            return Task.FromResult(user);
        }
    }
}
