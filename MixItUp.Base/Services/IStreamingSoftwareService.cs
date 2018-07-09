using MixItUp.Base.Actions;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IStreamingSoftwareService
    {
        event EventHandler Connected;
        event EventHandler Disconnected;

        Task<bool> Connect();
        Task Disconnect();

        Task<bool> TestConnection();

        Task ShowScene(string sceneName);

        Task SetSourceVisibility(string sourceName, bool visibility);
        Task SetWebBrowserSourceURL(string sourceName, string url);
        Task SetSourceDimensions(string sourceName, StreamingSourceDimensions dimensions);
        Task<StreamingSourceDimensions> GetSourceDimensions(string sourceName);

        Task StartStopStream();

        Task SaveReplayBuffer();
    }
}
