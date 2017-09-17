using MixItUp.Base.ViewModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class OBSStudioAction : ActionBase
    {
        [DataMember]
        public string SceneCollection { get; set; }

        [DataMember]
        public string SceneName { get; set; }

        [DataMember]
        public string SourceName { get; set; }

        [DataMember]
        public bool SourceVisible { get; set; }

        public OBSStudioAction() { }

        public OBSStudioAction(string sceneCollection, string sceneName) : this(sceneCollection, sceneName, null, false) { }

        public OBSStudioAction(string sourceName, bool sourceVisible) : this(null, null, sourceName, sourceVisible) { }

        private OBSStudioAction(string sceneCollection, string sceneName, string sourceName, bool sourceVisible)
            : base(ActionTypeEnum.OBSStudio)
        {
            this.SceneCollection = sceneCollection;
            this.SceneName = sceneName;
            this.SourceName = sourceName;
            this.SourceVisible = sourceVisible;
        }

        public override Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.OBSWebsocket == null)
            {
                ChannelSession.InitializeOBSWebsocket();
            }

            if (ChannelSession.OBSWebsocket != null)
            {
                if (!string.IsNullOrEmpty(this.SceneCollection))
                {
                    ChannelSession.OBSWebsocket.SetCurrentSceneCollection(this.SceneCollection);
                }

                if (!string.IsNullOrEmpty(this.SceneName))
                {
                    ChannelSession.OBSWebsocket.SetCurrentScene(this.SceneName);
                }

                if (!string.IsNullOrEmpty(this.SourceName))
                {
                    ChannelSession.OBSWebsocket.SetSourceRender(this.SourceName, this.SourceVisible);
                }
            }
            return Task.FromResult(0);
        }
    }
}
