using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.API
{
    [DataContract]
    public class IssueReportEvent
    {
        [JsonProperty]
        public uint MixerUserID { get; set; }
        [JsonProperty]
        public string Description { get; set; }
        [JsonProperty]
        public string LogContents { get; set; }

        public IssueReportEvent() { }
    }
}
