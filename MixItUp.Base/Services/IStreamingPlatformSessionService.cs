using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IStreamingPlatformSessionService
    {
        bool IsConnected { get; }

        Task<Result> ConnectUser();
        Task<Result> ConnectBot();
        Task<Result> Connect(SettingsV3Model settings);

        Task DisconnectUser(SettingsV3Model settings);
        Task DisconnectBot(SettingsV3Model settings);

        Task<Result> InitializeUser(SettingsV3Model settings);
        Task<Result> InitializeBot(SettingsV3Model settings);

        Task CloseUser();
        Task CloseBot();

        void SaveSettings(SettingsV3Model settings);

        Task RefreshUser();
        Task RefreshChannel();

        Task<string> GetTitle();
        Task<bool> SetTitle(string title);

        Task<string> GetGame();
        Task<bool> SetGame(string gameName);
    }
}
