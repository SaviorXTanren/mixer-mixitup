using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;
using YouTube.Base;

namespace MixItUp.Base.Services.YouTube
{
    public class YouTubeSessionService : IStreamingPlatformSessionService
    {
        public YouTubePlatformService UserConnection { get; private set; }
        public YouTubePlatformService BotConnection { get; private set; }
        public Channel Channel { get; private set; }

        public bool IsConnected { get { return this.UserConnection != null; } }

        public async Task<Result> ConnectUser()
        {
            throw new NotImplementedException();
        }

        public async Task<Result> ConnectBot()
        {
            throw new NotImplementedException();
        }

        public async Task<Result> Connect(SettingsV3Model settings)
        {
            throw new NotImplementedException();
        }

        public async Task DisconnectUser(SettingsV3Model settings)
        {
            throw new NotImplementedException();
        }

        public async Task DisconnectBot(SettingsV3Model settings)
        {
            throw new NotImplementedException();
        }

        public async Task<Result> InitializeUser(SettingsV3Model settings)
        {
            throw new NotImplementedException();
        }

        public async Task<Result> InitializeBot(SettingsV3Model settings)
        {
            throw new NotImplementedException();
        }

        public async Task CloseUser()
        {
            throw new NotImplementedException();
        }

        public async Task CloseBot()
        {
            throw new NotImplementedException();
        }

        public void SaveSettings(SettingsV3Model settings)
        {
            throw new NotImplementedException();
        }

        public async Task RefreshUser()
        {
            throw new NotImplementedException();
        }

        public async Task RefreshChannel()
        {
            throw new NotImplementedException();
        }
    }
}
