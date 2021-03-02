using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IStreamingPlatformSessionService
    {
        bool IsConnected { get; }

        Task<Result> ConnectUser(SettingsV3Model settings);
        Task<Result> ConnectBot(SettingsV3Model settings);
        Task<Result> Connect(SettingsV3Model settings);

        Task DisconnectUser(SettingsV3Model settings);
        Task DisconnectBot(SettingsV3Model settings);

        Task<Result> InitializeUser(SettingsV3Model settings);
        Task<Result> InitializeBot(SettingsV3Model settings);

        Task CloseUser(SettingsV3Model settings);
        Task CloseBot(SettingsV3Model settings);

        void SaveSettings(SettingsV3Model settings);

        Task RefreshUser();
        Task RefreshChannel();
    }
}
