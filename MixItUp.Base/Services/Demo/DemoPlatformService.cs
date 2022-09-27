using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using StreamingClient.Base.Model.OAuth;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.Users;

namespace MixItUp.Base.Services.Demo
{
    public class DemoPlatformService : StreamingPlatformServiceBase
    {
        public static Task<Result<DemoPlatformService>> Connect(OAuthTokenModel token)
        {
            return Task.FromResult(new Result<DemoPlatformService>(new DemoPlatformService()));
        }

        public static async Task<Result<DemoPlatformService>> ConnectUser()
        {
            return await DemoPlatformService.Connect();
        }

        public static async Task<Result<DemoPlatformService>> ConnectBot()
        {
            return await DemoPlatformService.Connect();
        }

        public static Task<Result<DemoPlatformService>> Connect()
        {
            return Task.FromResult(new Result<DemoPlatformService>(new DemoPlatformService()));
        }

        public override string Name { get { return MixItUp.Base.Resources.DemoConnection; } }

        public DemoPlatformService() { }

        public Task<UserModel> GetCurrentUser() { return Task.FromResult(DemoSessionService.user); }

        public Task<UserModel> GetUserByID(string userID)
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
            else
            {
                user = new UserModel()
                {
                    id = "user" + userID,
                    login = "user" + userID,
                    display_name = "User" + userID,
                    broadcaster_type = "",
                    profile_image_url = "https://raw.githubusercontent.com/SaviorXTanren/mixer-mixitup/master/Branding/MixItUp-Logo-Base-WhiteSM.png",
                };
            }
            return Task.FromResult(user);
        }

        public Task<UserModel> GetUserByLogin(string login)
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
            else
            {
                user = new UserModel()
                {
                    id = login,
                    login = login,
                    display_name = login,
                    broadcaster_type = "",
                    profile_image_url = "https://raw.githubusercontent.com/SaviorXTanren/mixer-mixitup/master/Branding/MixItUp-Logo-Base-WhiteSM.png",
                };
            }
            return Task.FromResult(user);
        }
    }
}
