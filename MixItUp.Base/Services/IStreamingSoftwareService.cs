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

        Task SetSourceVisibility(string sceneName, string sourceName, bool visibility);
        Task SetWebBrowserSourceURL(string sceneName, string sourceName, string url);
        Task SetSourceDimensions(string sceneName, string sourceName, StreamingSourceDimensions dimensions);
        Task<StreamingSourceDimensions> GetSourceDimensions(string sceneName, string sourceName);

        Task StartStopStream();

        Task SaveReplayBuffer();
        Task<bool> StartReplayBuffer();

        Task SetSceneCollection(string sceneCollectionName);
    }
}
