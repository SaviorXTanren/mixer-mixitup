using MixItUp.Base.Actions;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IOBSService
    {
        event EventHandler Connected;
        event EventHandler Disconnected;

        Task<bool> Initialize(string serverIP, string password);

        StreamingSourceDimensions GetSourceDimensions(string source);

        void SetCurrentSceneCollection(string sceneCollection);

        void SetCurrentScene(string scene);

        void SetSourceRender(string source, bool isVisible);

        void SetWebBrowserSource(string source, string url);

        void SetSourceDimensions(string source, StreamingSourceDimensions dimensions);

        void StartEndStream();

        Task Close();
    }
}
