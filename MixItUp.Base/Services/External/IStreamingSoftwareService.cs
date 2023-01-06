using MixItUp.Base.Model.Actions;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public interface IStreamingSoftwareService : IExternalService
    {
        bool IsEnabled { get; }

        event EventHandler Connected;
        event EventHandler Disconnected;

        Task<bool> TestConnection();

        Task ShowScene(string sceneName);
        Task<string> GetCurrentScene();

        Task SetSourceVisibility(string sceneName, string sourceName, bool visibility);
        Task SetSourceFilterVisibility(string sourceName, string filterName, bool visibility);
        Task SetImageSourceFilePath(string sceneName, string sourceName, string filePath);
        Task SetMediaSourceFilePath(string sceneName, string sourceName, string filePath);
        Task SetWebBrowserSourceURL(string sceneName, string sourceName, string url);
        Task SetSourceDimensions(string sceneName, string sourceName, StreamingSoftwareSourceDimensionsModel dimensions);
        Task<StreamingSoftwareSourceDimensionsModel> GetSourceDimensions(string sceneName, string sourceName);

        Task StartStopStream();
        Task StartStopRecording();

        Task SaveReplayBuffer();
        Task<bool> StartReplayBuffer();

        Task SetSceneCollection(string sceneCollectionName);
    }

    public interface IOBSStudioService : IStreamingSoftwareService
    {
    }
}
