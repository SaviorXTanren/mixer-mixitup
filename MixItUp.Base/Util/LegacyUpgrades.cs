using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    [Obsolete]
    public class OBSStudioAction : ActionBase
    {
        protected override SemaphoreSlim AsyncSemaphore { get { return null; } }

        [DataMember]
        public string SceneCollection { get; set; }
        [DataMember]
        public string SceneName { get; set; }

        [DataMember]
        public string SourceName { get; set; }
        [DataMember]
        public bool SourceVisible { get; set; }

        [DataMember]
        public string SourceText { get; set; }

        [DataMember]
        public string SourceURL { get; set; }

        [DataMember]
        public StreamingSourceDimensions SourceDimensions { get; set; }

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments) { throw new NotImplementedException(); }
    }

    [DataContract]
    [Obsolete]
    public class XSplitAction : ActionBase
    {
        protected override SemaphoreSlim AsyncSemaphore { get { return null; } }

        [DataMember]
        public string SceneName { get; set; }

        [DataMember]
        public string SourceName { get; set; }
        [DataMember]
        public bool SourceVisible { get; set; }

        [DataMember]
        public string SourceText { get; set; }

        [DataMember]
        public string SourceURL { get; set; }

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments) { throw new NotImplementedException(); }
    }

    [DataContract]
    [Obsolete]
    public class StreamlabsOBSAction : ActionBase
    {
        protected override SemaphoreSlim AsyncSemaphore { get { return null; } }

        [DataMember]
        public string SceneName { get; set; }

        [DataMember]
        public string SourceName { get; set; }
        [DataMember]
        public bool SourceVisible { get; set; }

        [DataMember]
        public string SourceText { get; set; }

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments) { throw new NotImplementedException(); }
    }
}
