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

    public interface IXSplitService
    {
        Task<bool> Initialize();

        Task<bool> TestConnection();

        void SetCurrentScene(XSplitScene scene);

        void UpdateSource(XSplitSource source);

        Task Close();
    }
}
