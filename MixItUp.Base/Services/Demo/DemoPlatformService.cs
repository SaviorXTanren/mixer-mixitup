using MixItUp.Base.Model;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using StreamingClient.Base.Model.OAuth;
using System;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.Users;

namespace MixItUp.Base.Services.Demo
{
    public class DemoPlatformService : StreamingPlatformServiceBase
    {
        public static async Task<SettingsV3Model> CreateDemoSettings()
        {
            SettingsV3Model settings = new SettingsV3Model("Demo");
            settings.ID = new Guid("00000000-0000-0000-0000-000000000001");

            await ServiceManager.Get<IDatabaseService>().Write(settings.DatabaseFilePath, "DELETE FROM Commands");
            await ServiceManager.Get<IDatabaseService>().Write(settings.DatabaseFilePath, "DELETE FROM Quotes");
            await ServiceManager.Get<IDatabaseService>().Write(settings.DatabaseFilePath, "DELETE FROM Users");

#pragma warning disable CS0612 // Type or member is obsolete
            settings.DefaultStreamingPlatform = StreamingPlatformTypeEnum.Demo;
            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Demo] = new StreamingPlatformAuthenticationSettingsModel()
            {
                Type = StreamingPlatformTypeEnum.Demo,
                UserID = ServiceManager.Get<DemoSessionService>().UserID,
                BotID = ServiceManager.Get<DemoSessionService>().BotID,
                ChannelID = ServiceManager.Get<DemoSessionService>().ChannelID,
                UserOAuthToken = new OAuthTokenModel(),
                BotOAuthToken = new OAuthTokenModel(),
            };
#pragma warning restore CS0612 // Type or member is obsolete

            return settings;
        }

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
