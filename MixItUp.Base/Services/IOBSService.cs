using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IOBSService
    {
        Task<bool> Initialize(string serverIP, string password);

        void SetCurrentSceneCollection(string sceneCollection);

        void SetCurrentScene(string scene);

        void SetSourceRender(string source, bool isVisible);

        void SetWebBrowserSource(string source, string url);

        Task Close();
    }
}
