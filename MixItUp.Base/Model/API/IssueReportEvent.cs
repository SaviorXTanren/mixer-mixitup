using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.API
{
    [DataContract]
    public class IssueReportModel
    {
        [DataMember]
        public StreamingPlatformTypeEnum Platform { get; set; } = StreamingPlatformTypeEnum.None;
        [DataMember]
        public string UserID { get; set; }
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string EmailAddress { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string LogContents { get; set; }

        public IssueReportModel() { }
    }
}
