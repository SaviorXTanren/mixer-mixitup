using System;
using System.Net.WebSockets;
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
        event EventHandler<WebSocketCloseStatus> OnWebSocketDisconnectOccurred;

        Task<bool> Initialize();

        Task<bool> TestConnection();

        Task SetCurrentScene(XSplitScene scene);

        Task SetSourceVisibility(XSplitSource source);

        Task SetWebBrowserSource(XSplitWebBrowserSource source);

        Task DisconnectServer(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure);
    }
}
