using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class XSplitScene
    {
        [DataMember]
        public string sceneName;
    }

    [DataContract]
    public class XSplitSource
    {
        [DataMember]
        public string sourceName;
        [DataMember]
        public bool sourceVisible;
    }

    [DataContract]
    public class XSplitWebBrowserSource : XSplitSource
    {
        [DataMember]
        public string webBrowserUrl;
    }

    public interface IXSplitService
    {
        Task<bool> Initialize();

        Task<bool> TestConnection();

        void SetCurrentScene(XSplitScene scene);

        void SetSourceVisibility(XSplitSource source);

        void SetWebBrowserSource(XSplitWebBrowserSource source);

        Task Close();
    }
}
