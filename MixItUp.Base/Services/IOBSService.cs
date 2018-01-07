using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class OBSSourceDimensions
    {
        public int X;
        public int Y;
        public int Rotation;
        public float XScale;
        public float YScale;
    }

    public interface IOBSService
    {
        event EventHandler Disconnected;

        Task<bool> Initialize(string serverIP, string password);

        OBSSourceDimensions GetSourceDimensions(string source);

        void SetCurrentSceneCollection(string sceneCollection);

        void SetCurrentScene(string scene);

        void SetSourceRender(string source, bool isVisible);

        void SetWebBrowserSource(string source, string url);

        void SetSourceDimensions(string source, OBSSourceDimensions dimensions);

        Task Close();
    }
}
