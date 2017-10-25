using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IOBSService
    {
        Task<bool> Initialize(string serverIP, string password);

        void SetCurrentSceneCollection(string sceneCollection);

        void SetCurrentScene(string scene);

        void SetSourceRender(string source, bool isVisible);

        Task Close();
    }
}
