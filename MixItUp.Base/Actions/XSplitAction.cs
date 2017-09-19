using MixItUp.Base.ViewModel;
using MixItUp.Base.XSplit;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class XSplitAction : ActionBase
    {
        [DataMember]
        public string SceneName { get; set; }

        [DataMember]
        public string SourceName { get; set; }

        [DataMember]
        public bool SourceVisible { get; set; }

        public XSplitAction() { }

        public XSplitAction(string sceneName) : this(sceneName, null, false) { }

        public XSplitAction(string sourceName, bool sourceVisible) : this(null, sourceName, sourceVisible) { }

        private XSplitAction(string sceneName, string sourceName, bool sourceVisible)
            : base(ActionTypeEnum.XSplit)
        {
            this.SceneName = sceneName;
            this.SourceName = sourceName;
            this.SourceVisible = sourceVisible;
        }

        public override Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.XSplitServer == null)
            {
                ChannelSession.InitializeXSplitServer();
            }

            if (ChannelSession.XSplitServer != null)
            {
                if (!string.IsNullOrEmpty(this.SceneName))
                {
                    ChannelSession.XSplitServer.SetCurrentScene(new XSplitScene() { sceneName = this.SceneName });
                }

                if (!string.IsNullOrEmpty(this.SourceName))
                {
                    ChannelSession.XSplitServer.UpdateSource(new XSplitSource() { sourceName = this.SourceName, sourceVisible = this.SourceVisible });
                }
            }
            return Task.FromResult(0);
        }
    }
}
