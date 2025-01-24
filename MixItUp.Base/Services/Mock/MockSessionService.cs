using MixItUp.Base.Model;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Mock
{
    [Obsolete]
    public class MockSessionService : IStreamingPlatformSessionService
    {
        public MockPlatformService UserConnection { get; private set; }
        public MockPlatformService BotConnection { get; private set; }

        public bool IsConnected { get { return this.UserConnection != null; } }
        public bool IsBotConnected { get { return this.BotConnection != null; } }

        public string UserID { get { return "0"; } }
        public string Username { get { return "Test User"; } }
        public string BotID { get { return "1"; } }
        public string Botname { get { return "Test Bot"; } }
        public string ChannelID { get { return "2"; } }
        public string ChannelLink { get { return "https://mixitupapp.com"; } }

        public StreamingPlatformAccountModel UserAccount
        {
            get
            {
                return new StreamingPlatformAccountModel()
                {
                    ID = this.UserID,
                    Username = this.Username,
                    AvatarURL = "https://github.com/SaviorXTanren/mixer-mixitup/raw/master/Branding/MixItUp-Logo-Base-WhiteXS.png"
                };
            }
        }
        public StreamingPlatformAccountModel BotAccount
        {
            get
            {
                return new StreamingPlatformAccountModel()
                {
                    ID = this.BotID,
                    Username = this.Botname,
                    AvatarURL = "https://github.com/SaviorXTanren/mixer-mixitup/raw/master/Branding/MixItUp-Logo-Base-WhiteXS.png"
                };
            }
        }

        public bool IsLive { get { return true; } }

        public int ViewerCount { get { return 999; } }

        public DateTimeOffset StreamStart { get { return this.streamStart; } }
        private DateTimeOffset streamStart = DateTimeOffset.Now;

        public async Task<Result> ConnectUser()
        {
            Result<MockPlatformService> result = await MockPlatformService.ConnectUser();
            if (result.Success)
            {
                this.UserConnection = result.Value;
            }
            return result;
        }

        public async Task<Result> ConnectBot()
        {
            Result<MockPlatformService> result = await MockPlatformService.ConnectBot();
            if (result.Success)
            {
                this.BotConnection = result.Value;
            }
            return result;
        }

        public async Task<Result> Connect(SettingsV3Model settings)
        {
            if (settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Mock].IsEnabled)
            {
                Result userResult = null;

                Result<MockPlatformService> mockResult = await MockPlatformService.Connect(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Mock].UserOAuthToken);
                if (mockResult.Success)
                {
                    this.UserConnection = mockResult.Value;
                    userResult = mockResult;
                }
                else
                {
                    userResult = await this.ConnectUser();
                }

                if (userResult.Success)
                {
                    if (settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Mock].BotOAuthToken != null)
                    {
                        mockResult = await MockPlatformService.Connect(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Mock].BotOAuthToken);
                        if (mockResult.Success)
                        {
                            this.BotConnection = mockResult.Value;
                        }
                        else
                        {
                            return new Result(success: true, message: MixItUp.Base.Resources.ErrorHeader);
                        }
                    }
                }
                else
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Mock].ClearUserData();
                    return userResult;
                }

                return userResult;
            }
            return new Result();
        }

        public async Task DisconnectUser(SettingsV3Model settings)
        {
            await this.DisconnectBot(settings);

            await ServiceManager.Get<MockChatEventService>().DisconnectUser();

            this.UserConnection = null;

            if (settings.StreamingPlatformAuthentications.TryGetValue(StreamingPlatformTypeEnum.Mock, out var streamingPlatform))
            {
                streamingPlatform.ClearUserData();
            }
        }

        public async Task DisconnectBot(SettingsV3Model settings)
        {
            await ServiceManager.Get<MockChatEventService>().DisconnectBot();

            this.BotConnection = null;

            if (settings.StreamingPlatformAuthentications.TryGetValue(StreamingPlatformTypeEnum.Mock, out var streamingPlatform))
            {
                streamingPlatform.ClearBotData();
            }
        }

        public async Task<Result> InitializeUser(SettingsV3Model settings)
        {
            if (this.UserConnection != null)
            {
                try
                {
                    List<Task<Result>> platformServiceTasks = new List<Task<Result>>();
                    platformServiceTasks.Add(ServiceManager.Get<MockChatEventService>().ConnectUser());

                    await Task.WhenAll(platformServiceTasks);

                    if (platformServiceTasks.Any(c => !c.Result.Success))
                    {
                        string errors = string.Join(Environment.NewLine, platformServiceTasks.Where(c => !c.Result.Success).Select(c => c.Result.Message));
                        return new Result(MixItUp.Base.Resources.TwitchFailedToConnectHeader + Environment.NewLine + Environment.NewLine + errors);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    return new Result(MixItUp.Base.Resources.ErrorHeader +
                        Environment.NewLine + Environment.NewLine + MixItUp.Base.Resources.ErrorHeader + ex.Message);
                }
            }
            return new Result();
        }

        public async Task<Result> InitializeBot(SettingsV3Model settings)
        {
            if (this.BotConnection != null)
            {
                Result result = await ServiceManager.Get<MockChatEventService>().ConnectBot();
                if (!result.Success)
                {
                    return result;
                }
            }
            return new Result();
        }

        public async Task CloseUser()
        {
            await ServiceManager.Get<MockChatEventService>().DisconnectUser();
        }

        public async Task CloseBot()
        {
            await ServiceManager.Get<MockChatEventService>().DisconnectBot();
        }

        public void SaveSettings(SettingsV3Model settings)
        {
            if (this.UserConnection != null)
            {
                if (!settings.StreamingPlatformAuthentications.ContainsKey(StreamingPlatformTypeEnum.Mock))
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Mock] = new StreamingPlatformAuthenticationSettingsModel(StreamingPlatformTypeEnum.Mock);
                }

                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Mock].UserOAuthToken = new OAuthTokenModel();
                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Mock].UserID = this.UserID;
                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Mock].ChannelID = this.ChannelID;

                if (this.BotConnection != null)
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Mock].BotOAuthToken = new OAuthTokenModel();
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Mock].BotID = this.BotID;
                }
            }
        }

        public Task RefreshUser()
        {
            return Task.CompletedTask;
        }

        public Task RefreshChannel()
        {
            return Task.CompletedTask;
        }

        public Task<string> GetTitle()
        {
            return Task.FromResult("Test Title");
        }

        public Task<bool> SetTitle(string title) { return Task.FromResult(false); }

        public Task<string> GetGame()
        {
            return Task.FromResult("Test Game");
        }

        public Task<bool> SetGame(string gameName) { return Task.FromResult(false); }

        public async Task AddMockViewerStatistics()
        {
            for (int days = 0; days < 10; days++)
            {
                int lastNumber = RandomHelper.GenerateRandomNumber(100);
                DateTime dateTime = DateTime.Now.Subtract(TimeSpan.FromDays(days));

                ServiceManager.Get<StatisticsService>().LogStatistic(StatisticItemTypeEnum.StreamStart, platform: StreamingPlatformTypeEnum.Twitch, amount: lastNumber, dateTime: dateTime);

                int totalViewerEvents = 25;
                for (int i = 0; i < totalViewerEvents; i++)
                {
                    ServiceManager.Get<StatisticsService>().LogStatistic(StatisticItemTypeEnum.Viewers, platform: StreamingPlatformTypeEnum.Twitch, amount: lastNumber, dateTime: dateTime.AddMinutes(i * 5));
                    lastNumber += Math.Max(RandomHelper.GenerateRandomNumber(-5, 5), 0);
                }

                ServiceManager.Get<StatisticsService>().LogStatistic(StatisticItemTypeEnum.StreamStop, platform: StreamingPlatformTypeEnum.Twitch, amount: lastNumber, dateTime: dateTime.AddMinutes(totalViewerEvents * 5));
            }

            await ChannelSession.SaveSettings();
        }
    }
}
